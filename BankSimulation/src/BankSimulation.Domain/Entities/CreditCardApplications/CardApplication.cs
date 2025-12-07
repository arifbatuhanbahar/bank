using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.CreditCardApplications;

public class CardApplication
{
    public int ApplicationId { get; set; }
    
    public int UserId { get; set; }
    
    public CardPrestigeLevel CardTypeRequested { get; set; }
    
    public decimal MonthlyIncome { get; set; }
    
    public EmploymentStatus EmploymentStatus { get; set; }
    
    public string EmployerName { get; set; } = null!; // İşveren Adı
    
    public DateTime ApplicationDate { get; set; } = DateTime.Now;
    
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    
    public string? RejectionReason { get; set; }
    
    public int? ApprovedBy { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public decimal? CreditLimitApproved { get; set; } // Onaylanan Limit
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
}