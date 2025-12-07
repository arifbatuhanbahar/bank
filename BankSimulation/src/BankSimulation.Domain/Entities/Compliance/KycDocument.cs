using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Compliance;

public class KycDocument
{
    public int DocumentId { get; set; }
    
    public int UserId { get; set; }
    
    public DocumentType DocumentType { get; set; }
    
    public string DocumentNumber { get; set; } = null!; // TC No, Pasaport No vb.
    
    public string DocumentFilePath { get; set; } = null!; // Dosyanın sunucudaki yolu
    
    public string? DocumentHash { get; set; } // Dosyanın değiştirilmediğini kanıtlamak için
    
    public DateTime? ExpiryDate { get; set; }
    
    public DateTime UploadDate { get; set; } = DateTime.Now;
    
    public DateTime? VerifiedAt { get; set; }
    
    public int? VerifiedBy { get; set; } // Onaylayan personel
    
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    
    public string? RejectionReason { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? VerifiedByUser { get; set; }
}