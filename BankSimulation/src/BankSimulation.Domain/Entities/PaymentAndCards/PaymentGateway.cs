using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.PaymentAndCards;

/// <summary>
/// Ödeme Sağlayıcıları / Sanal POS
/// </summary>
public class PaymentGateway
{
    public int GatewayId { get; set; }
    
    public string GatewayName { get; set; } = null!;
    
    public GatewayType GatewayType { get; set; }
    
    public string ApiEndpoint { get; set; } = null!;
    
    public string ApiKeyEncrypted { get; set; } = null!;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}