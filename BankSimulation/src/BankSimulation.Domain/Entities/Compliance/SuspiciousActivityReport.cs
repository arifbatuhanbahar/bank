using BankSimulation.Domain.Entities.TransactionManagement;
using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Compliance;

public class SuspiciousActivityReport
{
    public int SarId { get; set; }
    
    public int UserId { get; set; }
    
    public int? TransactionId { get; set; }
    
    public SarReportType ReportType { get; set; }
    
    public string Description { get; set; } = null!;
    
    public int RiskScore { get; set; } // 1-100 arası risk puanı
    
    public int? CreatedBy { get; set; } // Raporu oluşturan personel
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public bool ReportedToMasak { get; set; }
    
    public DateTime? MasakReportDate { get; set; }
    
    public string? MasakReferenceNumber { get; set; }
    
    public SarStatus Status { get; set; } = SarStatus.Draft;
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Transaction? Transaction { get; set; }
    public virtual User? CreatedByUser { get; set; }
}