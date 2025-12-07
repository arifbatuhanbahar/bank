namespace BankSimulation.Domain.Enums;

public enum AuditAction
{
    Insert,
    Update,
    Delete,
    Select,
    ViewCard,      // Kart Görüntüleme
    AddCard,       // Kart Ekleme
    ModifyCard     // Kart Düzenleme
}

public enum SecurityEventType
{
    LoginFailed,        // Başarısız Giriş
    AccountLocked,      // Hesap Kilitlendi
    PasswordChanged,    // Şifre Değişti
    SuspiciousActivity, // Şüpheli Aktivite
    UnusualIpAddress    // Farklı IP
}

public enum Severity
{
    Low,
    Medium,
    High,
    Critical
}

public enum DataType
{
    PersonalInfo,   // Kimlik, Adres
    Financial,      // Bakiye, Maaş
    Transaction,    // Transferler
    Document        // KYC Belgeleri
}

public enum KeyType
{
    TDE,            // Transparent Data Encryption
    Column,         // Kolon Bazlı
    Backup          // Yedekleme Anahtarı
}

public enum KeyStatus
{
    Active,
    Rotated,        // Değiştirilmiş
    Expired         // Süresi Dolmuş
}