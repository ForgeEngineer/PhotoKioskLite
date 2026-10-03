namespace PhotoKioskLite.Api.Devices;

public interface IDeviceWatcher
{
    DeviceState Current { get; }
}