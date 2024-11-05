using Blazored.LocalStorage;
using Blazored.Toast;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using URLShortenerBlazor;
using URLShortenerBlazor.Services;
using URLShortenerBlazor.Services.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddBlazoredLocalStorage();
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7167") });
builder.Services.AddHttpClient("Auth", client => client.BaseAddress = new Uri("https://localhost:7167"))
    .AddHttpMessageHandler<HTTPAuthAdder>();

builder.Services.AddScoped<CustomAuthProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddTransient<IShortenerService, ShortenerService>();
builder.Services.AddTransient<IProfileServices, ProfileServices>();
builder.Services.AddTransient<IRedirectService, RedirectService>();
builder.Services.AddTransient<IProfileDashboardService, ProfileDashboardService>();
builder.Services.AddTransient<IProfileSettingsService, ProfileSettingsService>();

builder.Services
    .AddBlazorise(options => options.Immediate = true)
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

builder.Services.AddBlazoredToast();

builder.Services.AddScoped<HTTPAuthAdder>();


await builder.Build().RunAsync();
