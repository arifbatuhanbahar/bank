using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.Audit;

public class EncryptionKey
{
    public int KeyId { get; set; }
    public string KeyName { get; set; } = null!;
    public KeyType KeyType { get; set; }
    public string KeyValueEncrypted { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RotationDate { get; set; }
    public KeyStatus Status { get; set; } = KeyStatus.Active;
}