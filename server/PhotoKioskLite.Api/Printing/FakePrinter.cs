using Microsoft.AspNetCore.SignalR;
using PhotoKioskLite.Api.Realtime;

namespace PhotoKioskLite.Api.Printing;

public sealed class FakePrinter(
    IHubContext<KioskHub, IKioskClient> hub,
    ILogger<FakePrinter> logger) : BackgroundService, IPrinter
{
    private volatile PrinterState _current = new Ready();
    public PrinterState Current => _current;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Publish(new Ready());
            await Task.Delay(3000, ct);

            var jobId = Guid.NewGuid().ToString("N")[..8];
            for (var progress = 0; progress <= 100; progress += 10)
            {
                await Publish(new Printing(jobId, progress));
                await Task.Delay(300, ct);

                if (progress == 50 && Random.Shared.Next(5) == 0)
                {
                    await Publish(new PrinterError("RIBBON_END", "Ribbon empty, replace media"));
                    await Task.Delay(4000, ct);
                    break;
                }
            }
        }
    }

    private Task Publish(PrinterState state)
    {
        _current = state;
        logger.LogDebug("Printer -> {Status}", state.Status);
        return hub.Clients.All.PrinterStateChanged(state);
    }
}