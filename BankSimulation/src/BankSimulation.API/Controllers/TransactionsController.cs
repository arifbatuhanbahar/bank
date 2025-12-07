using BankSimulation.Domain.Entities.AccountManagement;
using BankSimulation.Domain.Entities.TransactionManagement;
using BankSimulation.Domain.Enums;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly DapperContext _context;

    public TransactionsController(DapperContext context)
    {
        _context = context;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    // GET: api/transactions/account/1
    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByAccount(int accountId)
    {
        using var connection = _context.CreateConnection();
        
        // SQL: Hem gönderen hem de alıcı olduğu işlemleri getir
        var sql = @"
            SELECT * FROM transactions 
            WHERE from_account_id = @AccountId OR to_account_id = @AccountId 
            ORDER BY transaction_date DESC";

        var transactions = await connection.QueryAsync<Transaction>(sql, new { AccountId = accountId });

        return Ok(transactions);
    }

    // POST: api/transactions/transfer
    // Dapper ile ACID Transaction Yönetimi
    [HttpPost("transfer")]
    public async Task<IActionResult> TransferMoney([FromBody] TransferRequest request)
    {
        using var connection = _context.CreateConnection();
        connection.Open(); // Transaction başlatmak için bağlantıyı manuel açıyoruz

        // 1. Transaction Başlat (Hata olursa her şeyi geri almak için)
        using var transaction = connection.BeginTransaction();

        try
        {
            // 2. Gönderen Hesabı Kontrol Et (Bakiyeyi korumak için işlem içinde okuyoruz)
            var accountSql = "SELECT * FROM accounts WHERE account_id = @Id";
            var fromAccount = await connection.QuerySingleOrDefaultAsync<Account>(accountSql, new { Id = request.FromAccountId }, transaction);

            if (fromAccount == null) return BadRequest("Gönderen hesap bulunamadı.");
            if (fromAccount.AvailableBalance < request.Amount) return BadRequest("Yetersiz bakiye.");

            // 3. Alıcı Hesabı Kontrol Et
            var toAccount = await connection.QuerySingleOrDefaultAsync<Account>(accountSql, new { Id = request.ToAccountId }, transaction);
            if (toAccount == null) return BadRequest("Alıcı hesap bulunamadı.");

            // 4. Parayı Gönderenden Düş (SQL UPDATE)
            var deductSql = @"
                UPDATE accounts 
                SET balance = balance - @Amount, 
                    available_balance = available_balance - @Amount,
                    updated_at = GETDATE()
                WHERE account_id = @Id";
            
            await connection.ExecuteAsync(deductSql, new { request.Amount, Id = request.FromAccountId }, transaction);

            // 5. Parayı Alıcıya Ekle (SQL UPDATE)
            var addSql = @"
                UPDATE accounts 
                SET balance = balance + @Amount, 
                    available_balance = available_balance + @Amount,
                    updated_at = GETDATE()
                WHERE account_id = @Id";

            await connection.ExecuteAsync(addSql, new { request.Amount, Id = request.ToAccountId }, transaction);

            // 6. İşlem Kaydı Oluştur (INSERT)
            var insertTxSql = @"
                INSERT INTO transactions (
                    from_account_id, to_account_id, amount, currency, transaction_type, 
                    description, reference_number, status, transaction_date, 
                    requires_review, fraud_score, is_suspicious, reported_to_masak
                )
                VALUES (
                    @FromId, @ToId, @Amount, @Currency, @Type, 
                    @Desc, @Ref, @Status, GETDATE(), 
                    0, 0, 0, 0
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var txParams = new
            {
                FromId = request.FromAccountId,
                ToId = request.ToAccountId,
                request.Amount,
                Currency = fromAccount.Currency.ToString(),       // Enum -> String
                Type = TransactionType.Transfer.ToString(),       // Enum -> String
                Desc = request.Description,
                Ref = Guid.NewGuid().ToString(),                  // Benzersiz referans no
                Status = TransactionStatus.Completed.ToString()   // Enum -> String
            };

            var newTxId = await connection.QuerySingleAsync<int>(insertTxSql, txParams, transaction);

            // 7. Her şey başarılıysa VERİTABANINA İŞLE (COMMIT)
            transaction.Commit();

            return Ok(new { Message = "Transfer başarılı", TransactionId = newTxId, Reference = txParams.Ref });
        }
        catch (Exception ex)
        {
            // Hata olursa HER ŞEYİ GERİ AL (ROLLBACK)
            transaction.Rollback();
            return StatusCode(500, $"Transfer sırasında hata oluştu: {ex.Message}");
        }
    }
}

// DTO
public class TransferRequest
{
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}