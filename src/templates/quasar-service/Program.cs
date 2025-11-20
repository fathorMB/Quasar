using Quasar.Logging;
using Quasar.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseQuasarSerilog(configuration: builder.Configuration, sectionName: "Logging");
builder.Services.AddQuasarWeb();

var app = builder.Build();
app.MapGet("/", () => Results.Ok(new { message = "Hello from Quasar" }));
app.Run();
