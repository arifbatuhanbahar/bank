using System.Data;
using BankSimulation.Domain.Entities.Compliance;
using BankSimulation.Domain.Enums;
using BankSimulation.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComplianceController : ControllerBase
{
    private readonly DapperContext _context;

    public ComplianceController(DapperContext context)
    {
        _context = context;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    [HttpGet("kyc-documents/{userId}")]
    public async Task<ActionResult<IEnumerable<KycDocument>>> GetKycDocuments(int userId)
    {
        using var connection = _context.CreateConnection();
        var docs = await connection.QueryAsync<KycDocument>(
            "SELECT TOP 50 * FROM kyc_documents WHERE user_id = @UserId ORDER BY upload_date DESC",
            new { UserId = userId });
        return Ok(docs);
    }

    [HttpPost("upload-document")]
    public async Task<ActionResult> UploadDocument(KycDocumentRequest request)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            INSERT INTO kyc_documents (
                user_id, document_type, document_number, document_file_path, 
                document_hash, upload_date, verification_status
            )
            VALUES (
                @UserId, @DocumentType, @DocumentNumber, @FilePath, 
                'A1B2C3D4', GETDATE(), 'Pending'
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var filePath = $"/uploads/users/{request.UserId}/{Guid.NewGuid()}.pdf";

        var id = await connection.QuerySingleAsync<int>(sql, new
        {
            request.UserId,
            DocumentType = request.DocumentType.ToString(),
            request.DocumentNumber,
            FilePath = filePath
        });

        return Ok(new { Message = "Belge yüklendi", DocumentId = id, FilePath = filePath });
    }

    [HttpPost("verify-document")]
    public async Task<IActionResult> VerifyDocument(int documentId, bool isApproved, string? rejectionReason)
    {
        using var connection = _context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var status = isApproved ? VerificationStatus.Verified.ToString() : VerificationStatus.Rejected.ToString();

            var updateDocSql = @"
                UPDATE kyc_documents 
                SET verification_status = @Status, 
                    verified_at = GETDATE(), 
                    rejection_reason = @Reason, 
                    verified_by = 1 
                WHERE document_id = @Id";

            await connection.ExecuteAsync(updateDocSql, new { Status = status, Reason = rejectionReason, Id = documentId }, transaction);

            if (isApproved)
            {
                var userId = await connection.QuerySingleAsync<int>("SELECT user_id FROM kyc_documents WHERE document_id = @Id", new { Id = documentId }, transaction);
                var updateUserSql = "UPDATE users SET kyc_status = 'Verified' WHERE user_id = @UserId";
                await connection.ExecuteAsync(updateUserSql, new { UserId = userId }, transaction);
            }

            transaction.Commit();
            return Ok($"Belge durumu güncellendi: {status}");
        }
        catch
        {
            transaction.Rollback();
            return StatusCode(500, "Onaylama hatası");
        }
    }

    [HttpPost("report-suspicious-activity")]
    public async Task<IActionResult> ReportActivity(SarRequest request)
    {
        using var connection = _context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var riskScore = 85;
            var isMasak = riskScore > 80;

            var sarSql = @"
                INSERT INTO suspicious_activity_reports (
                    user_id, transaction_id, report_type, description, risk_score, 
                    status, created_at, created_by, reported_to_masak, masak_report_date
                )
                VALUES (
                    @UserId, @TransactionId, @ReportType, @Description, @RiskScore, 
                    'Draft', GETDATE(), 1, @IsMasak, @MasakDate
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var sarId = await connection.QuerySingleAsync<int>(sarSql, new
            {
                request.UserId,
                request.TransactionId,
                ReportType = request.ReportType.ToString(),
                request.Description,
                RiskScore = riskScore,
                IsMasak = isMasak,
                MasakDate = isMasak ? (DateTime?)DateTime.Now : null
            }, transaction);

            if (isMasak)
            {
                var masakSql = @"
                    INSERT INTO masak_records (
                        customer_id, transaction_id, record_type, data, 
                        created_at, retention_until
                    )
                    VALUES (
                        @UserId, @TransactionId, 'SuspiciousReport', 
                        @Data, GETDATE(), DATEADD(year, 10, GETDATE())
                    );";

                await connection.ExecuteAsync(masakSql, new
                {
                    request.UserId,
                    request.TransactionId,
                    Data = $"{{ 'reason': '{request.Description}', 'sar_id': {sarId} }}"
                }, transaction);
            }

            transaction.Commit();
            return Ok(new { Message = "Bildirim yapıldı", ReportId = sarId });
        }
        catch
        {
            transaction.Rollback();
            return StatusCode(500, "Raporlama hatası");
        }
    }

    [HttpGet("consents/{userId}")]
    public async Task<ActionResult<IEnumerable<KvkkConsent>>> GetConsents(int userId)
    {
        using var connection = _context.CreateConnection();
        var consents = await connection.QueryAsync<KvkkConsent>(
            "SELECT * FROM kvkk_consents WHERE user_id = @UserId ORDER BY granted_at DESC",
            new { UserId = userId });
        return Ok(consents);
    }

    [HttpPost("consents")]
    public async Task<IActionResult> UpsertConsent(KvkkConsentRequest request)
    {
        using var connection = _context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var exists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM kvkk_consents WHERE user_id = @UserId AND consent_type = @Type",
                new { request.UserId, Type = request.ConsentType.ToString() }, transaction);

            if (exists > 0)
            {
                var updateSql = @"
                    UPDATE kvkk_consents
                    SET consent_given = @Given,
                        consent_text = @Text,
                        consent_version = @Version,
                        granted_at = GETDATE(),
                        revoked_at = CASE WHEN @Given = 0 THEN GETDATE() ELSE NULL END,
                        ip_address = @Ip
                    WHERE user_id = @UserId AND consent_type = @Type;";

                await connection.ExecuteAsync(updateSql, new
                {
                    request.UserId,
                    Type = request.ConsentType.ToString(),
                    Given = request.ConsentGiven,
                    Text = request.ConsentText ?? "KVKK Aydinlatma Metni",
                    Version = request.ConsentVersion ?? "v1.0",
                    Ip = request.IpAddress
                }, transaction);
            }
            else
            {
                var insertSql = @"
                    INSERT INTO kvkk_consents (
                        user_id, consent_type, consent_given, consent_text,
                        consent_version, granted_at, revoked_at, ip_address
                    )
                    VALUES (
                        @UserId, @Type, @Given, @Text,
                        @Version, GETDATE(), NULL, @Ip
                    );";

                await connection.ExecuteAsync(insertSql, new
                {
                    request.UserId,
                    Type = request.ConsentType.ToString(),
                    Given = request.ConsentGiven,
                    Text = request.ConsentText ?? "KVKK Aydinlatma Metni",
                    Version = request.ConsentVersion ?? "v1.0",
                    Ip = request.IpAddress
                }, transaction);
            }

            transaction.Commit();
            return Ok(new { Message = "Rıza kaydı güncellendi." });
        }
        catch
        {
            transaction.Rollback();
            return StatusCode(500, "Rıza kaydı güncellenemedi.");
        }
    }

    [HttpGet("kvkk-requests/{userId}")]
    public async Task<ActionResult<IEnumerable<KvkkDataRequest>>> GetKvkkRequests(int userId)
    {
        using var connection = _context.CreateConnection();
        var requests = await connection.QueryAsync<KvkkDataRequest>(
            "SELECT * FROM kvkk_data_requests WHERE user_id = @UserId ORDER BY request_date DESC",
            new { UserId = userId });
        return Ok(requests);
    }

    [HttpPost("kvkk-requests")]
    public async Task<IActionResult> CreateKvkkRequest(KvkkDataRequestCreate request)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO kvkk_data_requests (user_id, request_type, status, request_date)
            VALUES (@UserId, @Type, 'Pending', GETDATE());
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await connection.QuerySingleAsync<int>(sql, new
        {
            request.UserId,
            Type = request.RequestType.ToString()
        });

        return Ok(new { RequestId = id, Message = "KVKK talebi alındı." });
    }
}

public class KycDocumentRequest
{
    public int UserId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = null!;
}

public class SarRequest
{
    public int UserId { get; set; }
    public int? TransactionId { get; set; }
    public SarReportType ReportType { get; set; }
    public string Description { get; set; } = null!;
}

public class KvkkConsentRequest
{
    public int UserId { get; set; }
    public ConsentType ConsentType { get; set; }
    public bool ConsentGiven { get; set; }
    public string? ConsentText { get; set; }
    public string? ConsentVersion { get; set; }
    public string? IpAddress { get; set; }
}

public class KvkkDataRequestCreate
{
    public int UserId { get; set; }
    public KvkkRequestType RequestType { get; set; }
}
