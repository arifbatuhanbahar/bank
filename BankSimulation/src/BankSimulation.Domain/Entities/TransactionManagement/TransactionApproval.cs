using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.TransactionManagement;

/// <summary>
/// İşlem onay kayıtları
/// </summary>
public class TransactionApproval
{
    public int ApprovalId { get; set; }
    
    public int TransactionId { get; set; }
    
    // Onaylayan yönetici
    public int ApproverId { get; set; }
    
    public int ApprovalLevel { get; set; } // 1, 2, 3 (Kademeli onay için)
    
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    
    public string? Comments { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation Properties
    public virtual Transaction Transaction { get; set; } = null!;
    public virtual User Approver { get; set; } = null!;
}