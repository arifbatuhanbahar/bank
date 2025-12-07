using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Audit;

public class PciAuditLog
{
    public int LogId { get; set; }
    public int UserId { get; set; }
    public AuditAction Action { get; set; }
    public string CardToken { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? IpAddress { get; set; }
    public bool Success { get; set; }
    public string? Reason { get; set; }
    
    public virtual User User { get; set; } = null!;
}