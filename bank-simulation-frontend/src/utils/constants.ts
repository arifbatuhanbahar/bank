// ==================== APP CONSTANTS ====================

export const APP_NAME = "Bank Simulation";
export const APP_VERSION = "1.0.0";

// ==================== API CONSTANTS ====================

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5161/api";
export const API_TIMEOUT = 15000; // 15 seconds

// ==================== AUTH CONSTANTS ====================

export const TOKEN_KEY = "authToken";
export const USER_KEY = "user";
export const SESSION_TIMEOUT = 30 * 60 * 1000; // 30 minutes
export const MAX_LOGIN_ATTEMPTS = 5;

// ==================== PAGINATION ====================

export const DEFAULT_PAGE_SIZE = 10;
export const PAGE_SIZE_OPTIONS = [5, 10, 25, 50, 100];

// ==================== LIMITS ====================

export const TRANSFER_LIMITS = {
  MIN_AMOUNT: 1,
  MAX_AMOUNT: 100000,
  DAILY_LIMIT_TRY: 500000,
  DAILY_LIMIT_USD: 50000,
  DAILY_LIMIT_EUR: 50000,
};

export const CARD_LIMITS = {
  MIN_INCOME_FOR_APPLICATION: 5000,
  DEFAULT_CREDIT_LIMIT: 10000,
  MAX_CREDIT_LIMIT: 500000,
  CONTACTLESS_LIMIT: 750,
};

export const MASAK_THRESHOLD = 75000; // TL

// ==================== CURRENCY SYMBOLS ====================

export const CURRENCY_SYMBOLS: Record<string, string> = {
  TRY: "₺",
  USD: "$",
  EUR: "€",
  GBP: "£",
};

export const CURRENCY_NAMES: Record<string, string> = {
  TRY: "Türk Lirası",
  USD: "Amerikan Doları",
  EUR: "Euro",
  GBP: "İngiliz Sterlini",
};

// ==================== ACCOUNT TYPES ====================

export const ACCOUNT_TYPE_LABELS: Record<string, string> = {
  Checking: "Vadesiz Mevduat",
  Savings: "Vadeli Mevduat",
  Investment: "Yatırım Hesabı",
};

// ==================== TRANSACTION TYPES ====================

export const TRANSACTION_TYPE_LABELS: Record<string, string> = {
  Transfer: "Transfer",
  Deposit: "Para Yatırma",
  Withdrawal: "Para Çekme",
  Payment: "Ödeme",
};

export const TRANSACTION_STATUS_LABELS: Record<string, string> = {
  Pending: "Beklemede",
  Processing: "İşleniyor",
  Completed: "Tamamlandı",
  Failed: "Başarısız",
  Reversed: "İade Edildi",
  Cancelled: "İptal Edildi",
};

// ==================== CARD BRANDS ====================

export const CARD_BRAND_LABELS: Record<string, string> = {
  Visa: "Visa",
  MasterCard: "MasterCard",
  Troy: "Troy",
  AmericanExpress: "American Express",
};

export const CARD_TYPE_LABELS: Record<string, string> = {
  Physical: "Fiziksel",
  Virtual: "Sanal",
};

export const CARD_STATUS_LABELS: Record<string, string> = {
  Active: "Aktif",
  Blocked: "Bloke",
  Expired: "Süresi Dolmuş",
  Cancelled: "İptal Edilmiş",
};

// ==================== KYC / COMPLIANCE ====================

export const KYC_STATUS_LABELS: Record<string, string> = {
  Pending: "Beklemede",
  Verified: "Doğrulandı",
  Rejected: "Reddedildi",
};

export const DOCUMENT_TYPE_LABELS: Record<string, string> = {
  IdentityCard: "Kimlik Kartı",
  Passport: "Pasaport",
  DriverLicense: "Ehliyet",
  UtilityBill: "Fatura (İkametgah)",
  TaxPlate: "Vergi Levhası",
};

export const EMPLOYMENT_STATUS_LABELS: Record<string, string> = {
  Employed: "Çalışan",
  SelfEmployed: "Serbest Meslek",
  Unemployed: "Çalışmıyor",
  Retired: "Emekli",
  Student: "Öğrenci",
};

export const CONSENT_TYPE_LABELS: Record<string, string> = {
  DataProcessing: "Veri İşleme",
  Marketing: "Pazarlama",
  ThirdPartyTransfer: "3. Taraf Paylaşımı",
};

export const KVKK_REQUEST_TYPE_LABELS: Record<string, string> = {
  Access: "Veri Erişimi",
  Rectification: "Düzeltme",
  Erasure: "Silme (Unutulma)",
  Portability: "Veri Taşıma",
};

export const REQUEST_STATUS_LABELS: Record<string, string> = {
  Pending: "Beklemede",
  InProgress: "İşleniyor",
  Completed: "Tamamlandı",
  Rejected: "Reddedildi",
};

export const APPLICATION_STATUS_LABELS: Record<string, string> = {
  Pending: "Beklemede",
  Approved: "Onaylandı",
  Rejected: "Reddedildi",
};

export const APPLICATION_PRESTIGE_LABELS: Record<string, string> = {
  Standard: "Standart",
  Gold: "Gold",
  Platinum: "Platinum",
  Corporate: "Kurumsal",
  Black: "Black",
};

// ==================== RISK LEVELS ====================

export const RISK_LEVEL_LABELS: Record<string, string> = {
  Low: "Düşük",
  Medium: "Orta",
  High: "Yüksek",
};

export const SEVERITY_LABELS: Record<string, string> = {
  Low: "Düşük",
  Medium: "Orta",
  High: "Yüksek",
  Critical: "Kritik",
};

// ==================== ROUTES ====================

export const ROUTES = {
  HOME: "/",
  LOGIN: "/login",
  REGISTER: "/register",
  DASHBOARD: "/dashboard",
  ACCOUNTS: "/accounts",
  ACCOUNT_DETAIL: "/accounts/:id",
  TRANSACTIONS: "/transactions",
  TRANSFER: "/transfer",
  CARDS: "/cards",
  CARD_APPLICATION: "/cards/apply",
  CARD_DETAIL: "/cards/:id",
  COMPLIANCE: "/compliance",
  KYC: "/compliance/kyc",
  KVKK: "/compliance/kvkk",
  SETTINGS: "/settings",
  PROFILE: "/profile",
  ADMIN: "/admin",
  ADMIN_USERS: "/admin/users",
  ADMIN_FRAUD: "/admin/fraud",
  ADMIN_AUDIT: "/admin/audit",
  ADMIN_SYSTEM: "/admin/system",
} as const;

// ==================== DATE FORMATS ====================

export const DATE_FORMATS = {
  DISPLAY: "dd.MM.yyyy",
  DISPLAY_WITH_TIME: "dd.MM.yyyy HH:mm",
  API: "yyyy-MM-dd",
  API_WITH_TIME: "yyyy-MM-dd'T'HH:mm:ss",
  MONTH_YEAR: "MMMM yyyy",
  DAY_MONTH: "d MMMM",
};

// ==================== ERROR MESSAGES ====================

export const ERROR_MESSAGES = {
  NETWORK_ERROR: "Ağ bağlantısı hatası. Lütfen internet bağlantınızı kontrol edin.",
  SERVER_ERROR: "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.",
  UNAUTHORIZED: "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.",
  FORBIDDEN: "Bu işlem için yetkiniz bulunmamaktadır.",
  NOT_FOUND: "İstenen kaynak bulunamadı.",
  VALIDATION_ERROR: "Lütfen form alanlarını kontrol edin.",
  INSUFFICIENT_BALANCE: "Yetersiz bakiye.",
  LIMIT_EXCEEDED: "Günlük limit aşıldı.",
  GENERIC_ERROR: "Bir hata oluştu. Lütfen tekrar deneyin.",
};

// ==================== SUCCESS MESSAGES ====================

export const SUCCESS_MESSAGES = {
  TRANSFER_SUCCESS: "Transfer işlemi başarıyla gerçekleştirildi.",
  ACCOUNT_CREATED: "Hesap başarıyla oluşturuldu.",
  CARD_APPLICATION_SENT: "Kart başvurunuz alındı.",
  DOCUMENT_UPLOADED: "Belgeniz başarıyla yüklendi.",
  PROFILE_UPDATED: "Profiliniz güncellendi.",
  PASSWORD_CHANGED: "Şifreniz başarıyla değiştirildi.",
};

// ==================== ADMIN ====================
export const ADMIN_EMAILS = [
  "admin@bank.com",
  "root@bank.com",
] as const;
