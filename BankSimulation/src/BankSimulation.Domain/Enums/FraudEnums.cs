namespace BankSimulation.Domain.Enums;

/// <summary>
/// Dolandırıcılık Kural Türü
/// </summary>
public enum RuleType
{
    Velocity,       // Hız (Örn: Dakikada 5 işlem)
    AmountAnomaly,  // Tutar (Örn: Normalden 10 kat fazla)
    Geographic,     // Konum (Örn: 1 saatte farklı ülkeden giriş)
    TimePattern     // Zaman (Örn: Gece 03:00-05:00 arası)
}

/// <summary>
/// Alarm Önem Derecesi
/// </summary>
public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Alarm Durumu
/// </summary>
public enum AlertStatus
{
    Open,           // Açık (İncelenmeyi bekliyor)
    InReview,       // İnceleniyor
    Resolved,       // Çözüldü (Dolandırıcılık onaylandı)
    FalsePositive   // Yanlış Alarm (Güvenli işlem)
}

/// <summary>
/// Müşteri Risk Seviyesi (Risk Profilinden gelir)
/// </summary>
// (Not: UserManagementEnums içinde RiskLevel zaten vardı, 
// eğer orada varsa buraya eklemeye gerek yok ama
// karışıklık olmasın diye burada tekrar tanımlayabiliriz veya oradakini kullanabiliriz.
// Şemada 'risk_level' alanı User tablosunda vardı. 
// Biz burada farklı bir enum ismi kullanalım.)
public enum ProfileRiskLevel
{
    Low,
    Medium,
    High,
    VeryHigh
}