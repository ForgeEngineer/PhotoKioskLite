using PhotoKioskLite.Api.Devices;
using PhotoKioskLite.Api.Photos;
using PhotoKioskLite.Api.Printing;

namespace PhotoKioskLite.Api.Realtime;

public interface IKioskClient
{
    Task PrinterStateChanged(PrinterState state);
    Task DeviceStateChanged(DeviceState state);
    Task PhotosAdded(IReadOnlyList<PhotoDto> photos);
}