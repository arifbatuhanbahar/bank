namespace BankSimulation.Domain.Entities.TransactionManagement;

/// <summary>
/// İşlem türü tanımları (Ücretler, kurallar)
/// </summary>
public class TransactionTypeDefinition
{
    public int TypeId { get; set; }
    
    public string TypeName { get; set; } = null!; // Örn: "FAST", "SWIFT"
    
    public string TypeCode { get; set; } = null!; // Örn: "TR_FAST"
    
    public string? Description { get; set; }
    
    // Sabit ücret (Örn: 5 TL)
    public decimal FeeFixed { get; set; }
    
    // Oransal ücret (Örn: %0.01)
    public decimal FeePercentage { get; set; }
    
    public bool IsActive { get; set; } = true;
}