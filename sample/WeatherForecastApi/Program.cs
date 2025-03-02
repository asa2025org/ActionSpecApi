using ASA.Core;
using ASA.Host;
using WeatherForecastApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActionSpecApi("./asa.yaml");

// Custom modules
builder.Services.AddSingleton<IModule, WeatherGenerator>();

var app = builder.Build();

app.UseActionSpecApi();

app.Run();
