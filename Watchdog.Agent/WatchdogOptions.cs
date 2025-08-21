namespace Watchdog.Agent;
public class ServiceInfo
{
    public string ProcessName { get; set; } = string.Empty;
    public string Decsription { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public bool? IsUIProcess { get; set; }
    public int CheckInterval { get; set; } = 1000;
    public int CheckIntervalAfterException { get; set; } = 5000;    
}

public class WatchdogOptions
{
    public string Name { get; set; } = string.Empty;
    public List<ServiceInfo> WatchList { get; set; } = new();
}