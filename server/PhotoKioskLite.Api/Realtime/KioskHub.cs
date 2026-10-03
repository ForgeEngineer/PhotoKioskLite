using Microsoft.AspNetCore.SignalR;
using PhotoKioskLite.Api.Devices;
using PhotoKioskLite.Api.Printing;

namespace PhotoKioskLite.Api.Realtime;

public sealed class KioskHub(IPrinter printer, IDeviceWatcher device) : Hub<IKioskClient>
{
    // Late joiners and reconnects get the current state immediately
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.PrinterStateChanged(printer.Current);
        await Clients.Caller.DeviceStateChanged(device.Current);
    }
}