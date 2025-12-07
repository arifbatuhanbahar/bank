namespace BankSimulation.Domain.Entities.AccountManagement;

/// <summary>
/// Hesap türleri tanım tablosu
/// </summary>
public class AccountTypeDefinition
{
    public int TypeId { get; set; }
    
    public string TypeName { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public decimal MinBalance { get; set; }
    
    public decimal InterestRate { get; set; }
    
    public string? Features { get; set; }  // JSON olarak
    
    public bool IsActive { get; set; } = true;
}