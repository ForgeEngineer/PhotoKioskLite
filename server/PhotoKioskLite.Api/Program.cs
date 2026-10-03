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

app.MapPhotoEndpoints();
app.MapHub<KioskHub>("/hubs/kiosk");

app.Run();