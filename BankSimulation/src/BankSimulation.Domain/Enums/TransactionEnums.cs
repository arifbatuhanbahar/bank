namespace BankSimulation.Domain.Enums;

/// <summary>
/// İşlem Türü (Kod içinde kullanım için)
/// </summary>
public enum TransactionType
{
    Transfer,       // Havale/EFT
    Deposit,        // Para Yatırma
    Withdrawal,     // Para Çekme
    Payment         // Fatura/Kart Ödemesi
}

/// <summary>
/// İşlem Durumu
/// </summary>
public enum TransactionStatus
{
    Pending,        // Beklemede (Onay veya süre bekleniyor)
    Processing,     // İşleniyor
    Completed,      // Başarılı
    Failed,         // Hata oluştu
    Reversed,       // İade edildi / Geri alındı
    Cancelled       // İptal edildi
}

/// <summary>
/// İşlem Sıklığı (Düzenli ödemeler için)
/// </summary>
public enum Frequency
{
    OneTime,        // Tek seferlik
    Daily,          // Günlük
    Weekly,         // Haftalık
    Monthly,        // Aylık
    Yearly          // Yıllık
}

/// <summary>
/// Onay Durumu
/// </summary>
public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

/// <summary>
/// Muhasebe Kayıt Türü
/// </summary>
public enum LedgerEntryType
{
    Debit,  // Borç (Hesaptan çıkan)
    Credit  // Alacak (Hesaba giren)
}

/// <summary>
/// Düzenli ödeme talimatı durumu
/// </summary>
public enum ScheduledTransactionStatus
{
    Active,     // Aktif
    Paused,     // Duraklatıldı
    Cancelled,  // İptal
    Completed   // Tamamlandı (Süresi bitti)
}