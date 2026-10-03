namespace PhotoKioskLite.Api.Devices;

public abstract record DeviceState(string Status);
public sealed record NoDevice() : DeviceState("none");
public sealed record Scanning(string Label, int Found) : DeviceState("scanning");
public sealed record Connected(string Label, int PhotoCount) : DeviceState("connected");
public sealed record DeviceError(string Message) : DeviceState("error");