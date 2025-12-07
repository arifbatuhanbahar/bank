namespace BankSimulation.Domain.Entities.AccountManagement;

/// <summary>
/// Kayıtlı alıcılar (havale/EFT için)
/// </summary>
public class AccountBeneficiary
{
    public int BeneficiaryId { get; set; }
    
    public int AccountId { get; set; }
    
    public string BeneficiaryName { get; set; } = null!;
    
    public string BeneficiaryIban { get; set; } = null!;
    
    public string? BeneficiaryBank { get; set; }
    
    public string? Nickname { get; set; }
    
    public bool IsVerified { get; set; }
    
    public DateTime AddedAt { get; set; }
    
    // Navigation Property
    public virtual Account Account { get; set; } = null!;
}