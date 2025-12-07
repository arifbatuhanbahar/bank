using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.UserManagement;

/// <summary>
/// Kullanıcı rolleri tablosu
/// </summary>
public class UserRole
{
    public int RoleId { get; set; }
    
    public int UserId { get; set; }
    
    public RoleName RoleName { get; set; }
    
    public DateTime AssignedAt { get; set; }
    
    public int? AssignedBy { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? AssignedByUser { get; set; }
}