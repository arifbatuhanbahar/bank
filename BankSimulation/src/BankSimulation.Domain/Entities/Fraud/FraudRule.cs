using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Fraud;

public class FraudRule
{
    public int RuleId { get; set; }
    
    public string RuleName { get; set; } = null!;
    
    public RuleType RuleType { get; set; }
    
    public string? RuleDescription { get; set; }
    
    // Kuralın teknik detayları (JSON) - Örn: {"threshold": 5000, "time_window_minutes": 5}
    public string RuleConditions { get; set; } = null!;
    
    // Bu kural ihlal edilirse risk skoruna kaç puan eklensin? (1-100)
    public int RiskScoreWeight { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? UpdatedAt { get; set; }
}