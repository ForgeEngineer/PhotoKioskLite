namespace PhotoKioskLite.Api.Photos;

public static class PhotoEndpoints
{
    public static IServiceCollection AddPhotos(this IServiceCollection services)
    {
        services.AddSingleton<PhotoLibrary>();
        services.AddSingleton<ThumbnailService>();
        services.AddSingleton<ThumbnailQueue>();
        services.AddHostedService<ThumbnailWorker>();
        return services;
    }

    public static IEndpointRouteBuilder MapPhotoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/photos");

        group.MapGet("/", (PhotoLibrary library) => library.Snapshot());

        group.MapGet("/{id:int}/thumb", async (int id, ThumbnailService thumbnails, HttpContext http) =>
        {
            var path = await thumbnails.GetOrCreateAsync(id);
            if (path is null) return Results.NotFound();

            http.Response.Headers.CacheControl = "private, max-age=3600";
            return Results.File(path, "image/jpeg");
        });

        return app;
    }
}