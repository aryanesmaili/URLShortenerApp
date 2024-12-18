using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.Settings;
using URLShortenerAPI.Data.Interfaces.Infra;
using URLShortenerAPI.Data.Interfaces.URL;
using URLShortenerAPI.Data.Interfaces.User;
using URLShortenerAPI.Responses.MapperConfigs;
using URLShortenerAPI.Services.Infra;
using URLShortenerAPI.Services.URL;
using URLShortenerAPI.Services.User;
using URLShortenerAPI.Services.Utility;
using URLShortenerAPI.Utility.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}


builder.Configuration.AddUserSecrets<Program>();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Automatically adds all validators of this project to DI pool.
var assembly = typeof(Program).Assembly;
builder.Services.AddValidatorsFromAssembly(assembly);

// Add services to the container.

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IIPInfoService, IPInfoService>();

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IURLService, URLService>();
builder.Services.AddTransient<IShortenerService, ShortenerService>();
builder.Services.AddTransient<IRedirectService, RedirectService>();
builder.Services.AddTransient<ICacheService, CacheService>();
builder.Services.AddTransient<IUserAgentService, UserAgentService>();
builder.Services.AddTransient<IZibalService, ZibalService>();
builder.Services.AddTransient<IPaymentService, PaymentService>();

builder.Services.AddSingleton<IRedisQueueService, RedisQueueService>();
builder.Services.AddSingleton<UserConnectionMapping>();

builder.Services.AddHostedService<ClickProcessService>(); // Background Service.

builder.Services.AddAutoMapper(typeof(UserMapper));
builder.Services.AddAutoMapper(typeof(AnalyticsMapper));
builder.Services.AddAutoMapper(typeof(URLCategoryMapper));
builder.Services.AddAutoMapper(typeof(URLMapper));
builder.Services.AddAutoMapper(typeof(TokenMapper));

string postgresConnectionString = builder.Environment.IsDevelopment() ? "PostgreSQLDockerDev" : "PostgreSQLDockerProd";
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString(postgresConnectionString)));

// Add the SMTP service to be able to send emails
builder.Services.Configure<SMTPSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder
                .WithOrigins("https://localhost:7112")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();  // Important for cookies/authentication
        });
    });
else
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder
                .WithOrigins("http://localhost:80", "http://shortenerfront:80") // nginx serves on port 80
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();  // Important for cookies/authentication
        });
    });

// Add redis Service
string redisHost = (builder.Environment.IsDevelopment() ? "localhost:9191" : "RedisCache:6379") + ",password=a123";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisHost;
    options.InstanceName = string.Empty;
});
builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(redisHost));



JwtSettings jwtSettings = new();
builder.Configuration.Bind(nameof(JwtSettings), jwtSettings);
builder.Services.AddSingleton(jwtSettings);
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey!);

builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwtBearer =>
{
    jwtBearer.RequireHttpsMetadata = true;
    jwtBearer.SaveToken = true;
    jwtBearer.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
    };

    // Read JWT from HttpOnly cookie
    jwtBearer.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["jwt"];
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("AllUsers", policy => policy.RequireRole("Admin", "ChannelAdmin", "TelegramBot"))
    .AddPolicy("TelegramBot", policy => policy.RequireRole("Admin", "TelegramBot"));

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.SameSite = builder.Environment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("Auth", context =>
        RateLimitPartition.GetSlidingWindowLimiter("Auth", key => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 15,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4
        }));

    options.AddPolicy("DataFetch", context =>
        RateLimitPartition.GetSlidingWindowLimiter("DataFetch", key => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4
        }));

    options.AddPolicy("AddURL", context =>
    RateLimitPartition.GetSlidingWindowLimiter("AddURL", key => new SlidingWindowRateLimiterOptions
    {
        PermitLimit = 20,
        Window = TimeSpan.FromMinutes(1),
        SegmentsPerWindow = 4
    }
    ));

    options.AddPolicy("UpdateUser", context =>
        RateLimitPartition.GetFixedWindowLimiter("UpdateUser", key => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1)
        }));

    options.AddPolicy("Deletion", context =>
        RateLimitPartition.GetSlidingWindowLimiter("Deletion", key => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4
        }));
});


builder.Services.AddSignalR();

if (builder.Environment.IsProduction())
    builder.WebHost.UseUrls("http://0.0.0.0:5261");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    // no authorization since cross-origin requests do not contain http-only cookies that have our JWTs in them
    app.MapHub<UserBalanceHub>("api/User/BalanceHub");
    app.MapHub<UserURLsHub>("api/User/UrlCountsHub");
}
else if (app.Environment.IsProduction())
{
    app.MapHub<UserBalanceHub>("api/User/BalanceHub").RequireAuthorization("AllUsers");
    app.MapHub<UserURLsHub>("api/User/UrlCountsHub").RequireAuthorization("AllUsers");
}


app.Use(async (context, next) =>
{
    // Check if the request is for an API endpoint
    if (!context.Request.Path.StartsWithSegments("/api") ||
        (context.Request.Path.ToString().Contains("Hub")) ||
        HttpMethods.IsGet(context.Request.Method))
    {
        await next();
        return;
    }

    // Get the endpoint being accessed
    Endpoint? endpoint = context.GetEndpoint();
    if (endpoint != null)
    {
        // Check if the endpoint has the IgnoreAntiforgeryToken attribute
        bool ignoreAntiforgery = endpoint.Metadata.GetMetadata<IgnoreAntiforgeryTokenAttribute>() != null;
        if (ignoreAntiforgery)
        {
            await next();
            return;
        }
    }

    // Validate the antiforgery token
    try
    {
        await context.RequestServices
            .GetRequiredService<IAntiforgery>()
            .ValidateRequestAsync(context);
        await next();
    }
    catch (AntiforgeryValidationException)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }
});
app.Run();
