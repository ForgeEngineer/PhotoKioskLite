using System.Threading.Channels;

namespace PhotoKioskLite.Api.Photos;

public sealed class ThumbnailQueue
{
    private readonly Channel<int> _channel = Channel.CreateBounded<int>(
        new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });

    // Best effort: if the queue is full, skip it. The UI requests
    // visible thumbnails on demand anyway, so nothing is lost.
    public bool TryEnqueue(int photoId) => _channel.Writer.TryWrite(photoId);

    public IAsyncEnumerable<int> ReadAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}