namespace PhotoKioskLite.Api.Photos;

// Consumer only. The device watcher is now the producer.
public sealed class ThumbnailWorker(
    ThumbnailQueue queue,
    ThumbnailService thumbnails,
    ILogger<ThumbnailWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var done = 0;

        await foreach (var id in queue.ReadAllAsync(ct))
        {
            await thumbnails.GetOrCreateAsync(id);

            if (++done % 50 == 0)
                logger.LogInformation("Warmed {Count} thumbnails", done);
        }
    }
}