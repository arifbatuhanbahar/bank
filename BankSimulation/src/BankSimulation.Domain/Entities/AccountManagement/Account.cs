using BankSimulation.Domain.Enums;
using BankSimulation.Domain.Entities.UserManagement;

namespace BankSimulation.Domain.Entities.AccountManagement;

/// <summary>
/// Banka hesapları tablosu
/// </summary>
public class Account
{
    public int AccountId { get; set; }
    
    public int UserId { get; set; }
    
    // IBAN formatında, UNIQUE
    public string AccountNumber { get; set; } = null!;
    
    public AccountType AccountType { get; set; }
    
    public Currency Currency { get; set; } = Currency.TRY;
    
    public decimal Balance { get; set; }
    
    public decimal AvailableBalance { get; set; }
    
    public decimal DailyTransferLimit { get; set; }
    
    public decimal DailyWithdrawalLimit { get; set; }
    
    public decimal InterestRate { get; set; }
    
    public DateTime? InterestCalculationDate { get; set; }
    
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    
    public DateTime OpenedDate { get; set; }
    
    public DateTime? ClosedDate { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<AccountBeneficiary> Beneficiaries { get; set; } = new List<AccountBeneficiary>();
    public virtual ICollection<AccountLimit> Limits { get; set; } = new List<AccountLimit>();
}