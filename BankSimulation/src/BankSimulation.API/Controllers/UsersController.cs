using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

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

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            UPDATE users
            SET first_name = @FirstName,
                last_name = @LastName,
                status = @Status,
                kyc_status = @KycStatus,
                risk_level = @RiskLevel,
                updated_at = GETDATE()
            WHERE user_id = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            request.FirstName,
            request.LastName,
            Status = request.Status ?? "Active",
            KycStatus = request.KycStatus ?? "Pending",
            RiskLevel = request.RiskLevel ?? "Low"
        });

        if (rows == 0) return NotFound();

        var updated = await connection.QuerySingleAsync<User>(
            "SELECT * FROM users WHERE user_id = @Id",
            new { Id = id });

        return Ok(updated);
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

    // POST: api/users/{id}/change-password
    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        using var connection = _context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var exists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM users WHERE user_id = @Id",
                new { Id = id }, transaction);

            if (exists == 0)
            {
                transaction.Rollback();
                return NotFound();
            }

            var hashed = ComputeHash(request.NewPassword);
            var salt = Guid.NewGuid().ToString("N");

            var updateSql = @"
                UPDATE users 
                SET password_hash = @Hash, password_salt = @Salt, password_changed_at = GETDATE()
                WHERE user_id = @Id";

            await connection.ExecuteAsync(updateSql, new { Hash = hashed, Salt = salt, Id = id }, transaction);

            var historySql = @"
                INSERT INTO password_history (user_id, password_hash, changed_at, changed_by)
                VALUES (@UserId, @Hash, GETDATE(), @UserId)";

            await connection.ExecuteAsync(historySql, new { UserId = id, Hash = hashed }, transaction);

            transaction.Commit();
            return Ok(new { Message = "Şifre güncellendi." });
        }
        catch
        {
            transaction.Rollback();
            return StatusCode(500, "Şifre güncellenemedi.");
        }
    }

    // GET: api/users/{id}/password-history
    [HttpGet("{id}/password-history")]
    public async Task<ActionResult<IEnumerable<PasswordHistory>>> GetPasswordHistory(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            SELECT TOP 10 * FROM password_history
            WHERE user_id = @UserId
            ORDER BY changed_at DESC";

        var history = await connection.QueryAsync<PasswordHistory>(sql, new { UserId = id });
        return Ok(history);
    }

    /// <summary>
    /// Demo amaçlı: Tüm kullanıcıların şifrelerini aynı değere sıfırlar (varsayılan demo123).
    /// Güvenlik açısından gerçek ortamda kullanmayın.
    /// </summary>
    [HttpPost("reset-passwords")]
    public async Task<IActionResult> ResetAllPasswords([FromQuery] string newPassword = "demo123")
    {
        using var connection = _context.CreateConnection();
        var hash = ComputeHash(newPassword);
        var salt = Guid.NewGuid().ToString("N");

        var sql = @"
            UPDATE users
            SET password_hash = @Hash,
                password_salt = @Salt,
                password_changed_at = GETDATE()";

        var rows = await connection.ExecuteAsync(sql, new { Hash = hash, Salt = salt });

        // Tarihçeye tek kayıt eklemek isterseniz burada ekleyebilirsiniz; basit tutuyoruz.
        return Ok(new { Updated = rows, Message = $"Tüm şifreler '{newPassword}' olarak sıfırlandı (hashlenmiş)." });
    }

    private static string ComputeHash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = null!;
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Status { get; set; }
    public string? KycStatus { get; set; }
    public string? RiskLevel { get; set; }
}
