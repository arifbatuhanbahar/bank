namespace BankSimulation.Domain.Enums;

/// <summary>
/// Ödeme Yöntemi Türü
/// </summary>
public enum PaymentMethodType
{
    CreditCard,     // Kredi Kartı
    DebitCard,      // Banka Kartı
    BankAccount     // Banka Hesabı
}

/// <summary>
/// Kart Markası
/// </summary>
public enum CardBrand
{
    Visa,
    MasterCard,
    Troy,
    AmericanExpress
}

/// <summary>
/// Kart Türü
/// </summary>
public enum CardType
{
    Physical,       // Fiziksel Kart
    Virtual         // Sanal Kart
}

/// <summary>
/// Kart/Ödeme Durumu
/// </summary>
public enum CardStatus
{
    Active,         // Aktif
    Blocked,        // Bloke
    Expired,        // Süresi Dolmuş
    Cancelled       // İptal Edilmiş
}

/// <summary>
/// Kart İşlem Durumu (Provizyon)
/// </summary>
public enum CardTransactionStatus
{
    Pending,        // Provizyonda
    Approved,       // Onaylandı
    Declined,       // Reddedildi
    Refunded        // İade Edildi
}

/// <summary>
/// Gateway Türü
/// </summary>
public enum GatewayType
{
    VirtualPos,     // Sanal POS
    ThreeD,         // 3D Secure
    DirectDebit     // Doğrudan Borçlandırma
}