using BankSimulation.Domain.Entities.Fraud;
using BankSimulation.Domain.Enums;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FraudController : ControllerBase
{
    private readonly DapperContext _context;

    public FraudController(DapperContext context)
    {
        _context = context;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule(FraudRuleRequest request)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO fraud_rules (
                rule_name, rule_type, rule_description, rule_conditions, 
                risk_score_weight, is_active, created_at
            )
            VALUES (
                @RuleName, @RuleType, @Description, @Conditions, 
                @RiskWeight, 1, GETDATE()
            )";

        await connection.ExecuteAsync(sql, new {
            request.RuleName, 
            RuleType = request.RuleType.ToString(),
            request.Description, 
            request.Conditions, 
            request.RiskWeight
        });

        return Ok("Kural eklendi.");
    }

    [HttpPost("check-transaction")]
    public async Task<IActionResult> CheckTransaction(int transactionId, int userId, decimal amount)
    {
        using var connection = _context.CreateConnection();

        var rules = await connection.QueryAsync<FraudRule>("SELECT * FROM fraud_rules WHERE is_active = 1");
        
        int totalRiskScore = 0;
        List<string> triggered = new List<string>();

        foreach (var rule in rules)
        {
            if (rule.RuleType == RuleType.AmountAnomaly && amount > 50000)
            {
                totalRiskScore += rule.RiskScoreWeight;
                triggered.Add(rule.RuleName);
            }
        }

        if (totalRiskScore > 0)
        {
            var alertSql = @"
                INSERT INTO fraud_alerts (
                    user_id, transaction_id, fraud_score, triggered_rules, 
                    alert_severity, status, created_at
                )
                VALUES (
                    @UserId, @TxId, @Score, @Rules, 
                    @Severity, 'Open', GETDATE()
                )";

            var severity = totalRiskScore > 80 ? "Critical" : "High";
            
            await connection.ExecuteAsync(alertSql, new {
                UserId = userId, TxId = transactionId, Score = totalRiskScore,
                Rules = string.Join(", ", triggered), Severity = severity
            });

            return Ok(new { Message = "RİSKLİ İŞLEM!", Score = totalRiskScore });
        }

        return Ok(new { Message = "Güvenli", Score = 0 });
    }
}

public class FraudRuleRequest
{
    public string RuleName { get; set; } = null!;
    public RuleType RuleType { get; set; }
    public string Description { get; set; } = null!;
    public string Conditions { get; set; } = null!;
    public int RiskWeight { get; set; }
}