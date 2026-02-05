namespace Smart.ProxyPilot.Models;

public class ProxyPoolSnapshot()
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ValidatingCount { get; set; }
    public int AvailableCount { get; set; }
    public int InUseCount { get; set; }
    public int CooldownCount { get; set; }
    public int DisabledCount { get; set; }

    public long TotalValidations { get; set; }
    public long SuccessfulValidations { get; set; }
    public long FailedValidations { get; set; }

    public long TotalGetRequests { get; set; }
    public long SuccessfulGetRequests { get; set; }
    public long WaitingGetRequests { get; set; }

    public double AvgValidationTime { get; set; }
    public double AvgResponseTime { get; set; }
    public double OverallSuccessRate { get; set; }
}
