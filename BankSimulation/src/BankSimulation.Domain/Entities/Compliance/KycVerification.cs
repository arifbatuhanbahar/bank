using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Compliance;

public class KycVerification
{
    public int VerificationId { get; set; }
    
    public int UserId { get; set; }
    
    public VerificationType VerificationType { get; set; }
    
    public VerificationMethod VerificationMethod { get; set; }
    
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    
    public string? VerificationCode { get; set; } // SMS/Email kodu
    
    public DateTime? VerifiedAt { get; set; }
    
    public int Attempts { get; set; } // Deneme sayısı
    
    public DateTime ExpiresAt { get; set; }
    
    // Navigation Property
    public virtual User User { get; set; } = null!;
}