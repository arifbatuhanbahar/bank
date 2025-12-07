using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Compliance;

public class KvkkDataRequest
{
    public int RequestId { get; set; }
    
    public int UserId { get; set; }
    
    public KvkkRequestType RequestType { get; set; }
    
    public DateTime RequestDate { get; set; } = DateTime.Now;
    
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    
    public DateTime? CompletedAt { get; set; }
    
    public int? CompletedBy { get; set; }
    
    // Talep edilen veriler (JSON formatında saklanır)
    public string? ResponseData { get; set; }
    
    // Eğer veriler dosya olarak verildiyse yolu
    public string? ResponseFilePath { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? CompletedByUser { get; set; }
}