using Microsoft.AspNetCore.SignalR;
using PhotoKioskLite.Api.Photos;
using PhotoKioskLite.Api.Realtime;

namespace PhotoKioskLite.Api.Devices;

public sealed class UsbDeviceWatcher(
    PhotoLibrary library,
    ThumbnailQueue queue,
    ThumbnailService thumbnails,
    IHubContext<KioskHub, IKioskClient> hub,
    ILogger<UsbDeviceWatcher> logger) : BackgroundService, IDeviceWatcher
{
    private const int BatchSize = 100;

    private static readonly EnumerationOptions Walk = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
    };

    private sealed record UsbDrive(string Root, string Label);

    private volatile DeviceState _current = new NoDevice();
    public DeviceState Current => _current;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        UsbDrive? active = null;
        CancellationTokenSource? scan = null;

        // Polling once a second: simple and robust. Production on Linux
        // would listen to udev events instead.
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        do
        {
            var drive = FindDrive();
            if (drive?.Root == active?.Root) continue;

            // Removed (or swapped for another drive)
            if (active is not null)
            {
                scan?.Cancel();
                scan?.Dispose();
                scan = null;

                library.Reset();
                thumbnails.PurgeCache();   // the next customer must never see these photos
                await PublishAsync(new NoDevice());
                logger.LogInformation("Device removed: {Label}", active.Label);
            }

            active = drive;

            if (drive is not null)
            {
                logger.LogInformation("Device connected: {Label} at {Root}", drive.Label, drive.Root);
                var generation = library.Reset();
                scan = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var token = scan.Token;
                _ = Task.Run(() => ScanAsync(drive, generation, token), token);
            }
        }
        while (await timer.WaitForNextTickAsync(ct));
    }

    private async Task ScanAsync(UsbDrive drive, int generation, CancellationToken ct)
    {
        try
        {
            await PublishAsync(new Scanning(drive.Label, 0), ct);

            var batch = new List<string>(BatchSize);
            var total = 0;

            foreach (var file in EnumeratePhotos(drive.Root))
            {
                ct.ThrowIfCancellationRequested();
                batch.Add(file);
                if (batch.Count < BatchSize) continue;

                total += await FlushAsync(batch, generation, ct);
                await PublishAsync(new Scanning(drive.Label, total), ct);
            }

            total += await FlushAsync(batch, generation, ct);
            await PublishAsync(new Connected(drive.Label, total), ct);
            logger.LogInformation("Found {Count} photos on {Label}", total, drive.Label);
        }
        catch (OperationCanceledException)
        {
            // Device removed mid-scan: expected
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scan failed on {Label}", drive.Label);
            await PublishAsync(new DeviceError($"Could not read {drive.Label}"), ct);
        }
    }

    // Stream photos to the UI in batches while scanning continues
    private async Task<int> FlushAsync(List<string> batch, int generation, CancellationToken ct)
    {
        if (batch.Count == 0) return 0;

        var added = library.AddRange(generation, batch);
        batch.Clear();
        if (added.Count == 0 || ct.IsCancellationRequested) return 0;

        await hub.Clients.All.PhotosAdded(added);

        foreach (var photo in added)
            queue.TryEnqueue(photo.Id);

        return added.Count;
    }

    private Task PublishAsync(DeviceState state, CancellationToken ct = default)
    {
        // A cancelled scan must not overwrite the "none" state
        if (ct.IsCancellationRequested) return Task.CompletedTask;

        _current = state;
        return hub.Clients.All.DeviceStateChanged(state);
    }

    private static IEnumerable<string> EnumeratePhotos(string root) =>
        Directory.EnumerateFiles(root, "*", Walk)
            .Where(f => !Path.GetFileName(f).StartsWith("._")   // macOS metadata files
                     && (f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                      || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)));

    private static UsbDrive? FindDrive()
    {
        if (OperatingSystem.IsWindows())
        {
            var drive = DriveInfo.GetDrives()
                .FirstOrDefault(d => d.DriveType == DriveType.Removable && d.IsReady);

            return drive is null
                ? null
                : new UsbDrive(
                    drive.RootDirectory.FullName,
                    string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name : drive.VolumeLabel);
        }

        // Raspberry Pi OS automounts USB drives at /media/<user>/<label>
        var media = Path.Combine("/media", Environment.UserName);
        if (!Directory.Exists(media)) return null;

        var mount = Directory.EnumerateDirectories(media).FirstOrDefault();
        return mount is null ? null : new UsbDrive(mount, Path.GetFileName(mount));
    }
}