using Microsoft.AspNetCore.Mvc;
using PhotoKioskLite.Api.Photos;

namespace PhotoKioskLite.Api.Controllers;

[ApiController]
[Route("api/photos")]
public sealed class PhotosController(PhotoLibrary library, ThumbnailService thumbnails) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<PhotoDto>> GetAll() => Ok(library.Snapshot());

    [HttpGet("{id:int}/thumb")]
    public async Task<IActionResult> GetThumbnail(int id)
    {
        var path = await thumbnails.GetOrCreateAsync(id);
        if (path is null) return NotFound();

        Response.Headers.CacheControl = "private, max-age=3600";
        // Absolute path on disk: PhysicalFile, not File (which resolves against wwwroot)
        return PhysicalFile(path, "image/jpeg");
    }
}
