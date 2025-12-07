using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BankSimulation.Infrastructure.Data;

public class DapperContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
        // appsettings.json dosyasindaki "DefaultConnection" adresini okur
        _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
    }

    // Her cagrida yeni bir veritabani baglantisi olusturur
    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);
}
