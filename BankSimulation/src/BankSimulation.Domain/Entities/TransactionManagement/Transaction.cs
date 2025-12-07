using BankSimulation.Domain.Entities.AccountManagement;
using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.TransactionManagement;

/// <summary>
/// Ana işlem tablosu (Para transferleri, ödemeler vb.)
/// </summary>
public class Transaction
{
    public int TransactionId { get; set; }
    
    // Gönderen Hesap (Para yatırmada null olabilir)
    public int? FromAccountId { get; set; }
    
    // Alıcı Hesap (Para çekmede null olabilir)
    public int? ToAccountId { get; set; }
    
    public decimal Amount { get; set; }
    
    public Currency Currency { get; set; }
    
    public TransactionType TransactionType { get; set; }
    
    public string? Description { get; set; }
    
    // Her işlem için benzersiz referans no (GUID veya özel format)
    public string ReferenceNumber { get; set; } = null!;
    
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    
    // Fraud (Dolandırıcılık) Kontrol Alanları
    public int FraudScore { get; set; } // 0-100 arası
    public string? FraudFlags { get; set; } // JSON formatında uyarılar
    public bool RequiresReview { get; set; }
    
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    // Zaman Damgaları
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // İşlemi yapan kullanıcı (Sistemsel ise null olabilir)
    public int? CreatedBy { get; set; }
    
    // Güvenlik Logları
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    public bool IsSuspicious { get; set; }
    public string? SuspiciousReason { get; set; }
    
    public bool ReportedToMasak { get; set; }
    public DateTime? MasakReportDate { get; set; }

    // Navigation Properties
    public virtual Account? FromAccount { get; set; }
    public virtual Account? ToAccount { get; set; }
    public virtual User? ReviewedByUser { get; set; }
    public virtual User? CreatedByUser { get; set; }
    
    // Alt tablolarla ilişkiler
    public virtual ICollection<TransactionFee> TransactionFees { get; set; } = new List<TransactionFee>();
    public virtual ICollection<TransactionApproval> TransactionApprovals { get; set; } = new List<TransactionApproval>();
    public virtual ICollection<GeneralLedger> GeneralLedgerEntries { get; set; } = new List<GeneralLedger>();
}