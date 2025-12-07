using BankSimulation.Domain.Entities.TransactionManagement;
using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Fraud;

public class FraudAlert
{
    public int AlertId { get; set; }
    
    public int UserId { get; set; }
    
    public int TransactionId { get; set; }
    
    public int FraudScore { get; set; } // O an hesaplanan risk puanı
    
    public string? TriggeredRules { get; set; } // Hangi kurallar tetiklendi? (Array/String)
    
    public AlertSeverity AlertSeverity { get; set; }
    
    public AlertStatus Status { get; set; } = AlertStatus.Open;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? ReviewedAt { get; set; }
    
    public int? ReviewedBy { get; set; }
    
    public string? ResolutionNotes { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Transaction Transaction { get; set; } = null!;
    public virtual User? ReviewedByUser { get; set; }
}