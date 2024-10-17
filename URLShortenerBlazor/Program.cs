using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using URLShortenerBlazor;
using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using URLShortenerBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddBlazoredLocalStorage();
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services
    .AddBlazorise(options => options.Immediate = true)
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

builder.Services.AddScoped<HTTTPAuthAdder>();

builder.Services.AddHttpClient("Auth", client => client.BaseAddress = new Uri("https://Pexita.Click"))

    .AddHttpMessageHandler<HTTTPAuthAdder>();

await builder.Build().RunAsync();
