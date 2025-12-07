using BankSimulation.Domain.Entities.CreditCardApplications;
using BankSimulation.Domain.Entities.PaymentAndCards;
using BankSimulation.Domain.Enums;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly DapperContext _context;

    public ApplicationController(DapperContext context)
    {
        _context = context;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    [HttpPost("apply")]
    public async Task<ActionResult> ApplyForCard(CardApplicationRequest request)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO card_applications (
                user_id, card_type_requested, monthly_income, employment_status, 
                employer_name, application_date, status
            )
            VALUES (
                @UserId, @CardType, @Income, @EmpStatus, 
                @EmpName, GETDATE(), 'Pending'
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await connection.QuerySingleAsync<int>(sql, new {
            request.UserId, 
            CardType = request.CardTypeRequested.ToString(),
            Income = request.MonthlyIncome, 
            EmpStatus = request.EmploymentStatus.ToString(),
            EmpName = request.EmployerName
        });

        return Ok(new { Message = "Başvuru alındı", ApplicationId = id });
    }

    [HttpPost("approve/{applicationId}")]
    public async Task<IActionResult> ApproveApplication(int applicationId, decimal approvedLimit)
    {
        using var connection = _context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var app = await connection.QuerySingleOrDefaultAsync<CardApplication>(
                "SELECT * FROM card_applications WHERE application_id = @Id", 
                new { Id = applicationId }, transaction);

            if (app == null) return NotFound();

            await connection.ExecuteAsync(@"
                UPDATE card_applications 
                SET status = 'Approved', approved_by = 1, approved_at = GETDATE(), credit_limit_approved = @Limit 
                WHERE application_id = @Id", 
                new { Limit = approvedLimit, Id = applicationId }, transaction);

            var cardSql = @"
                INSERT INTO credit_cards (
                    user_id, card_number_encrypted, card_last_four, cvv_encrypted,
                    card_type, card_brand, credit_limit, available_limit, current_balance,
                    minimum_payment, interest_rate, expiry_month, expiry_year,
                    payment_due_date, status, issued_at
                )
                VALUES (
                    @UserId, '5555000011112222', '2222', '999',
                    'Physical', 'Visa', @Limit, @Limit, 0,
                    0, 3.5, 12, 2030,
                    DATEADD(day, 30, GETDATE()), 'Active', GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var cardId = await connection.QuerySingleAsync<int>(cardSql, new { app.UserId, Limit = approvedLimit }, transaction);

            var limitSql = @"
                INSERT INTO card_limits (card_id, limit_type, limit_amount, used_amount)
                VALUES (@CardId, 'OnlineShopping', @Limit, 0),
                       (@CardId, 'Contactless', 750, 0)";
            
            await connection.ExecuteAsync(limitSql, new { CardId = cardId, Limit = approvedLimit }, transaction);

            transaction.Commit();
            return Ok(new { Message = "Onaylandı", CardId = cardId });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, ex.Message);
        }
    }
}

public class CardApplicationRequest
{
    public int UserId { get; set; }
    public CardPrestigeLevel CardTypeRequested { get; set; }
    public decimal MonthlyIncome { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public string EmployerName { get; set; } = null!;
}