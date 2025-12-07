using BankSimulation.Domain.Entities.TransactionManagement;
using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Compliance;

public class MasakRecord
{
    public int RecordId { get; set; }
    
    public int CustomerId { get; set; }
    
    public int? TransactionId { get; set; }
    
    public MasakRecordType RecordType { get; set; }
    
    // Bildirilen verinin detayı (JSON)
    public string Data { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Yasal saklama süresi (Genelde 8 veya 10 yıl)
    public DateTime RetentionUntil { get; set; }
    
    // Navigation Properties
    public virtual User Customer { get; set; } = null!;
    public virtual Transaction? Transaction { get; set; }
}