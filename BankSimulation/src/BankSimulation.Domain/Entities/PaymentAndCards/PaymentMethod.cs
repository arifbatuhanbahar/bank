using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.PaymentAndCards;

/// <summary>
/// Kayıtlı Ödeme Yöntemleri
/// </summary>
public class PaymentMethod
{
    public int PaymentMethodId { get; set; }
    
    public int UserId { get; set; }
    
    public PaymentMethodType MethodType { get; set; }
    
    // Tokenize edilmiş kart bilgisi (Güvenlik için)
    public string CardToken { get; set; } = null!;
    
    public string CardLastFour { get; set; } = null!;
    
    public CardBrand CardBrand { get; set; }
    
    public int ExpiryMonth { get; set; }
    
    public int ExpiryYear { get; set; }
    
    public string CardHolderName { get; set; } = null!;
    
    public bool IsDefault { get; set; }
    
    public CardStatus Status { get; set; } = CardStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<RecurringPayment> RecurringPayments { get; set; } = new List<RecurringPayment>();
}