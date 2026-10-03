namespace PhotoKioskLite.Api.Devices;

public static class DeviceSetup
{
    public static IServiceCollection AddDevices(this IServiceCollection services)
    {
        services.AddSingleton<UsbDeviceWatcher>();
        services.AddSingleton<IDeviceWatcher>(sp => sp.GetRequiredService<UsbDeviceWatcher>());
        services.AddHostedService(sp => sp.GetRequiredService<UsbDeviceWatcher>());
        return services;
    }
}