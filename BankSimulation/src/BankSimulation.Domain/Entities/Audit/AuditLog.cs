using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Audit;

public class AuditLog
{
    public int LogId { get; set; }
    public string TableName { get; set; } = null!;
    public string RecordId { get; set; } = null!; // PK değeri
    public AuditAction Action { get; set; }
    public string? OldValue { get; set; } // JSON
    public string? NewValue { get; set; } // JSON
    public string? ChangedFields { get; set; }
    public int? ChangedBy { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}