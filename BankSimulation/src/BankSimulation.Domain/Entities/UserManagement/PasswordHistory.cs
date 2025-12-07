namespace BankSimulation.Domain.Entities.UserManagement;

/// <summary>
/// Şifre geçmişi tablosu
/// </summary>
public class PasswordHistory
{
    public int HistoryId { get; set; }
    
    public int UserId { get; set; }
    
    public string PasswordHash { get; set; } = null!;
    
    public DateTime ChangedAt { get; set; }
    
    public int? ChangedBy { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? ChangedByUser { get; set; }
}
