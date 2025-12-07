using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Audit;

public class SecurityEvent
{
    public int EventId { get; set; }
    public SecurityEventType EventType { get; set; }
    public int? UserId { get; set; }
    public Severity Severity { get; set; }
    public string Description { get; set; } = null!;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime EventDate { get; set; } = DateTime.Now;
    public bool Resolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedBy { get; set; }
    
    public virtual User? User { get; set; }
    public virtual User? ResolvedByUser { get; set; }
}