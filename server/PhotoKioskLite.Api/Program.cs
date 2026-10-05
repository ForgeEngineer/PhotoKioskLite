using PhotoKioskLite.Api.Devices;
using PhotoKioskLite.Api.Photos;
using PhotoKioskLite.Api.Printing;
using PhotoKioskLite.Api.Realtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddPhotos();
builder.Services.AddPrinting();
builder.Services.AddDevices();

var app = builder.Build();

// Production: serve the built Angular app from wwwroot (same origin, no proxy)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPhotoEndpoints();
app.MapHub<KioskHub>("/hubs/kiosk");

// Any other route goes to Angular
app.MapFallbackToFile("index.html");

app.Run();