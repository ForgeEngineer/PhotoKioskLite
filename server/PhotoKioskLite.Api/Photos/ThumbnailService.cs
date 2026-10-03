using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace PhotoKioskLite.Api.Photos;

public sealed class ThumbnailService
{
    private const int ThumbSize = 400;
    private static readonly JpegEncoder Encoder = new() { Quality = 75 };

    private readonly PhotoLibrary _library;
    private readonly ILogger<ThumbnailService> _logger;
    private readonly string _cacheDir;

    // Leave one core free for the web server
    private readonly SemaphoreSlim _gate = new(Math.Max(1, Environment.ProcessorCount - 1));

    // If two requests ask for the same thumbnail at once, only generate it once
    private readonly ConcurrentDictionary<string, Lazy<Task<string?>>> _inFlight = new();

    public ThumbnailService(PhotoLibrary library, IConfiguration config, ILogger<ThumbnailService> logger)
    {
        _library = library;
        _logger = logger;
        _cacheDir = config["Photos:CacheFolder"] ?? Path.Combine(Path.GetTempPath(), "kiosk-thumbs");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task<string?> GetOrCreateAsync(int id)
    {
        if (_library.GetPath(id) is not { } source) return null;

        var target = Path.Combine(_cacheDir, CacheKey(source) + ".jpg");
        if (File.Exists(target)) return target;

        var work = _inFlight.GetOrAdd(target,
            _ => new Lazy<Task<string?>>(() => CreateAsync(source, target)));
        try
        {
            return await work.Value;
        }
        finally
        {
            _inFlight.TryRemove(new KeyValuePair<string, Lazy<Task<string?>>>(target, work));
        }
    }

    // Customer privacy: called when a device is removed
    public void PurgeCache()
    {
        foreach (var file in Directory.EnumerateFiles(_cacheDir))
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "Could not delete cached thumbnail {File}", file);
            }
        }
    }

    private async Task<string?> CreateAsync(string source, string target)
    {
        await _gate.WaitAsync();
        try
        {
            // Decode at reduced size: a 12MP JPEG never fully lands in memory
            var options = new DecoderOptions { TargetSize = new Size(ThumbSize * 2, ThumbSize * 2) };
            using var image = await Image.LoadAsync(options, source);

            image.Mutate(x => x
                .AutoOrient()
                .Resize(new ResizeOptions
                {
                    Size = new Size(ThumbSize, ThumbSize),
                    Mode = ResizeMode.Crop,
                }));

            // Write to a temp file, then move: a half-written thumbnail is never served
            var temp = target + ".tmp";
            await image.SaveAsJpegAsync(temp, Encoder);
            File.Move(temp, target, overwrite: true);
            return target;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create thumbnail for {Source}", source);
            return null;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static string CacheKey(string source)
    {
        var stamp = File.GetLastWriteTimeUtc(source).Ticks;
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{source}|{stamp}"));
        return Convert.ToHexString(hash)[..16];
    }
}