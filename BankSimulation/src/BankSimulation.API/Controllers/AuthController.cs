using System.Security.Cryptography;
using System.Text;
using BankSimulation.Domain.Entities.UserManagement;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DapperContext _context;

    public AuthController(DapperContext context)
    {
        _context = context;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        using var connection = _context.CreateConnection();
        var user = await connection.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM users WHERE email = @Email AND deleted_at IS NULL",
            new { request.Email });

        if (user == null)
            return Unauthorized("Kullanıcı bulunamadı.");

        // Hem düz metin (eski kayıtlar) hem SHA256 hash kontrolü
        var hashWithoutSalt = ComputeHash(request.Password);
        var hashWithSalt = string.IsNullOrWhiteSpace(user.PasswordSalt)
            ? null
            : ComputeHash(request.Password, user.PasswordSalt);

        // Demo/gelistirme icin ortak parola (istek: demo123)
        var demoPassword = "demo123";

        var match = user.PasswordHash == request.Password ||
                    user.PasswordHash == hashWithoutSalt ||
                    user.PasswordHash == hashWithSalt ||
                    request.Password == demoPassword;

        if (!match)
            return Unauthorized("Şifre hatalı.");

        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{user.UserId}:{user.Email}:{Guid.NewGuid()}"));

        var expiresAt = DateTime.UtcNow.AddHours(1);

        return Ok(new
        {
            user,
            token,
            expiresAt
        });
    }

    private static string ComputeHash(string input, string? salt = null)
    {
        using var sha = SHA256.Create();
        var payload = salt is null ? input : input + salt;
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(bytes);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
