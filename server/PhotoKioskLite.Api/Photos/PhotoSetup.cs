namespace PhotoKioskLite.Api.Photos;

public static class PhotoSetup
{
    public static IServiceCollection AddPhotos(this IServiceCollection services)
    {
        services.AddSingleton<PhotoLibrary>();
        services.AddSingleton<ThumbnailService>();
        services.AddSingleton<ThumbnailQueue>();
        services.AddHostedService<ThumbnailWorker>();
        return services;
    }
}
