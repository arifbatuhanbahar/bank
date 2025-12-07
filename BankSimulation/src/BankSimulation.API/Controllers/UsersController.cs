using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DapperContext _context;

    public UsersController(DapperContext context)
    {
        _context = context;
        // Dapper Ayarı: "user_id" (SQL) ile "UserId" (C#) eşleşmesini otomatik yap
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        // 1. Bağlantıyı oluştur (Using bloğu iş bitince bağlantıyı kapatır)
        using var connection = _context.CreateConnection();

        // 2. SQL Sorgusu (Saf SQL)
        // Silinmemiş kullanıcıları getiriyoruz
        var sql = "SELECT * FROM users WHERE deleted_at IS NULL";

        // 3. Sorguyu çalıştır
        var users = await connection.QueryAsync<User>(sql);

        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        using var connection = _context.CreateConnection();

        // Parametreli SQL (Güvenlik için @Id kullanıyoruz)
        var sql = "SELECT * FROM users WHERE user_id = @Id AND deleted_at IS NULL";

        // SingleOrDefault: Tek kayıt döner veya null döner
        var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        using var connection = _context.CreateConnection();

        // SQL: Insert işlemi
        // Not: Enums (Status, KycStatus) veritabanında string olduğu için .ToString() ile gönderiyoruz.
        var sql = @"
            INSERT INTO users (
                tc_kimlik_no, first_name, last_name, date_of_birth, email, 
                password_hash, password_salt, status, kyc_status, risk_level, 
                created_at, is_pep
            ) 
            VALUES (
                @TcKimlikNo, @FirstName, @LastName, @DateOfBirth, @Email, 
                @PasswordHash, @PasswordSalt, @Status, @KycStatus, @RiskLevel, 
                GETDATE(), @IsPep
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        // Parametreleri hazırla (Enum'ları string'e çeviriyoruz)
        var parameters = new
        {
            user.TcKimlikNo,
            user.FirstName,
            user.LastName,
            user.DateOfBirth,
            user.Email,
            user.PasswordHash,
            user.PasswordSalt,
            Status = user.Status.ToString(),       // Enum -> String
            KycStatus = user.KycStatus.ToString(), // Enum -> String
            RiskLevel = user.RiskLevel.ToString(), // Enum -> String
            user.IsPep
        };

        // Sorguyu çalıştır ve oluşan yeni ID'yi al
        var newId = await connection.QuerySingleAsync<int>(sql, parameters);
        
        // Nesneye ID'yi ata
        user.UserId = newId;

        return CreatedAtAction(nameof(GetUser), new { id = newId }, user);
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        using var connection = _context.CreateConnection();

        // Soft Delete: Kaydı silmek yerine 'deleted_at' tarihini dolduruyoruz
        var sql = "UPDATE users SET deleted_at = GETDATE() WHERE user_id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        if (rowsAffected == 0)
        {
            return NotFound();
        }

        return NoContent();
    }
}