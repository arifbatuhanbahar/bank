using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.AccountManagement;

/// <summary>
/// Hesap limitleri
/// </summary>
public class AccountLimit
{
    public int LimitId { get; set; }
    
    public int AccountId { get; set; }
    
    public LimitType LimitType { get; set; }
    
    public decimal LimitAmount { get; set; }
    
    public decimal UsedAmount { get; set; }
    
    public DateTime ResetDate { get; set; }
    
    public DateTime? LastUpdated { get; set; }
    
    // Navigation Property
    public virtual Account Account { get; set; } = null!;
}
