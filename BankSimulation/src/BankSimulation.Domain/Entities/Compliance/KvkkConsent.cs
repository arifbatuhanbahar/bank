using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Compliance;

public class KvkkConsent
{
    public int ConsentId { get; set; }
    
    public int UserId { get; set; }
    
    public ConsentType ConsentType { get; set; }
    
    public bool ConsentGiven { get; set; } // Evet/Hayır
    
    public string ConsentText { get; set; } = null!; // Onaylanan metin
    
    public string ConsentVersion { get; set; } = "v1.0";
    
    public DateTime GrantedAt { get; set; } = DateTime.Now;
    
    public DateTime? RevokedAt { get; set; } // İptal tarihi
    
    public string? IpAddress { get; set; }
    
    // Navigation Property
    public virtual User User { get; set; } = null!;
}