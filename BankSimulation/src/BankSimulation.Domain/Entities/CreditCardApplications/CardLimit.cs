using BankSimulation.Domain.Entities.PaymentAndCards;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.CreditCardApplications;

public class CardLimit
{
    public int LimitId { get; set; }
    
    public int CardId { get; set; }
    
    public CardLimitType LimitType { get; set; }
    
    public decimal LimitAmount { get; set; }
    
    public decimal UsedAmount { get; set; }
    
    public DateTime? ResetDate { get; set; } // Limit ne zaman sıfırlanacak? (Örn: Ekstre kesim tarihi)
    
    // Navigation Property
    public virtual CreditCard CreditCard { get; set; } = null!;
}