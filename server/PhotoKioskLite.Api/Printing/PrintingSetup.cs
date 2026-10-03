namespace PhotoKioskLite.Api.Printing;

public static class PrintingSetup
{
    public static IServiceCollection AddPrinting(this IServiceCollection services)
    {
        // One instance, exposed as both the interface and a hosted service
        services.AddSingleton<FakePrinter>();
        services.AddSingleton<IPrinter>(sp => sp.GetRequiredService<FakePrinter>());
        services.AddHostedService(sp => sp.GetRequiredService<FakePrinter>());
        return services;
    }
}