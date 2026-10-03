namespace PhotoKioskLite.Api.Printing;

public abstract record PrinterState(string Status);
public sealed record Ready() : PrinterState("ready");
public sealed record Printing(string JobId, int Progress) : PrinterState("printing");
public sealed record PrinterError(string Code, string Message) : PrinterState("error");