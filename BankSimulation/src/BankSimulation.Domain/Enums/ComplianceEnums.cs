namespace BankSimulation.Domain.Enums;

/// <summary>
/// KYC Belge Türü
/// </summary>
public enum DocumentType
{
    IdentityCard,       // Kimlik Kartı
    Passport,           // Pasaport
    DriverLicense,      // Ehliyet
    UtilityBill,        // Fatura (İkametgah için)
    TaxPlate            // Vergi Levhası (Şirketler için)
}

/// <summary>
/// Doğrulama Durumu
/// </summary>
public enum VerificationStatus
{
    Pending,
    Verified,
    Failed,
    Rejected
}

/// <summary>
/// Doğrulama Türü
/// </summary>
public enum VerificationType
{
    Email,
    Phone,
    Identity,           // Kimlik Doğrulama
    Address,            // Adres Doğrulama
    Income              // Gelir Doğrulama
}

/// <summary>
/// Doğrulama Yöntemi
/// </summary>
public enum VerificationMethod
{
    Sms,
    EmailLink,
    VideoCall,          // Görüntülü Görüşme (NFC)
    DocumentUpload,
    DatabaseCheck       // NVI (Nüfus) Sorgusu
}

/// <summary>
/// KVKK Rıza Türü
/// </summary>
public enum ConsentType
{
    DataProcessing,     // Veri İşleme
    Marketing,          // Pazarlama / Kampanya
    ThirdPartyTransfer  // 3. Taraf Paylaşımı
}

/// <summary>
/// KVKK Talep Türü
/// </summary>
public enum KvkkRequestType
{
    Access,             // Veriye Erişim
    Rectification,      // Düzeltme
    Erasure,            // Silme (Unutulma Hakkı)
    Portability         // Veri Taşıma
}

/// <summary>
/// KVKK Talep Durumu
/// </summary>
public enum RequestStatus
{
    Pending,
    InProgress,
    Completed,
    Rejected
}

/// <summary>
/// MASAK Kayıt Türü
/// </summary>
public enum MasakRecordType
{
    KycLog,             // Kimlik Tespiti
    TransactionReport,  // İşlem Bildirimi
    SuspiciousReport,   // Şüpheli İşlem
    CustomerDueDiligence // Müşteri Tanıma Formu
}

/// <summary>
/// Şüpheli İşlem Raporu (SAR) Türü
/// </summary>
public enum SarReportType
{
    Structuring,        // Şirinleme (Bölerek yatırma)
    LargeCash,          // Yüksek Nakit
    UnusualActivity,    // Olağandışı Aktivite
    TerrorismFinancing  // Terör Finansmanı Şüphesi
}

/// <summary>
/// SAR Durumu
/// </summary>
public enum SarStatus
{
    Draft,              // Taslak
    Submitted,          // Gönderildi
    Closed              // Kapatıldı (İncelendi)
}