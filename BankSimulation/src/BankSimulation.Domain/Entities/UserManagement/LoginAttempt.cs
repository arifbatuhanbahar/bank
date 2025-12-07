namespace BankSimulation.Domain.Entities.UserManagement;

/// <summary>
/// Giriş denemeleri tablosu
/// </summary>
public class LoginAttempt
{
    public int AttemptId { get; set; }
    
    public int? UserId { get; set; }
    
    public string EmailAttempted { get; set; } = null!;
    
    public string? IpAddress { get; set; }
    
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    
    public DateTime AttemptedAt { get; set; }
    public string? UserAgent { get; set; }
    
    // Navigation Property
    public virtual User? User { get; set; }
}