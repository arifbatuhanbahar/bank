namespace BankSimulation.Domain.Entities.UserManagement;

/// <summary>
/// Kullanıcı oturum tablosu
/// </summary>
public class UserSession
{
    public int SessionId { get; set; }
    
    public int UserId { get; set; }
    
    public string SessionToken { get; set; } = null!;
    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Property
    public virtual User User { get; set; } = null!;
}