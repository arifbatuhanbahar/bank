using BankSimulation.Domain.Entities.System;
using BankSimulation.Domain.Enums;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly DapperContext _context;

    public SystemController(DapperContext context)
    {
        _context = context;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    [HttpGet("settings")]
    public async Task<IEnumerable<SystemSetting>> GetSettings()
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SystemSetting>("SELECT * FROM system_settings");
    }

    [HttpPost("settings")]
    public async Task<IActionResult> UpdateSetting(string key, string value, string? description)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            IF EXISTS (SELECT 1 FROM system_settings WHERE setting_key = @Key)
                UPDATE system_settings 
                SET setting_value = @Value, description = @Desc, updated_at = GETDATE()
                WHERE setting_key = @Key
            ELSE
                INSERT INTO system_settings (
                    setting_key, setting_value, setting_type, description, updated_at, updated_by
                )
                VALUES (@Key, @Value, 'String', @Desc, GETDATE(), 1)";

        await connection.ExecuteAsync(sql, new { Key = key, Value = value, Desc = description });
        return Ok("Ayar güncellendi.");
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CreateTemplate(TemplateRequest request)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO notification_templates (
                template_name, template_type, subject, body, variables, 
                language, is_active, created_at
            )
            VALUES (
                @TemplateName, @Type, @Subject, @Body, @Variables, 
                'TR', 1, GETDATE()
            )";

        await connection.ExecuteAsync(sql, new {
            request.TemplateName, Type = request.TemplateType.ToString(),
            request.Subject, request.Body, request.Variables
        });

        return Ok("Şablon oluşturuldu.");
    }
}

public class TemplateRequest
{
    public string TemplateName { get; set; } = null!;
    public TemplateType TemplateType { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; } = null!;
    public string? Variables { get; set; }
}