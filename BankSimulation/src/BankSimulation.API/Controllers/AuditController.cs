using BankSimulation.Domain.Entities.Audit;
using BankSimulation.Domain.Enums;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly DapperContext _context;

    public AuditController(DapperContext context)
    {
        _context = context;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    [HttpPost("security-event")]
    public async Task<IActionResult> LogSecurityEvent(SecurityEventRequest request)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO security_events (
                event_type, user_id, severity, description, 
                ip_address, user_agent, event_date, resolved
            )
            VALUES (
                @EventType, @UserId, @Severity, @Description, 
                '192.168.1.10', 'Mozilla/5.0', GETDATE(), 0
            )";

        await connection.ExecuteAsync(sql, new {
            EventType = request.EventType.ToString(),
            request.UserId,
            Severity = request.Severity.ToString(),
            request.Description
        });

        return Ok(new { Message = "Güvenlik olayı loglandı." });
    }

    [HttpGet("security-events")]
    public async Task<ActionResult<IEnumerable<SecurityEvent>>> GetSecurityEvents()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT TOP 10 * FROM security_events ORDER BY event_date DESC";
        var events = await connection.QueryAsync<SecurityEvent>(sql);
        return Ok(events);
    }
    
    [HttpPost("access-log")]
    public async Task<IActionResult> LogDataAccess(int staffUserId, int customerUserId, string reason)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO data_access_log (
                accessed_by_user_id, target_user_id, data_type, 
                access_reason, access_timestamp, ip_address, is_sensitive
            )
            VALUES (
                @StaffId, @CustomerId, 'Financial', 
                @Reason, GETDATE(), '10.0.0.5', 1
            )";

        await connection.ExecuteAsync(sql, new { StaffId = staffUserId, CustomerId = customerUserId, Reason = reason });
        return Ok("Veri erişimi kayıt altına alındı.");
    }
}

public class SecurityEventRequest
{
    public SecurityEventType EventType { get; set; }
    public int? UserId { get; set; }
    public Severity Severity { get; set; }
    public string Description { get; set; } = null!;
}