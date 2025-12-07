namespace BankSimulation.Domain.Enums;

/// <summary>
/// Kullanıcı hesap durumu
/// </summary>
public enum UserStatus
{
    Active,
    Suspended,
    Locked,
    Closed
}

/// <summary>
/// KYC (Know Your Customer) doğrulama durumu
/// </summary>
public enum KycStatus
{
    Pending,
    Verified,
    Rejected
}

/// <summary>
/// Kullanıcı risk seviyesi
/// </summary>
public enum RiskLevel
{
    Low,
    Medium,
    High
}

/// <summary>
/// Kullanıcı rolleri
/// </summary>
public enum RoleName
{
    Customer,
    Employee,
    Admin,
    Auditor
}

/// <summary>
/// Hesap türü
/// </summary>
public enum AccountType
{
    Checking,
    Savings,
    Investment
}

/// <summary>
/// Para birimi
/// </summary>
public enum Currency
{
    TRY,
    USD,
    EUR
}

/// <summary>
/// Hesap durumu
/// </summary>
public enum AccountStatus
{
    Active,
    Frozen,
    Closed
}

/// <summary>
/// Limit türü
/// </summary>
public enum LimitType
{
    DailyTransfer,
    DailyWithdrawal,
    Monthly
}