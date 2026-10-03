using PhotoKioskLite.Api.Printing;

namespace PhotoKioskLite.Api.Printing;

public interface IPrinter
{
    PrinterState Current { get; }
}