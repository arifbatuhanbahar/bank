using BankSimulation.Domain.Entities.AccountManagement;
using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.TransactionManagement;

/// <summary>
/// Düzenli / Planlanmış İşlemler (Otomatik Ödeme)
/// </summary>
public class ScheduledTransaction
{
    public int ScheduleId { get; set; }
    
    public int UserId { get; set; }
    
    public int FromAccountId { get; set; }
    
    public int ToAccountId { get; set; }
    
    public decimal Amount { get; set; }
    
    public Frequency Frequency { get; set; } // Günlük, Aylık vb.
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public DateTime NextExecutionDate { get; set; }
    
    public DateTime? LastExecutionDate { get; set; }
    
    // GÜNCELLENEN KISIM: Enum kullanıldı
    public ScheduledTransactionStatus Status { get; set; } = ScheduledTransactionStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Account FromAccount { get; set; } = null!;
    public virtual Account ToAccount { get; set; } = null!;
}