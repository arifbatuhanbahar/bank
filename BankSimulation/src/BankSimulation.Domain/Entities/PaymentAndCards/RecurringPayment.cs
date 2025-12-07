using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.PaymentAndCards;

/// <summary>
/// Düzenli Ödemeler (Abonelikler)
/// </summary>
public class RecurringPayment
{
    public int RecurringId { get; set; }
    
    public int UserId { get; set; }
    
    public int PaymentMethodId { get; set; }
    
    public string MerchantName { get; set; } = null!;
    
    public decimal Amount { get; set; }
    
    public Frequency Frequency { get; set; } // Aylık, Yıllık
    
    public DateTime NextPaymentDate { get; set; }
    
    public DateTime? LastPaymentDate { get; set; }
    
    public ScheduledTransactionStatus Status { get; set; } = ScheduledTransactionStatus.Active;
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}