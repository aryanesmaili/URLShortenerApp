using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using URLShortener.Application;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Models;
using URLShortener.Infrastructure;
using URLShortener.Infrastructure.Utility;
using URLShortener.Persistence;
using URLShortenerAPI;
using URLShortenerAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApiServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    var authorizationSettings = services.GetRequiredService<IOptions<AuthorizationSettings>>().Value;
    await IdentitySeeder.SeedRoles(roleManager, authorizationSettings);
}

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
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapApiEndpoints();

app.Use(async (context, next) =>
{
    // Check if the request is for an API endpoint
    if (!context.Request.Path.StartsWithSegments("/api") ||
        context.Request.Path.ToString().Contains("Hub", StringComparison.Ordinal) ||
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
