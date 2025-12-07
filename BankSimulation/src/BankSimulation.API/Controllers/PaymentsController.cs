using BankSimulation.Domain.Entities.PaymentAndCards;
using BankSimulation.Domain.Enums;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly DapperContext _context;

    public PaymentsController(DapperContext context)
    {
        _context = context;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    // GET: api/payments/cards/user/1
    [HttpGet("cards/user/{userId}")]
    public async Task<ActionResult<IEnumerable<CreditCard>>> GetCardsByUser(int userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM credit_cards WHERE user_id = @UserId";
        var cards = await connection.QueryAsync<CreditCard>(sql, new { UserId = userId });
        return Ok(cards);
    }

    // POST: api/payments/cards
    [HttpPost("cards")]
    public async Task<ActionResult<CreditCard>> CreateCard(CreateCardRequest request)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            INSERT INTO credit_cards (
                user_id, card_number_encrypted, card_last_four, cvv_encrypted,
                card_type, card_brand, credit_limit, available_limit, current_balance,
                minimum_payment, interest_rate, expiry_month, expiry_year,
                payment_due_date, status, issued_at
            )
            VALUES (
                @UserId, @CardNumber, @LastFour, '123',
                'Physical', 'MasterCard', @Limit, @Limit, 0,
                0, 3.5, 12, 2030,
                DATEADD(day, 30, GETDATE()), 'Active', GETDATE()
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var parameters = new
        {
            request.UserId,
            request.CardNumber,
            LastFour = request.CardNumber.Substring(request.CardNumber.Length - 4),
            request.Limit
        };

        var newId = await connection.QuerySingleAsync<int>(sql, parameters);

        // Cevap dönmek için basit bir nesne oluşturuyoruz
        var createdCard = new CreditCard
        {
            CardId = newId,
            UserId = request.UserId,
            CardLastFour = parameters.LastFour,
            CreditLimit = request.Limit
        };

        return CreatedAtAction(nameof(GetCardsByUser), new { userId = request.UserId }, createdCard);
    }

    // POST: api/payments/transaction
    [HttpPost("transaction")]
    public async Task<IActionResult> MakeTransaction(CardTransactionRequest request)
    {
        using var connection = _context.CreateConnection();
        connection.Open();
        
        // Transaction başlatıyoruz (Limit düşme ve kayıt ekleme atomik olmalı)
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Kartı ve Limiti Kontrol Et (SQL ile)
            var cardSql = "SELECT * FROM credit_cards WHERE card_id = @CardId";
            var card = await connection.QuerySingleOrDefaultAsync<CreditCard>(cardSql, new { request.CardId }, transaction);

            if (card == null) return NotFound("Kart bulunamadı.");
            if (card.AvailableLimit < request.Amount) return BadRequest("Yetersiz limit.");

            // 2. Kart Limitini Düşür (UPDATE)
            var updateCardSql = @"
                UPDATE credit_cards 
                SET available_limit = available_limit - @Amount,
                    current_balance = current_balance + @Amount
                WHERE card_id = @CardId";
            
            await connection.ExecuteAsync(updateCardSql, new { request.Amount, request.CardId }, transaction);

            // 3. Harcama Kaydı Ekle (INSERT)
            var insertTxSql = @"
                INSERT INTO card_transactions (
                    card_id, merchant_name, merchant_category, amount, currency,
                    transaction_date, status, authorization_code
                )
                VALUES (
                    @CardId, @MerchantName, 'Shopping', @Amount, 'TRY',
                    GETDATE(), 'Approved', LEFT(NEWID(), 8)
                );";

            await connection.ExecuteAsync(insertTxSql, new { request.CardId, request.MerchantName, request.Amount }, transaction);

            // 4. Onayla
            transaction.Commit();

            return Ok(new { Message = "İşlem Başarılı" });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, $"Hata: {ex.Message}");
        }
    }
}

// DTO'lar (Aynı kalıyor)
public class CreateCardRequest
{
    public int UserId { get; set; }
    public string CardNumber { get; set; } = null!;
    public decimal Limit { get; set; }
}

public class CardTransactionRequest
{
    public int CardId { get; set; }
    public decimal Amount { get; set; }
    public string MerchantName { get; set; } = null!;
}