using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Domain.Enums;

namespace BankSimulation.Domain.Entities.System;

public class SystemSetting
{
    public int SettingId { get; set; }
    
    public string SettingKey { get; set; } = null!; // Örn: "default_interest_rate"
    
    public string SettingValue { get; set; } = null!; // Örn: "3.50"
    
    public SettingType SettingType { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public int? UpdatedBy { get; set; }
    
    // Navigation Property
    public virtual User? UpdatedByUser { get; set; }
}