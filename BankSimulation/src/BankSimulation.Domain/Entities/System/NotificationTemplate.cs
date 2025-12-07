using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.System;

public class NotificationTemplate
{
    public int TemplateId { get; set; }
    
    public string TemplateName { get; set; } = null!; // Örn: "WelcomeEmail"
    
    public TemplateType TemplateType { get; set; }
    
    public string? Subject { get; set; } // Sadece Email için
    
    public string Body { get; set; } = null!; // "Merhaba {Name}, hoşgeldiniz..."
    
    public string? Variables { get; set; } // JSON: ["Name", "Date"]
    
    public string Language { get; set; } = "TR";
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}