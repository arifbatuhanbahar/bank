using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Audit;

public class DataAccessLog
{
    public int LogId { get; set; }
    public int AccessedByUserId { get; set; }
    public int TargetUserId { get; set; }
    public DataType DataType { get; set; }
    public string? AccessReason { get; set; }
    public DateTime AccessTimestamp { get; set; } = DateTime.Now;
    public string? IpAddress { get; set; }
    public bool IsSensitive { get; set; }
    
    public virtual User AccessedByUser { get; set; } = null!;
    public virtual User TargetUser { get; set; } = null!;
}