using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.PaymentAndCards;

/// <summary>
/// Kredi Kartı Harcamaları / İşlemleri
/// </summary>
public class CardTransaction
{
    public int CardTransactionId { get; set; }
    
    public int CardId { get; set; }
    
    public string MerchantName { get; set; } = null!; // Üye İşyeri (Örn: Netflix, Migros)
    
    public string MerchantCategory { get; set; } = null!; // MCC Kodu veya Kategori
    
    public decimal Amount { get; set; }
    
    public Currency Currency { get; set; }
    
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    
    public CardTransactionStatus Status { get; set; } = CardTransactionStatus.Pending;
    
    public string AuthorizationCode { get; set; } = null!; // Provizyon Kodu
    
    public string? IpAddress { get; set; }
    
    public string? Location { get; set; }
    
    // Navigation Property
    public virtual CreditCard CreditCard { get; set; } = null!;
}