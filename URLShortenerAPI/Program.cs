using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Utility.SignalR;
using URLShortener.Infrastructure;
using URLShortener.Infrastructure.Services.User;
using URLShortener.Persistence;
using URLShortenerAPI.Data;
using URLShortenerAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

#region DI Container Modification
// Adding Creds to DI Container
builder.Services.RegisterCreds(builder.Configuration);

// Adding services to DI Container
builder.Services.AddServices();

// Adding Repositories to DI Container
builder.Services.AddRepositories();

#endregion
builder.Configuration.AddUserSecrets<Program>();

// Automatically adds all validators of this project to DI pool.
var assembly = typeof(Program).Assembly;
builder.Services.AddValidatorsFromAssembly(assembly);

string postgresConnectionString = builder.Environment.IsDevelopment() ? "PostgreSQLDockerDev" : "PostgreSQLDockerProd";
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString(postgresConnectionString)));

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(postgresConnectionString)));

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

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionMiddleware>();

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
    app.MapHub<UserHub>("api/User/UserHub");
}
else if (app.Environment.IsProduction())
{
    app.MapHub<UserHub>("api/User/UserHub").RequireAuthorization("AllUsers");
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
