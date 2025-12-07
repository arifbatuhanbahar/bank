using BankSimulation.Domain.Entities.CreditCardApplications; // Yeni eklediğimiz using
using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.PaymentAndCards;

/// <summary>
/// Kredi Kartları Tablosu
/// </summary>
public class CreditCard
{
    public int CardId { get; set; }
    
    public int UserId { get; set; }
    
    // Güvenlik gereği şifreli saklanır
    public string CardNumberEncrypted { get; set; } = null!;
    
    // Maskelenmiş numara (Örn: **** **** **** 1234)
    public string CardLastFour { get; set; } = null!;
    
    public CardType CardType { get; set; } // Sanal/Fiziksel
    
    public CardBrand CardBrand { get; set; } // Visa/Master
    
    public decimal CreditLimit { get; set; }
    
    public decimal AvailableLimit { get; set; }
    
    public decimal CurrentBalance { get; set; } // Güncel Borç
    
    public decimal MinimumPayment { get; set; }
    
    public DateTime PaymentDueDate { get; set; } // Son ödeme tarihi
    
    public decimal InterestRate { get; set; }
    
    public int ExpiryMonth { get; set; }
    
    public int ExpiryYear { get; set; }
    
    // CVV şifreli saklanır
    public string CvvEncrypted { get; set; } = null!;
    
    public CardStatus Status { get; set; } = CardStatus.Active;
    
    public DateTime IssuedAt { get; set; } = DateTime.Now;
    
    public DateTime? ActivatedAt { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<CardTransaction> CardTransactions { get; set; } = new List<CardTransaction>();
    
    // YENİ EKLENEN SATIR: Kartın limitleri
    public virtual ICollection<CardLimit> CardLimits { get; set; } = new List<CardLimit>();
}