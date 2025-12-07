using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Fraud;

public class RiskProfile
{
    public int ProfileId { get; set; }
    
    public int UserId { get; set; }
    
    public ProfileRiskLevel RiskLevel { get; set; }
    
    public int TransactionVelocityScore { get; set; } // Hız puanı
    
    public int AmountAnomalyScore { get; set; } // Tutar puanı
    
    public int GeographicRiskScore { get; set; } // Konum puanı
    
    public int BehavioralScore { get; set; } // Davranış puanı
    
    public DateTime LastCalculatedAt { get; set; } = DateTime.Now;
    
    public string? Factors { get; set; } // JSON (Puanı etkileyen faktörler)
    
    // Navigation Property
    public virtual User User { get; set; } = null!;
}