using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.UserManagement;

/// <summary>
/// Ana kullanıcı tablosu - Tüm banka müşterilerinin ve çalışanlarının bilgilerini tutar
/// </summary>
public class User
{
    public int UserId { get; set; }
    
    // TC Kimlik - 11 haneli, UNIQUE
    public string TcKimlikNo { get; set; } = null!;
    
    // Kişisel Bilgiler
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    
    // İletişim Bilgileri
    public string Email { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public string? Phone { get; set; }
    public bool PhoneVerified { get; set; }
    
    // Adres Bilgileri
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "TR";
    
    // Güvenlik Bilgileri
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public DateTime? PasswordChangedAt { get; set; }
    
    // Hesap Durumu
    public UserStatus Status { get; set; } = UserStatus.Active;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    
    // KYC & Risk Bilgileri
    public KycStatus KycStatus { get; set; } = KycStatus.Pending;
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
    public bool IsPep { get; set; }
    
    // Audit Bilgileri
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Soft Delete & GDPR
    public DateTime? DeletedAt { get; set; }
    public bool GdprAnonymized { get; set; }
    
    // Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public virtual ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
    public virtual ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();
    
    // Self-referencing navigation
    public virtual ICollection<UserRole> AssignedRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<PasswordHistory> ChangedPasswords { get; set; } = new List<PasswordHistory>();
}