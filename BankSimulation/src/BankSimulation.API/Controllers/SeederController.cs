using Dapper;
using BankSimulation.API.Services;
using BankSimulation.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace BankSimulation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeederController : ControllerBase
    {
        private readonly DataSeeder _dataSeeder;
        private readonly DapperContext _context;

        public SeederController(DataSeeder dataSeeder, DapperContext context)
        {
            _dataSeeder = dataSeeder;
            _context = context;
        }

        /// <summary>
        /// Tüm tablolara test verisi ekler (38 tablo)
        /// </summary>
        [HttpPost("seed-all")]
        public async Task<IActionResult> SeedAll()
        {
            try
            {
                await _dataSeeder.SeedAllDataAsync();
                var stats = await GetStatsInternal();
                return Ok(new
                {
                    success = true,
                    message = "✅ Tüm veriler başarıyla eklendi!",
                    timestamp = DateTime.Now,
                    statistics = stats
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "❌ Hata oluştu!",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Veritabanı istatistiklerini döndürür
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await GetStatsInternal();
                return Ok(new
                {
                    success = true,
                    timestamp = DateTime.Now,
                    statistics = stats
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Detaylı tablo bazında kayıt sayıları
        /// </summary>
        [HttpGet("table-counts")]
        public async Task<IActionResult> GetTableCounts()
        {
            try
            {
                using var connection = _context.CreateConnection();
                
                var tables = new Dictionary<string, int>
                {
                    // Modül 1: User Management
                    ["users"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM users"),
                    ["user_roles"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM user_roles"),
                    ["user_sessions"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM user_sessions"),
                    ["login_attempts"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM login_attempts"),
                    ["password_history"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM password_history"),
                    
                    // Modül 2: Account Management
                    ["account_types"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_types"),
                    ["accounts"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM accounts"),
                    ["account_beneficiaries"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_beneficiaries"),
                    ["account_limits"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_limits"),
                    
                    // Modül 3: Transaction Management
                    ["transaction_types"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transaction_types"),
                    ["transactions"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transactions"),
                    ["transaction_fees"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transaction_fees"),
                    ["scheduled_transactions"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM scheduled_transactions"),
                    ["transaction_approvals"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transaction_approvals"),
                    ["general_ledger"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM general_ledger"),
                    
                    // Modül 4: Payment & Cards
                    ["credit_cards"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM credit_cards"),
                    ["card_transactions"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM card_transactions"),
                    ["payment_methods"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM payment_methods"),
                    ["recurring_payments"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM recurring_payments"),
                    ["payment_gateways"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM payment_gateways"),
                    
                    // Modül 5: Compliance & KYC
                    ["kyc_documents"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kyc_documents"),
                    ["kyc_verifications"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kyc_verifications"),
                    ["kvkk_consents"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kvkk_consents"),
                    ["kvkk_data_requests"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kvkk_data_requests"),
                    ["masak_records"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM masak_records"),
                    ["suspicious_activity_reports"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM suspicious_activity_reports"),
                    
                    // Modül 6: Audit & Security
                    ["audit_logs"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM audit_logs"),
                    ["security_events"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM security_events"),
                    ["data_access_log"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM data_access_log"),
                    ["pci_audit_log"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM pci_audit_log"),
                    ["encryption_keys"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM encryption_keys"),
                    
                    // Modül 7: Fraud & Risk
                    ["fraud_rules"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM fraud_rules"),
                    ["fraud_alerts"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM fraud_alerts"),
                    ["risk_profiles"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM risk_profiles"),
                    
                    // Modül 8: System & Config
                    ["system_settings"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM system_settings"),
                    ["notification_templates"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM notification_templates"),
                    
                    // Modül 9: Credit Card Applications
                    ["card_applications"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM card_applications"),
                    ["card_limits"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM card_limits")
                };

                var totalRecords = tables.Values.Sum();

                return Ok(new
                {
                    success = true,
                    totalTables = 38,
                    totalRecords = totalRecords,
                    tables = tables
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Tüm verileri siler (DİKKAT!)
        /// </summary>
        [HttpDelete("clear-all")]
        public async Task<IActionResult> ClearAll()
        {
            try
            {
                using var connection = _context.CreateConnection();

                // Sıralama önemli - FK ilişkileri nedeniyle
                var deleteOrder = new[]
                {
                    // Önce bağımlı tablolar
                    "card_limits", "card_applications",
                    "notification_templates", "system_settings",
                    "risk_profiles", "fraud_alerts", "fraud_rules",
                    "encryption_keys", "pci_audit_log", "data_access_log", "security_events", "audit_logs",
                    "suspicious_activity_reports", "masak_records", "kvkk_data_requests", "kvkk_consents", "kyc_verifications", "kyc_documents",
                    "payment_gateways", "recurring_payments", "payment_methods", "card_transactions", "credit_cards",
                    "general_ledger", "transaction_approvals", "scheduled_transactions", "transaction_fees", "transactions", "transaction_types",
                    "account_limits", "account_beneficiaries", "accounts", "account_types",
                    "password_history", "login_attempts", "user_sessions", "user_roles", "users"
                };

                foreach (var table in deleteOrder)
                {
                    await connection.ExecuteAsync($"DELETE FROM [{table}]");
                }

                // Identity'leri sıfırla
                foreach (var table in deleteOrder)
                {
                    try
                    {
                        await connection.ExecuteAsync($"DBCC CHECKIDENT('[{table}]', RESEED, 0)");
                    }
                    catch { /* Bazı tablolarda identity olmayabilir */ }
                }

                return Ok(new
                {
                    success = true,
                    message = "⚠️ Tüm veriler silindi!",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        private async Task<object> GetStatsInternal()
        {
            using var connection = _context.CreateConnection();

            return new
            {
                // Modül 1: User Management
                users = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM users"),
                userRoles = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM user_roles"),
                userSessions = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM user_sessions"),
                loginAttempts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM login_attempts"),
                passwordHistory = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM password_history"),

                // Modül 2: Account Management
                accountTypes = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_types"),
                accounts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM accounts"),
                accountBeneficiaries = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_beneficiaries"),
                accountLimits = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_limits"),

                // Modül 3: Transaction Management
                transactionTypes = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transaction_types"),
                transactions = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transactions"),
                transactionFees = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transaction_fees"),
                scheduledTransactions = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM scheduled_transactions"),
                transactionApprovals = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transaction_approvals"),
                generalLedger = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM general_ledger"),

                // Modül 4: Payment & Cards
                creditCards = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM credit_cards"),
                cardTransactions = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM card_transactions"),
                paymentMethods = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM payment_methods"),
                recurringPayments = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM recurring_payments"),
                paymentGateways = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM payment_gateways"),

                // Modül 5: Compliance & KYC
                kycDocuments = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kyc_documents"),
                kycVerifications = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kyc_verifications"),
                kvkkConsents = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kvkk_consents"),
                kvkkDataRequests = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kvkk_data_requests"),
                masakRecords = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM masak_records"),
                suspiciousActivityReports = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM suspicious_activity_reports"),

                // Modül 6: Audit & Security
                auditLogs = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM audit_logs"),
                securityEvents = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM security_events"),
                dataAccessLog = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM data_access_log"),
                pciAuditLog = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM pci_audit_log"),
                encryptionKeys = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM encryption_keys"),

                // Modül 7: Fraud & Risk
                fraudRules = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM fraud_rules"),
                fraudAlerts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM fraud_alerts"),
                riskProfiles = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM risk_profiles"),

                // Modül 8: System & Config
                systemSettings = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM system_settings"),
                notificationTemplates = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM notification_templates"),

                // Modül 9: Credit Card Applications
                cardApplications = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM card_applications"),
                cardLimits = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM card_limits"),

                // Özet bilgiler
                totalBalance_TRY = await connection.ExecuteScalarAsync<decimal>("SELECT ISNULL(SUM(balance), 0) FROM accounts WHERE currency = 'TRY'"),
                totalBalance_USD = await connection.ExecuteScalarAsync<decimal>("SELECT ISNULL(SUM(balance), 0) FROM accounts WHERE currency = 'USD'"),
                totalBalance_EUR = await connection.ExecuteScalarAsync<decimal>("SELECT ISNULL(SUM(balance), 0) FROM accounts WHERE currency = 'EUR'"),
                completedTransactionsAmount = await connection.ExecuteScalarAsync<decimal>("SELECT ISNULL(SUM(amount), 0) FROM transactions WHERE status = 'Completed'"),
                activeUsers = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM users WHERE status = 'Active'"),
                activeCreditCards = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM credit_cards WHERE status = 'Active'"),
                openFraudAlerts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM fraud_alerts WHERE status = 'Open'"),
                pendingKycDocuments = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kyc_documents WHERE verification_status = 'Pending'")
            };
        }
    }
}
