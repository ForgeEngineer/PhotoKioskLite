using PhotoKioskLite.Api.Devices;
using PhotoKioskLite.Api.Photos;
using PhotoKioskLite.Api.Printing;
using PhotoKioskLite.Api.Realtime;

var builder = WebApplication.CreateBuilder(args);

// Keep 404s with an empty body, like the Minimal API version (no ProblemDetails)
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o => o.SuppressMapClientErrors = true);
builder.Services.AddSignalR();
builder.Services.AddPhotos();
builder.Services.AddPrinting();
builder.Services.AddDevices();

var app = builder.Build();

// Production: serve the built Angular app from wwwroot (same origin, no proxy)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<KioskHub>("/hubs/kiosk");

// Any other route goes to Angular
app.MapFallbackToFile("index.html");

app.Run();