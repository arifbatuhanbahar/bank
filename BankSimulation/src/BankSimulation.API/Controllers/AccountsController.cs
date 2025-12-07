using BankSimulation.Domain.Entities.AccountManagement;
using BankSimulation.Domain.Enums;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly DapperContext _context;

    public AccountsController(DapperContext context)
    {
        _context = context;
        // SQL'deki "account_id" ile C#'taki "AccountId" eşleşmesi için
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    // GET: api/accounts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM accounts";
        var accounts = await connection.QueryAsync<Account>(sql);
        return Ok(accounts);
    }

    // GET: api/accounts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM accounts WHERE account_id = @Id";
        var account = await connection.QuerySingleOrDefaultAsync<Account>(sql, new { Id = id });

        if (account == null) return NotFound();
        return Ok(account);
    }

    // GET: api/accounts/user/5
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccountsByUser(int userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM accounts WHERE user_id = @UserId";
        var accounts = await connection.QueryAsync<Account>(sql, new { UserId = userId });
        return Ok(accounts);
    }

    // POST: api/accounts
    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccount(CreateAccountRequest request)
    {
        using var connection = _context.CreateConnection();

        // 1. Kullanıcı var mı kontrolü (SQL ile)
        var userCheckSql = "SELECT COUNT(1) FROM users WHERE user_id = @UserId";
        var userExists = await connection.ExecuteScalarAsync<bool>(userCheckSql, new { request.UserId });

        if (!userExists) return BadRequest("Kullanıcı bulunamadı.");

        // 2. Hesap Oluşturma (Saf SQL INSERT)
        var sql = @"
            INSERT INTO accounts (
                user_id, account_number, account_type, currency, balance, available_balance,
                daily_transfer_limit, daily_withdrawal_limit, interest_rate, status,
                opened_date, created_at
            )
            VALUES (
                @UserId, @AccountNumber, @AccountType, @Currency, @Balance, @Balance,
                @DailyTransferLimit, @DailyWithdrawalLimit, @InterestRate, @Status,
                GETDATE(), GETDATE()
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var parameters = new
        {
            request.UserId,
            request.AccountNumber,
            AccountType = request.AccountType.ToString(), // Enum -> String
            Currency = request.Currency.ToString(),       // Enum -> String
            request.Balance,
            request.DailyTransferLimit,
            request.DailyWithdrawalLimit,
            request.InterestRate,
            Status = AccountStatus.Active.ToString()      // Enum -> String
        };

        var newId = await connection.QuerySingleAsync<int>(sql, parameters);

        // Oluşan hesabı döndürmek için nesneyi güncelle
        var createdAccount = new Account
        {
            AccountId = newId,
            UserId = request.UserId,
            AccountNumber = request.AccountNumber,
            Balance = request.Balance
        };

        return CreatedAtAction(nameof(GetAccount), new { id = newId }, createdAccount);
    }

    // PUT: api/accounts/5/balance
    [HttpPut("{id}/balance")]
    public async Task<IActionResult> UpdateBalance(int id, [FromBody] decimal newBalance)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            UPDATE accounts 
            SET balance = @NewBalance, 
                available_balance = @NewBalance,
                updated_at = GETDATE()
            WHERE account_id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, NewBalance = newBalance });

        if (rowsAffected == 0) return NotFound();

        return Ok(new { Message = "Bakiye güncellendi." });
    }
}

// DTO sınıfı (Aynı kalıyor, silmeye gerek yok)
public class CreateAccountRequest
{
    public int UserId { get; set; }
    public string AccountNumber { get; set; } = null!;
    public AccountType AccountType { get; set; }
    public Currency Currency { get; set; }
    public decimal Balance { get; set; }
    public decimal DailyTransferLimit { get; set; }
    public decimal DailyWithdrawalLimit { get; set; }
    public decimal InterestRate { get; set; }
}