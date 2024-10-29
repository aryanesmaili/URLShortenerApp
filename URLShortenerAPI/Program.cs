using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.Settings;
using URLShortenerAPI.Responses.MapperConfigs;
using URLShortenerAPI.Services;
using URLShortenerAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

builder.Services.AddSingleton<IRedisQueueService, RedisQueueService>();

builder.Services.AddHostedService<ClickProcessService>(); // Background Service.

builder.Services.AddAutoMapper(typeof(UserMapper));
builder.Services.AddAutoMapper(typeof(AnalyticsMapper));
builder.Services.AddAutoMapper(typeof(URLCategoryMapper));
builder.Services.AddAutoMapper(typeof(URLMapper));
builder.Services.AddAutoMapper(typeof(TokenMapper));


builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLDocker")));

// Add the SMTP service to be able to send emails
builder.Services.Configure<SMTPSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("http://localhost:5083")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // Important for cookies/authentication
    });
});


// Add redis Service
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:9191,password=a123";
    options.InstanceName = string.Empty;
});
builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect("localhost:9191,password=a123"));


JwtSettings jwtSettings = new();
builder.Configuration.Bind(nameof(JwtSettings), jwtSettings);
builder.Services.AddSingleton(jwtSettings);
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey!);

builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwtBearer =>
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
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("AllUsers", policy => policy.RequireRole("Admin", "ChannelAdmin"));

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

app.UseAuthorization();

app.MapControllers();

app.Run();
