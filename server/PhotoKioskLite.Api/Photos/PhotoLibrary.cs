namespace PhotoKioskLite.Api.Photos;

// The photos of the currently connected device. Thread-safe: the scanner
// writes while API requests read.
public sealed class PhotoLibrary
{
    private readonly Lock _gate = new();
    private readonly List<PhotoDto> _photos = [];
    private readonly Dictionary<int, string> _paths = [];
    private int _nextId;
    private int _generation;
    private string _session = NewSession();

    public IReadOnlyList<PhotoDto> Snapshot()
    {
        lock (_gate) return _photos.ToArray();
    }

    public string? GetPath(int id)
    {
        lock (_gate) return _paths.GetValueOrDefault(id);
    }

    // Starts a new device session. Returns a generation number so a scan
    // from an already-removed device can't add stale photos.
    public int Reset()
    {
        lock (_gate)
        {
            _photos.Clear();
            _paths.Clear();
            _session = NewSession();
            return ++_generation;
        }
    }

    public IReadOnlyList<PhotoDto> AddRange(int generation, IEnumerable<string> files)
    {
        lock (_gate)
        {
            if (generation != _generation) return [];

            var added = new List<PhotoDto>();
            foreach (var file in files)
            {
                var id = _nextId++;
                _paths[id] = file;

                // Session in the URL: the browser cache can never show
                // a previous customer's thumbnail
                var photo = new PhotoDto(id, $"/api/photos/{id}/thumb?s={_session}");
                _photos.Add(photo);
                added.Add(photo);
            }
            return added;
        }
    }

    private static string NewSession() => Guid.NewGuid().ToString("N")[..8];
}