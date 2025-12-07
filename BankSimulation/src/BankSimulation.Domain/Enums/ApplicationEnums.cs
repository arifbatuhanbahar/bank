namespace BankSimulation.Domain.Enums;

/// <summary>
/// Başvuru Durumu
/// </summary>
public enum ApplicationStatus
{
    Pending,    // Beklemede
    Approved,   // Onaylandı
    Rejected    // Reddedildi
}

/// <summary>
/// İstenen Kart Tipi (Prestij)
/// </summary>
public enum CardPrestigeLevel
{
    Standard,
    Gold,
    Platinum,
    Corporate,  // Kurumsal
    Black       // VIP
}

/// <summary>
/// Çalışma Durumu
/// </summary>
public enum EmploymentStatus
{
    Employed,       // Çalışıyor
    SelfEmployed,   // Kendi İşi
    Unemployed,     // İşsiz
    Retired,        // Emekli
    Student         // Öğrenci
}

/// <summary>
/// Kart Limit Türü
/// </summary>
public enum CardLimitType
{
    TotalLimit,         // Genel Limit
    CashAdvance,        // Nakit Avans
    Installment,        // Taksitli Alışveriş
    OnlineShopping,     // E-Ticaret
    Contactless         // Temassız İşlem
}