using BankSimulation.Domain.Entities.AccountManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.TransactionManagement;

/// <summary>
/// Muhasebe / Büyük Defter Kayıtları
/// </summary>
public class GeneralLedger
{
    public int LedgerId { get; set; }
    
    public int TransactionId { get; set; }
    
    public int AccountId { get; set; }
    
    public decimal DebitAmount { get; set; } // Borç
    
    public decimal CreditAmount { get; set; } // Alacak
    
    public LedgerEntryType EntryType { get; set; }
    
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    
    public string? Description { get; set; }
    
    // Navigation Properties
    public virtual Transaction Transaction { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
}