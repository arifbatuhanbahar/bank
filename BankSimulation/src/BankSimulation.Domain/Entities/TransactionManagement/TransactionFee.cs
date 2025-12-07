using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.TransactionManagement;

/// <summary>
/// İşlem ücretleri (Komisyon, BSMV vb.)
/// </summary>
public class TransactionFee
{
    public int FeeId { get; set; }
    
    public int TransactionId { get; set; }
    
    public string FeeType { get; set; } = null!; // "Commission", "Tax", "Service"
    
    public decimal FeeAmount { get; set; }
    
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Property
    public virtual Transaction Transaction { get; set; } = null!;
}