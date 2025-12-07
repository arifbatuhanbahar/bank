using Bogus;
using Dapper;
using BankSimulation.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace BankSimulation.API.Services;

public class DataSeeder
{
    private readonly DapperContext _context;
    private static readonly Random _random = new Random();

    public DataSeeder(DapperContext context)
    {
        _context = context;
    }

    public async Task SeedAllDataAsync()
    {
        using var connection = _context.CreateConnection();
        connection.Open();

        // MODÜL 1: User Management
        await SeedUsersAsync(connection);
        await SeedUserRolesAsync(connection);
        await SeedUserSessionsAsync(connection);
        await SeedLoginAttemptsAsync(connection);
        await SeedPasswordHistoryAsync(connection);

        // MODÜL 2: Account Management
        await SeedAccountTypesAsync(connection);
        await SeedAccountsAsync(connection);
        await SeedAccountBeneficiariesAsync(connection);
        await SeedAccountLimitsAsync(connection);

        // MODÜL 3: Transaction Management
        await SeedTransactionTypesAsync(connection);
        await SeedTransactionsAsync(connection);
        await SeedTransactionFeesAsync(connection);
        await SeedScheduledTransactionsAsync(connection);
        await SeedTransactionApprovalsAsync(connection);
        await SeedGeneralLedgerAsync(connection);

        // MODÜL 4: Payment & Cards
        await SeedCreditCardsAsync(connection);
        await SeedCardTransactionsAsync(connection);
        await SeedPaymentMethodsAsync(connection);
        await SeedRecurringPaymentsAsync(connection);
        await SeedPaymentGatewaysAsync(connection);

        // MODÜL 5: Compliance & KYC
        await SeedKycDocumentsAsync(connection);
        await SeedKycVerificationsAsync(connection);
        await SeedKvkkConsentsAsync(connection);
        await SeedKvkkDataRequestsAsync(connection);
        await SeedMasakRecordsAsync(connection);
        await SeedSuspiciousActivityReportsAsync(connection);

        // MODÜL 6: Audit & Security
        await SeedAuditLogsAsync(connection);
        await SeedSecurityEventsAsync(connection);
        await SeedDataAccessLogAsync(connection);
        await SeedPciAuditLogAsync(connection);
        await SeedEncryptionKeysAsync(connection);

        // MODÜL 7: Fraud & Risk
        await SeedFraudRulesAsync(connection);
        await SeedFraudAlertsAsync(connection);
        await SeedRiskProfilesAsync(connection);

        // MODÜL 8: System & Config
        await SeedSystemSettingsAsync(connection);
        await SeedNotificationTemplatesAsync(connection);

        // MODÜL 9: Credit Card Applications
        await SeedCardApplicationsAsync(connection);
        await SeedCardLimitsAsync(connection);
    }

    // ==================== MODÜL 1: USER MANAGEMENT ====================

    private async Task SeedUsersAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM users");
        if (count > 0) { Console.WriteLine($"Users: {count} kayıt var, atlanıyor..."); return; }

        var faker = new Faker<UserSeed>("tr")
            .RuleFor(u => u.TcKimlikNo, f => f.Random.String2(11, "0123456789"))
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.DateOfBirth, f => f.Date.Between(new DateTime(1960, 1, 1), new DateTime(2000, 12, 31)))
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber("05## ### ## ##"))
            .RuleFor(u => u.AddressLine1, f => f.Address.StreetAddress())
            .RuleFor(u => u.AddressLine2, f => f.Address.SecondaryAddress())
            .RuleFor(u => u.City, f => f.PickRandom("İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Adana", "Konya", "Gaziantep"))
            .RuleFor(u => u.PostalCode, f => f.Address.ZipCode("#####"))
            .RuleFor(u => u.Status, f => f.PickRandom("Active", "Active", "Active", "Suspended", "Locked"))
            .RuleFor(u => u.KycStatus, f => f.PickRandom("Verified", "Verified", "Pending", "Rejected"))
            .RuleFor(u => u.RiskLevel, f => f.PickRandom("Low", "Low", "Medium", "High"))
            .RuleFor(u => u.IsPep, f => f.Random.Bool(0.05f));

        var users = faker.Generate(100);

        var sql = @"INSERT INTO users (tc_kimlik_no, first_name, last_name, date_of_birth, email, email_verified,
                    phone, phone_verified, address_line1, address_line2, city, postal_code, country,
                    password_hash, password_salt, password_changed_at, status, failed_login_attempts,
                    kyc_status, risk_level, is_pep, created_at, last_login_at)
                    VALUES (@TcKimlikNo, @FirstName, @LastName, @DateOfBirth, @Email, @EmailVerified,
                    @Phone, @PhoneVerified, @AddressLine1, @AddressLine2, @City, @PostalCode, 'TR',
                    @PasswordHash, @PasswordSalt, @PasswordChangedAt, @Status, @FailedAttempts,
                    @KycStatus, @RiskLevel, @IsPep, @CreatedAt, @LastLoginAt)";

        foreach (var user in users)
        {
            user.PasswordSalt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.PasswordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("Test123!" + user.PasswordSalt)));
            user.EmailVerified = _random.Next(100) < 80;
            user.PhoneVerified = _random.Next(100) < 70;
            user.PasswordChangedAt = DateTime.Now.AddDays(-_random.Next(1, 365));
            user.FailedAttempts = _random.Next(0, 3);
            user.CreatedAt = DateTime.Now.AddDays(-_random.Next(30, 730));
            user.LastLoginAt = DateTime.Now.AddDays(-_random.Next(0, 30));
            await connection.ExecuteAsync(sql, user);
        }
        Console.WriteLine("✅ 100 kullanıcı eklendi.");
    }

    private async Task SeedUserRolesAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM user_roles");
        if (count > 0) { Console.WriteLine($"User Roles: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var roles = new[] { "Customer", "Customer", "Customer", "Employee", "Admin", "Auditor" };

        var sql = @"INSERT INTO user_roles (user_id, role_name, assigned_at, assigned_by, expires_at)
                    VALUES (@UserId, @RoleName, @AssignedAt, @AssignedBy, @ExpiresAt)";

        foreach (var userId in userIds)
        {
            var role = roles[_random.Next(roles.Length)];
            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                RoleName = role,
                AssignedAt = DateTime.Now.AddDays(-_random.Next(1, 365)),
                AssignedBy = userIds[_random.Next(Math.Min(5, userIds.Count))],
                ExpiresAt = role == "Customer" ? (DateTime?)null : DateTime.Now.AddYears(1)
            });
        }
        Console.WriteLine("✅ 100 kullanıcı rolü eklendi.");
    }

    private async Task SeedUserSessionsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM user_sessions");
        if (count > 0) { Console.WriteLine($"User Sessions: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var devices = new[] { "iPhone 14 Pro", "Samsung Galaxy S23", "Windows PC", "MacBook Pro", "iPad Air" };
        var browsers = new[] { "Chrome/120.0", "Safari/17.0", "Firefox/121.0", "Edge/120.0" };

        var sql = @"INSERT INTO user_sessions (user_id, session_token, ip_address, user_agent, device_info,
                    created_at, expires_at, last_activity_at, is_active)
                    VALUES (@UserId, @Token, @Ip, @UserAgent, @Device, @Created, @Expires, @LastActivity, @IsActive)";

        for (int i = 0; i < 300; i++)
        {
            var created = DateTime.Now.AddHours(-_random.Next(1, 720));
            var isActive = _random.Next(100) < 20;
            await connection.ExecuteAsync(sql, new
            {
                UserId = userIds[_random.Next(userIds.Count)],
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                Ip = $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}",
                UserAgent = $"Mozilla/5.0 {browsers[_random.Next(browsers.Length)]}",
                Device = devices[_random.Next(devices.Length)],
                Created = created,
                Expires = created.AddHours(24),
                LastActivity = isActive ? DateTime.Now.AddMinutes(-_random.Next(1, 60)) : created.AddHours(_random.Next(1, 12)),
                IsActive = isActive
            });
        }
        Console.WriteLine("✅ 300 oturum kaydı eklendi.");
    }

    private async Task SeedLoginAttemptsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM login_attempts");
        if (count > 0) { Console.WriteLine($"Login Attempts: {count} kayıt var, atlanıyor..."); return; }

        var users = (await connection.QueryAsync<dynamic>("SELECT user_id, email FROM users")).ToList();
        var failReasons = new[] { "Invalid password", "Account locked", "Invalid email", "Session expired", null };

        var sql = @"INSERT INTO login_attempts (user_id, email_attempted, ip_address, success, failure_reason, attempted_at, user_agent)
                    VALUES (@UserId, @Email, @Ip, @Success, @Reason, @AttemptedAt, @UserAgent)";

        for (int i = 0; i < 500; i++)
        {
            var user = users[_random.Next(users.Count)];
            var success = _random.Next(100) < 85;
            await connection.ExecuteAsync(sql, new
            {
                UserId = success ? (int?)user.user_id : (_random.Next(100) < 50 ? (int?)user.user_id : null),
                Email = (string)user.email,
                Ip = $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}",
                Success = success,
                Reason = success ? null : failReasons[_random.Next(failReasons.Length - 1)],
                AttemptedAt = DateTime.Now.AddDays(-_random.Next(0, 90)).AddHours(-_random.Next(0, 24)),
                UserAgent = "Mozilla/5.0 Chrome/120.0"
            });
        }
        Console.WriteLine("✅ 500 giriş denemesi eklendi.");
    }

    private async Task SeedPasswordHistoryAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM password_history");
        if (count > 0) { Console.WriteLine($"Password History: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();

        var sql = @"INSERT INTO password_history (user_id, password_hash, changed_at, changed_by)
                    VALUES (@UserId, @Hash, @ChangedAt, @ChangedBy)";

        foreach (var userId in userIds)
        {
            var historyCount = _random.Next(1, 5);
            for (int i = 0; i < historyCount; i++)
            {
                await connection.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    Hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"OldPass{i}!" + Guid.NewGuid()))),
                    ChangedAt = DateTime.Now.AddDays(-_random.Next(30, 730)),
                    ChangedBy = _random.Next(100) < 90 ? (int?)userId : userIds[_random.Next(Math.Min(5, userIds.Count))]
                });
            }
        }
        Console.WriteLine("✅ Şifre geçmişi eklendi.");
    }

    // ==================== MODÜL 2: ACCOUNT MANAGEMENT ====================

    private async Task SeedAccountTypesAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_types");
        if (count > 0) { Console.WriteLine($"Account Types: {count} kayıt var, atlanıyor..."); return; }

        var types = new[]
        {
            new { TypeName = "Vadesiz Mevduat", Description = "Günlük bankacılık işlemleri için standart hesap", MinBalance = 0m, InterestRate = 0m, Features = "{\"atm\":true,\"eft\":true,\"fast\":true}" },
            new { TypeName = "Vadeli Mevduat", Description = "Belirli vade sonunda faiz kazancı sağlayan hesap", MinBalance = 1000m, InterestRate = 42.5m, Features = "{\"term_options\":[30,60,90,180,365]}" },
            new { TypeName = "Tasarruf Hesabı", Description = "Birikim amaçlı düşük faizli hesap", MinBalance = 100m, InterestRate = 15.0m, Features = "{\"auto_save\":true}" },
            new { TypeName = "Yatırım Hesabı", Description = "Hisse senedi ve fon alım satımı için hesap", MinBalance = 5000m, InterestRate = 0m, Features = "{\"stocks\":true,\"funds\":true,\"bonds\":true}" },
            new { TypeName = "Döviz Hesabı (USD)", Description = "ABD Doları cinsinden hesap", MinBalance = 100m, InterestRate = 3.5m, Features = "{\"currency\":\"USD\"}" },
            new { TypeName = "Döviz Hesabı (EUR)", Description = "Euro cinsinden hesap", MinBalance = 100m, InterestRate = 2.8m, Features = "{\"currency\":\"EUR\"}" },
            new { TypeName = "Altın Hesabı", Description = "Gram altın alım satım hesabı", MinBalance = 0m, InterestRate = 0m, Features = "{\"gold_type\":\"gram\"}" }
        };

        var sql = @"INSERT INTO account_types (type_name, description, min_balance, interest_rate, features, is_active)
                    VALUES (@TypeName, @Description, @MinBalance, @InterestRate, @Features, 1)";
        foreach (var t in types) await connection.ExecuteAsync(sql, t);
        Console.WriteLine("✅ 7 hesap türü eklendi.");
    }

    private async Task SeedAccountsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM accounts");
        if (count > 0) { Console.WriteLine($"Accounts: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var types = new[] { "Checking", "Checking", "Savings", "Investment" };
        var currencies = new[] { "TRY", "TRY", "TRY", "TRY", "USD", "EUR" };
        var statuses = new[] { "Active", "Active", "Active", "Active", "Frozen", "Closed" };

        var sql = @"INSERT INTO accounts (user_id, account_number, account_type, currency, balance, available_balance,
                    daily_transfer_limit, daily_withdrawal_limit, interest_rate, interest_calculation_date,
                    status, opened_date, created_at)
                    VALUES (@UserId, @AccNum, @Type, @Currency, @Balance, @Available, @TransferLimit, @WithdrawLimit,
                    @Interest, @InterestDate, @Status, @OpenedDate, @CreatedAt)";

        int total = 0;
        foreach (var userId in userIds)
        {
            var accCount = _random.Next(1, 4);
            for (int i = 0; i < accCount; i++)
            {
                var currency = currencies[_random.Next(currencies.Length)];
                var balance = currency == "TRY" ? _random.Next(100, 1000000) : _random.Next(50, 100000);
                var openedDate = DateTime.Now.AddDays(-_random.Next(30, 1000));
                await connection.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    AccNum = $"TR{_random.Next(10, 99)}000{_random.Next(100, 999)}{_random.Next(10000000, 99999999)}{_random.Next(10000000, 99999999)}",
                    Type = types[_random.Next(types.Length)],
                    Currency = currency,
                    Balance = balance,
                    Available = balance - _random.Next(0, Math.Min(1000, balance)),
                    TransferLimit = currency == "TRY" ? 100000m : 10000m,
                    WithdrawLimit = currency == "TRY" ? 20000m : 5000m,
                    Interest = currency == "TRY" ? _random.Next(0, 45) : _random.Next(0, 5),
                    InterestDate = DateTime.Now.AddDays(_random.Next(1, 30)),
                    Status = statuses[_random.Next(statuses.Length)],
                    OpenedDate = openedDate,
                    CreatedAt = openedDate
                });
                total++;
            }
        }
        Console.WriteLine($"✅ {total} hesap eklendi.");
    }

    private async Task SeedAccountBeneficiariesAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_beneficiaries");
        if (count > 0) { Console.WriteLine($"Account Beneficiaries: {count} kayıt var, atlanıyor..."); return; }

        var accountIds = (await connection.QueryAsync<int>("SELECT account_id FROM accounts")).ToList();
        var banks = new[] { "Ziraat Bankası", "İş Bankası", "Garanti BBVA", "Yapı Kredi", "Akbank", "QNB Finansbank", "Halkbank", "Vakıfbank" };
        var faker = new Faker("tr");

        var sql = @"INSERT INTO account_beneficiaries (account_id, beneficiary_name, beneficiary_iban, beneficiary_bank, nickname, is_verified, added_at)
                    VALUES (@AccountId, @Name, @Iban, @Bank, @Nickname, @Verified, @AddedAt)";

        for (int i = 0; i < 200; i++)
        {
            var name = faker.Name.FullName();
            await connection.ExecuteAsync(sql, new
            {
                AccountId = accountIds[_random.Next(accountIds.Count)],
                Name = name,
                Iban = $"TR{_random.Next(10, 99)}000{_random.Next(100, 999)}{_random.Next(10000000, 99999999)}{_random.Next(10000000, 99999999)}",
                Bank = banks[_random.Next(banks.Length)],
                Nickname = _random.Next(100) < 60 ? name.Split(' ')[0] : null,
                Verified = _random.Next(100) < 80,
                AddedAt = DateTime.Now.AddDays(-_random.Next(1, 365))
            });
        }
        Console.WriteLine("✅ 200 kayıtlı alıcı eklendi.");
    }

    private async Task SeedAccountLimitsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_limits");
        if (count > 0) { Console.WriteLine($"Account Limits: {count} kayıt var, atlanıyor..."); return; }

        var accountIds = (await connection.QueryAsync<int>("SELECT account_id FROM accounts")).ToList();
        var limitTypes = new[] { "DailyTransfer", "DailyWithdrawal", "Monthly" };

        var sql = @"INSERT INTO account_limits (account_id, limit_type, limit_amount, used_amount, reset_date, last_updated)
                    VALUES (@AccountId, @LimitType, @LimitAmount, @UsedAmount, @ResetDate, @LastUpdated)";

        foreach (var accountId in accountIds)
        {
            foreach (var limitType in limitTypes)
            {
                var limitAmount = limitType switch
                {
                    "DailyTransfer" => 100000m,
                    "DailyWithdrawal" => 20000m,
                    "Monthly" => 500000m,
                    _ => 50000m
                };
                await connection.ExecuteAsync(sql, new
                {
                    AccountId = accountId,
                    LimitType = limitType,
                    LimitAmount = limitAmount,
                    UsedAmount = _random.Next(0, (int)(limitAmount * 0.3m)),
                    ResetDate = limitType == "Monthly" ? DateTime.Now.AddDays(30 - DateTime.Now.Day) : DateTime.Now.Date.AddDays(1),
                    LastUpdated = DateTime.Now.AddHours(-_random.Next(1, 48))
                });
            }
        }
        Console.WriteLine("✅ Hesap limitleri eklendi.");
    }

    // ==================== MODÜL 3: TRANSACTION MANAGEMENT ====================

    private async Task SeedTransactionTypesAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transaction_types");
        if (count > 0) { Console.WriteLine($"Transaction Types: {count} kayıt var, atlanıyor..."); return; }

        var types = new[]
        {
            new { TypeName = "FAST Transfer", TypeCode = "FAST", Description = "Anlık para transferi (7/24)", FeeFixed = 2.50m, FeePercentage = 0m },
            new { TypeName = "EFT", TypeCode = "EFT", Description = "Elektronik Fon Transferi", FeeFixed = 5.00m, FeePercentage = 0m },
            new { TypeName = "Havale", TypeCode = "HAVALE", Description = "Banka içi para transferi", FeeFixed = 0m, FeePercentage = 0m },
            new { TypeName = "SWIFT Transfer", TypeCode = "SWIFT", Description = "Uluslararası para transferi", FeeFixed = 50.00m, FeePercentage = 0.001m },
            new { TypeName = "ATM Para Çekme", TypeCode = "ATM_WITHDRAW", Description = "ATM'den nakit çekimi", FeeFixed = 0m, FeePercentage = 0m },
            new { TypeName = "ATM Para Yatırma", TypeCode = "ATM_DEPOSIT", Description = "ATM'den nakit yatırma", FeeFixed = 0m, FeePercentage = 0m },
            new { TypeName = "Fatura Ödeme", TypeCode = "BILL_PAY", Description = "Fatura ve aidat ödemeleri", FeeFixed = 1.00m, FeePercentage = 0m },
            new { TypeName = "Vergi Ödeme", TypeCode = "TAX_PAY", Description = "Vergi ve harç ödemeleri", FeeFixed = 0m, FeePercentage = 0m },
            new { TypeName = "SGK Ödeme", TypeCode = "SGK_PAY", Description = "SGK prim ödemeleri", FeeFixed = 0m, FeePercentage = 0m }
        };

        var sql = @"INSERT INTO transaction_types (type_name, type_code, description, fee_fixed, fee_percentage, is_active)
                    VALUES (@TypeName, @TypeCode, @Description, @FeeFixed, @FeePercentage, 1)";
        foreach (var t in types) await connection.ExecuteAsync(sql, t);
        Console.WriteLine("✅ 9 işlem türü eklendi.");
    }

    private async Task SeedTransactionsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transactions");
        if (count > 0) { Console.WriteLine($"Transactions: {count} kayıt var, atlanıyor..."); return; }

        var accounts = (await connection.QueryAsync<dynamic>("SELECT account_id, user_id, currency FROM accounts WHERE status = 'Active'")).ToList();
        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var types = new[] { "Transfer", "Transfer", "Deposit", "Withdrawal", "Payment" };
        var statuses = new[] { "Completed", "Completed", "Completed", "Completed", "Pending", "Failed", "Reversed" };
        var descriptions = new[] { "Kira ödemesi", "Maaş transferi", "Fatura ödemesi", "Market alışverişi", "Online alışveriş", "Borç ödemesi", "Hediye transferi", "Yatırım" };

        var sql = @"INSERT INTO transactions (from_account_id, to_account_id, amount, currency, transaction_type,
                    description, reference_number, status, fraud_score, fraud_flags, requires_review, reviewed_by,
                    reviewed_at, transaction_date, completed_at, created_by, ip_address, user_agent,
                    is_suspicious, suspicious_reason, reported_to_masak, masak_report_date)
                    VALUES (@FromId, @ToId, @Amount, @Currency, @Type, @Desc, @Ref, @Status, @FraudScore, @FraudFlags,
                    @RequiresReview, @ReviewedBy, @ReviewedAt, @TxDate, @CompletedAt, @CreatedBy, @Ip, @UserAgent,
                    @IsSuspicious, @SuspiciousReason, @ReportedToMasak, @MasakReportDate)";

        for (int i = 0; i < 1000; i++)
        {
            var fromAcc = accounts[_random.Next(accounts.Count)];
            var toAcc = accounts[_random.Next(accounts.Count)];
            var type = types[_random.Next(types.Length)];
            var status = statuses[_random.Next(statuses.Length)];
            var amount = _random.Next(10, 50000);
            var fraudScore = _random.Next(0, 100);
            var isSuspicious = fraudScore > 70;
            var txDate = DateTime.Now.AddDays(-_random.Next(0, 365)).AddHours(-_random.Next(0, 24));

            await connection.ExecuteAsync(sql, new
            {
                FromId = type == "Deposit" ? (int?)null : (int)fromAcc.account_id,
                ToId = type == "Withdrawal" ? (int?)null : (int)toAcc.account_id,
                Amount = amount,
                Currency = (string)fromAcc.currency,
                Type = type,
                Desc = descriptions[_random.Next(descriptions.Length)],
                Ref = $"TXN{DateTime.Now:yyyyMMdd}{_random.Next(100000, 999999)}",
                Status = status,
                FraudScore = fraudScore,
                FraudFlags = fraudScore > 50 ? "{\"high_amount\":true,\"new_beneficiary\":true}" : null,
                RequiresReview = fraudScore > 60,
                ReviewedBy = fraudScore > 60 && status == "Completed" ? userIds[_random.Next(Math.Min(5, userIds.Count))] : (int?)null,
                ReviewedAt = fraudScore > 60 && status == "Completed" ? txDate.AddHours(_random.Next(1, 24)) : (DateTime?)null,
                TxDate = txDate,
                CompletedAt = status == "Completed" ? txDate.AddSeconds(_random.Next(1, 300)) : (DateTime?)null,
                CreatedBy = (int)fromAcc.user_id,
                Ip = $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}",
                UserAgent = "Mozilla/5.0 Chrome/120.0",
                IsSuspicious = isSuspicious,
                SuspiciousReason = isSuspicious ? "Yüksek tutarlı işlem" : null,
                ReportedToMasak = isSuspicious && amount > 75000,
                MasakReportDate = isSuspicious && amount > 75000 ? txDate.AddDays(1) : (DateTime?)null
            });
        }
        Console.WriteLine("✅ 1000 işlem eklendi.");
    }

    private async Task SeedTransactionFeesAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transaction_fees");
        if (count > 0) { Console.WriteLine($"Transaction Fees: {count} kayıt var, atlanıyor..."); return; }

        var txIds = (await connection.QueryAsync<int>("SELECT transaction_id FROM transactions WHERE transaction_type IN ('Transfer', 'Payment')")).ToList();
        var feeTypes = new[] { "Commission", "BSMV", "ServiceFee" };

        var sql = @"INSERT INTO transaction_fees (transaction_id, fee_type, fee_amount, applied_at)
                    VALUES (@TxId, @FeeType, @FeeAmount, @AppliedAt)";

        foreach (var txId in txIds.Take(500))
        {
            if (_random.Next(100) < 70)
            {
                await connection.ExecuteAsync(sql, new
                {
                    TxId = txId,
                    FeeType = feeTypes[_random.Next(feeTypes.Length)],
                    FeeAmount = _random.Next(1, 50) + _random.Next(0, 100) / 100m,
                    AppliedAt = DateTime.Now.AddDays(-_random.Next(0, 365))
                });
            }
        }
        Console.WriteLine("✅ İşlem ücretleri eklendi.");
    }

    private async Task SeedScheduledTransactionsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM scheduled_transactions");
        if (count > 0) { Console.WriteLine($"Scheduled Transactions: {count} kayıt var, atlanıyor..."); return; }

        var accounts = (await connection.QueryAsync<dynamic>("SELECT account_id, user_id FROM accounts WHERE status = 'Active'")).ToList();
        var frequencies = new[] { "Monthly", "Weekly", "Yearly" };
        var statuses = new[] { "Active", "Active", "Active", "Paused", "Cancelled" };

        var sql = @"INSERT INTO scheduled_transactions (user_id, from_account_id, to_account_id, amount, frequency,
                    start_date, end_date, next_execution_date, last_execution_date, status, created_at)
                    VALUES (@UserId, @FromId, @ToId, @Amount, @Frequency, @StartDate, @EndDate, @NextDate, @LastDate, @Status, @CreatedAt)";

        for (int i = 0; i < 100; i++)
        {
            var fromAcc = accounts[_random.Next(accounts.Count)];
            var toAcc = accounts[_random.Next(accounts.Count)];
            var startDate = DateTime.Now.AddMonths(-_random.Next(1, 12));

            await connection.ExecuteAsync(sql, new
            {
                UserId = (int)fromAcc.user_id,
                FromId = (int)fromAcc.account_id,
                ToId = (int)toAcc.account_id,
                Amount = _random.Next(100, 5000),
                Frequency = frequencies[_random.Next(frequencies.Length)],
                StartDate = startDate,
                EndDate = _random.Next(100) < 30 ? startDate.AddYears(1) : (DateTime?)null,
                NextDate = DateTime.Now.AddDays(_random.Next(1, 30)),
                LastDate = DateTime.Now.AddDays(-_random.Next(1, 30)),
                Status = statuses[_random.Next(statuses.Length)],
                CreatedAt = startDate
            });
        }
        Console.WriteLine("✅ 100 planlanmış işlem eklendi.");
    }

    private async Task SeedTransactionApprovalsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM transaction_approvals");
        if (count > 0) { Console.WriteLine($"Transaction Approvals: {count} kayıt var, atlanıyor..."); return; }

        var txIds = (await connection.QueryAsync<int>("SELECT transaction_id FROM transactions WHERE requires_review = 1")).ToList();
        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).Take(10).ToList();
        var statuses = new[] { "Approved", "Approved", "Rejected", "Pending" };

        var sql = @"INSERT INTO transaction_approvals (transaction_id, approver_id, approval_level, status, comments, approved_at)
                    VALUES (@TxId, @ApproverId, @Level, @Status, @Comments, @ApprovedAt)";

        foreach (var txId in txIds)
        {
            var status = statuses[_random.Next(statuses.Length)];
            await connection.ExecuteAsync(sql, new
            {
                TxId = txId,
                ApproverId = userIds[_random.Next(userIds.Count)],
                Level = _random.Next(1, 3),
                Status = status,
                Comments = status == "Rejected" ? "Risk skoru yüksek, işlem reddedildi." : (status == "Approved" ? "İşlem onaylandı." : null),
                ApprovedAt = status != "Pending" ? DateTime.Now.AddDays(-_random.Next(0, 30)) : (DateTime?)null
            });
        }
        Console.WriteLine("✅ İşlem onayları eklendi.");
    }

    private async Task SeedGeneralLedgerAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM general_ledger");
        if (count > 0) { Console.WriteLine($"General Ledger: {count} kayıt var, atlanıyor..."); return; }

        var transactions = (await connection.QueryAsync<dynamic>(
            "SELECT transaction_id, from_account_id, to_account_id, amount FROM transactions WHERE status = 'Completed' AND from_account_id IS NOT NULL AND to_account_id IS NOT NULL")).Take(300).ToList();

        var sql = @"INSERT INTO general_ledger (transaction_id, account_id, debit_amount, credit_amount, entry_type, entry_date, description)
                    VALUES (@TxId, @AccountId, @Debit, @Credit, @EntryType, @EntryDate, @Desc)";

        foreach (var tx in transactions)
        {
            // Borç kaydı (gönderen)
            await connection.ExecuteAsync(sql, new
            {
                TxId = (int)tx.transaction_id,
                AccountId = (int)tx.from_account_id,
                Debit = (decimal)tx.amount,
                Credit = 0m,
                EntryType = "Debit",
                EntryDate = DateTime.Now.AddDays(-_random.Next(0, 365)),
                Desc = "Para transferi - Borç"
            });

            // Alacak kaydı (alıcı)
            await connection.ExecuteAsync(sql, new
            {
                TxId = (int)tx.transaction_id,
                AccountId = (int)tx.to_account_id,
                Debit = 0m,
                Credit = (decimal)tx.amount,
                EntryType = "Credit",
                EntryDate = DateTime.Now.AddDays(-_random.Next(0, 365)),
                Desc = "Para transferi - Alacak"
            });
        }
        Console.WriteLine("✅ Muhasebe kayıtları eklendi.");
    }
// ==================== MODÜL 4: PAYMENT & CARDS ====================

    private async Task SeedCreditCardsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM credit_cards");
        if (count > 0) { Console.WriteLine($"Credit Cards: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var cardTypes = new[] { "Physical", "Physical", "Physical", "Virtual" };
        var cardBrands = new[] { "Visa", "Mastercard", "Troy" };
        var statuses = new[] { "Active", "Active", "Active", "Blocked", "Expired" };

        var sql = @"INSERT INTO credit_cards (user_id, card_number_encrypted, card_last_four, card_type, card_brand,
                    credit_limit, available_limit, current_balance, minimum_payment, payment_due_date, interest_rate,
                    expiry_month, expiry_year, cvv_encrypted, status, issued_at, activated_at)
                    VALUES (@UserId, @CardNum, @Last4, @CardType, @CardBrand, @CreditLimit, @AvailableLimit,
                    @CurrentBalance, @MinPayment, @DueDate, @InterestRate, @ExpMonth, @ExpYear, @Cvv, @Status, @IssuedAt, @ActivatedAt)";

        foreach (var userId in userIds.Take(60))
        {
            var limit = _random.Next(5000, 150000);
            var used = _random.Next(0, limit);
            var issuedAt = DateTime.Now.AddDays(-_random.Next(30, 1000));

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                CardNum = Convert.ToBase64String(Encoding.UTF8.GetBytes($"4{_random.Next(100, 999)}{_random.Next(1000, 9999)}{_random.Next(1000, 9999)}{_random.Next(1000, 9999)}")),
                Last4 = _random.Next(1000, 9999).ToString(),
                CardType = cardTypes[_random.Next(cardTypes.Length)],
                CardBrand = cardBrands[_random.Next(cardBrands.Length)],
                CreditLimit = limit,
                AvailableLimit = limit - used,
                CurrentBalance = used,
                MinPayment = used * 0.03m,
                DueDate = DateTime.Now.AddDays(_random.Next(1, 28)),
                InterestRate = 2.99m + _random.Next(0, 200) / 100m,
                ExpMonth = _random.Next(1, 13),
                ExpYear = _random.Next(2025, 2030),
                Cvv = Convert.ToBase64String(Encoding.UTF8.GetBytes(_random.Next(100, 999).ToString())),
                Status = statuses[_random.Next(statuses.Length)],
                IssuedAt = issuedAt,
                ActivatedAt = issuedAt.AddDays(_random.Next(1, 14))
            });
        }
        Console.WriteLine("✅ 60 kredi kartı eklendi.");
    }

    private async Task SeedCardTransactionsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM card_transactions");
        if (count > 0) { Console.WriteLine($"Card Transactions: {count} kayıt var, atlanıyor..."); return; }

        var cardIds = (await connection.QueryAsync<int>("SELECT card_id FROM credit_cards WHERE status = 'Active'")).ToList();
        var merchants = new[] { "Migros", "BIM", "A101", "Teknosa", "MediaMarkt", "Trendyol", "Hepsiburada", "Amazon", "Netflix", "Spotify", "Shell", "Opet", "Starbucks", "McDonald's" };
        var categories = new[] { "Market", "Market", "Market", "Elektronik", "Elektronik", "E-Ticaret", "E-Ticaret", "E-Ticaret", "Abonelik", "Abonelik", "Akaryakıt", "Akaryakıt", "Restoran", "Restoran" };
        var statuses = new[] { "Approved", "Approved", "Approved", "Approved", "Declined", "Refunded" };

        var sql = @"INSERT INTO card_transactions (card_id, merchant_name, merchant_category, amount, currency,
                    transaction_date, status, authorization_code, ip_address, location)
                    VALUES (@CardId, @Merchant, @Category, @Amount, @Currency, @TxDate, @Status, @AuthCode, @Ip, @Location)";

        for (int i = 0; i < 500; i++)
        {
            var idx = _random.Next(merchants.Length);
            await connection.ExecuteAsync(sql, new
            {
                CardId = cardIds[_random.Next(cardIds.Count)],
                Merchant = merchants[idx],
                Category = categories[idx],
                Amount = _random.Next(10, 5000) + _random.Next(0, 100) / 100m,
                Currency = "TRY",
                TxDate = DateTime.Now.AddDays(-_random.Next(0, 180)).AddHours(-_random.Next(0, 24)),
                Status = statuses[_random.Next(statuses.Length)],
                AuthCode = $"AUTH{_random.Next(100000, 999999)}",
                Ip = $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}",
                Location = _random.Next(100) < 80 ? "İstanbul, TR" : "Online"
            });
        }
        Console.WriteLine("✅ 500 kart harcaması eklendi.");
    }

    private async Task SeedPaymentMethodsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM payment_methods");
        if (count > 0) { Console.WriteLine($"Payment Methods: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).Take(50).ToList();
        var faker = new Faker("tr");
        var brands = new[] { "Visa", "Mastercard", "Troy" };
        var types = new[] { "CreditCard", "DebitCard" };

        var sql = @"INSERT INTO payment_methods (user_id, method_type, card_token, card_last_four, card_brand,
                    expiry_month, expiry_year, cardholder_name, is_default, status, created_at, last_used_at)
                    VALUES (@UserId, @MethodType, @Token, @Last4, @Brand, @ExpMonth, @ExpYear, @Holder, @IsDefault, @Status, @CreatedAt, @LastUsed)";

        foreach (var userId in userIds)
        {
            var methodCount = _random.Next(1, 3);
            for (int i = 0; i < methodCount; i++)
            {
                await connection.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    MethodType = types[_random.Next(types.Length)],
                    Token = $"tok_{Guid.NewGuid():N}",
                    Last4 = _random.Next(1000, 9999).ToString(),
                    Brand = brands[_random.Next(brands.Length)],
                    ExpMonth = _random.Next(1, 13),
                    ExpYear = _random.Next(2025, 2030),
                    Holder = faker.Name.FullName().ToUpper(),
                    IsDefault = i == 0,
                    Status = "Active",
                    CreatedAt = DateTime.Now.AddDays(-_random.Next(30, 365)),
                    LastUsed = DateTime.Now.AddDays(-_random.Next(0, 30))
                });
            }
        }
        Console.WriteLine("✅ Ödeme yöntemleri eklendi.");
    }

    private async Task SeedRecurringPaymentsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM recurring_payments");
        if (count > 0) { Console.WriteLine($"Recurring Payments: {count} kayıt var, atlanıyor..."); return; }

        var paymentMethods = (await connection.QueryAsync<dynamic>("SELECT payment_method_id, user_id FROM payment_methods")).ToList();
        var merchants = new[] { "Netflix", "Spotify", "YouTube Premium", "Apple iCloud", "Google One", "Amazon Prime", "Turkcell", "Vodafone", "ISKI", "IGDAS" };
        var frequencies = new[] { "Monthly", "Monthly", "Monthly", "Yearly" };

        var sql = @"INSERT INTO recurring_payments (user_id, payment_method_id, merchant_name, amount, frequency, next_payment_date, last_payment_date, status)
                    VALUES (@UserId, @PaymentMethodId, @Merchant, @Amount, @Frequency, @NextDate, @LastDate, @Status)";

        for (int i = 0; i < 80; i++)
        {
            var pm = paymentMethods[_random.Next(paymentMethods.Count)];
            await connection.ExecuteAsync(sql, new
            {
                UserId = (int)pm.user_id,
                PaymentMethodId = (int)pm.payment_method_id,
                Merchant = merchants[_random.Next(merchants.Length)],
                Amount = _random.Next(30, 500),
                Frequency = frequencies[_random.Next(frequencies.Length)],
                NextDate = DateTime.Now.AddDays(_random.Next(1, 30)),
                LastDate = DateTime.Now.AddDays(-_random.Next(1, 30)),
                Status = _random.Next(100) < 85 ? "Active" : "Cancelled"
            });
        }
        Console.WriteLine("✅ 80 düzenli ödeme eklendi.");
    }

    private async Task SeedPaymentGatewaysAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM payment_gateways");
        if (count > 0) { Console.WriteLine($"Payment Gateways: {count} kayıt var, atlanıyor..."); return; }

        var gateways = new[]
        {
            new { Name = "iyzico", Type = "VirtualPos", Endpoint = "https://api.iyzipay.com", IsActive = true },
            new { Name = "PayTR", Type = "VirtualPos", Endpoint = "https://www.paytr.com/odeme", IsActive = true },
            new { Name = "Param", Type = "ThreeD", Endpoint = "https://pos.param.com.tr", IsActive = true },
            new { Name = "Stripe", Type = "VirtualPos", Endpoint = "https://api.stripe.com/v1", IsActive = false },
            new { Name = "PayPal", Type = "DirectDebit", Endpoint = "https://api.paypal.com", IsActive = false }
        };

        var sql = @"INSERT INTO payment_gateways (gateway_name, gateway_type, api_endpoint, api_key_encrypted, is_active, created_at)
                    VALUES (@Name, @Type, @Endpoint, @ApiKey, @IsActive, @CreatedAt)";

        foreach (var gw in gateways)
        {
            await connection.ExecuteAsync(sql, new
            {
                gw.Name,
                gw.Type,
                gw.Endpoint,
                ApiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"sk_live_{Guid.NewGuid():N}")),
                gw.IsActive,
                CreatedAt = DateTime.Now.AddDays(-_random.Next(100, 500))
            });
        }
        Console.WriteLine("✅ 5 ödeme sağlayıcısı eklendi.");
    }

    // ==================== MODÜL 5: COMPLIANCE & KYC ====================

    private async Task SeedKycDocumentsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kyc_documents");
        if (count > 0) { Console.WriteLine($"KYC Documents: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var docTypes = new[] { "IdentityCard", "IdentityCard", "Passport", "DriverLicense", "UtilityBill" };
        var statuses = new[] { "Verified", "Verified", "Verified", "Pending", "Rejected" };

        var sql = @"INSERT INTO kyc_documents (user_id, document_type, document_number, document_file_path, document_hash,
                    expiry_date, upload_date, verified_at, verified_by, verification_status, rejection_reason)
                    VALUES (@UserId, @DocType, @DocNum, @FilePath, @Hash, @Expiry, @UploadDate, @VerifiedAt, @VerifiedBy, @Status, @Reason)";

        foreach (var userId in userIds)
        {
            var docType = docTypes[_random.Next(docTypes.Length)];
            var status = statuses[_random.Next(statuses.Length)];
            var uploadDate = DateTime.Now.AddDays(-_random.Next(30, 365));

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                DocType = docType,
                DocNum = docType == "IdentityCard" ? $"{_random.Next(10000, 99999)}{_random.Next(100000, 999999)}" : $"U{_random.Next(10000000, 99999999)}",
                FilePath = $"/uploads/kyc/{userId}/{docType.ToLower()}_{Guid.NewGuid():N}.pdf",
                Hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))),
                Expiry = DateTime.Now.AddYears(_random.Next(1, 10)),
                UploadDate = uploadDate,
                VerifiedAt = status != "Pending" ? uploadDate.AddDays(_random.Next(1, 7)) : (DateTime?)null,
                VerifiedBy = status != "Pending" ? userIds[_random.Next(Math.Min(5, userIds.Count))] : (int?)null,
                Status = status,
                Reason = status == "Rejected" ? "Belge okunaklı değil" : null
            });
        }
        Console.WriteLine("✅ KYC belgeleri eklendi.");
    }

    private async Task SeedKycVerificationsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kyc_verifications");
        if (count > 0) { Console.WriteLine($"KYC Verifications: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var verTypes = new[] { "Email", "Phone", "Identity", "Address" };
        var methods = new[] { "Sms", "EmailLink", "DocumentUpload", "DatabaseCheck" };
        var statuses = new[] { "Verified", "Verified", "Verified", "Pending", "Failed" };

        var sql = @"INSERT INTO kyc_verifications (user_id, verification_type, verification_method, verification_status,
                    verification_code, verified_at, attempts, expires_at)
                    VALUES (@UserId, @VerType, @Method, @Status, @Code, @VerifiedAt, @Attempts, @ExpiresAt)";

        foreach (var userId in userIds)
        {
            foreach (var verType in verTypes)
            {
                if (_random.Next(100) < 80)
                {
                    var status = statuses[_random.Next(statuses.Length)];
                    await connection.ExecuteAsync(sql, new
                    {
                        UserId = userId,
                        VerType = verType,
                        Method = methods[_random.Next(methods.Length)],
                        Status = status,
                        Code = _random.Next(100000, 999999).ToString(),
                        VerifiedAt = status == "Verified" ? DateTime.Now.AddDays(-_random.Next(1, 365)) : (DateTime?)null,
                        Attempts = _random.Next(1, 4),
                        ExpiresAt = DateTime.Now.AddMinutes(_random.Next(5, 30))
                    });
                }
            }
        }
        Console.WriteLine("✅ KYC doğrulamaları eklendi.");
    }

    private async Task SeedKvkkConsentsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kvkk_consents");
        if (count > 0) { Console.WriteLine($"KVKK Consents: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var consentTypes = new[] { "DataProcessing", "Marketing", "ThirdPartyTransfer" };
        var consentTexts = new Dictionary<string, string>
        {
            { "DataProcessing", "Kişisel verilerimin işlenmesini kabul ediyorum." },
            { "Marketing", "Pazarlama amaçlı iletişim almayı kabul ediyorum." },
            { "ThirdPartyTransfer", "Verilerimin 3. taraflarla paylaşılmasını kabul ediyorum." }
        };

        var sql = @"INSERT INTO kvkk_consents (user_id, consent_type, consent_given, consent_text, consent_version, granted_at, revoked_at, ip_address)
                    VALUES (@UserId, @ConsentType, @Given, @Text, @Version, @GrantedAt, @RevokedAt, @Ip)";

        foreach (var userId in userIds)
        {
            foreach (var consentType in consentTypes)
            {
                var given = consentType == "DataProcessing" ? true : _random.Next(100) < 60;
                var grantedAt = DateTime.Now.AddDays(-_random.Next(30, 730));

                await connection.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    ConsentType = consentType,
                    Given = given,
                    Text = consentTexts[consentType],
                    Version = "v2.1",
                    GrantedAt = grantedAt,
                    RevokedAt = (!given && _random.Next(100) < 20) ? grantedAt.AddDays(_random.Next(30, 180)) : (DateTime?)null,
                    Ip = $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}"
                });
            }
        }
        Console.WriteLine("✅ KVKK onayları eklendi.");
    }

    private async Task SeedKvkkDataRequestsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM kvkk_data_requests");
        if (count > 0) { Console.WriteLine($"KVKK Data Requests: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var requestTypes = new[] { "Access", "Rectification", "Erasure", "Portability" };
        var statuses = new[] { "Completed", "Completed", "InProgress", "Pending", "Rejected" };

        var sql = @"INSERT INTO kvkk_data_requests (user_id, request_type, request_date, status, completed_at, completed_by, response_data, response_file_path)
                    VALUES (@UserId, @RequestType, @RequestDate, @Status, @CompletedAt, @CompletedBy, @ResponseData, @FilePath)";

        for (int i = 0; i < 30; i++)
        {
            var status = statuses[_random.Next(statuses.Length)];
            var requestDate = DateTime.Now.AddDays(-_random.Next(1, 180));

            await connection.ExecuteAsync(sql, new
            {
                UserId = userIds[_random.Next(userIds.Count)],
                RequestType = requestTypes[_random.Next(requestTypes.Length)],
                RequestDate = requestDate,
                Status = status,
                CompletedAt = status == "Completed" ? requestDate.AddDays(_random.Next(1, 30)) : (DateTime?)null,
                CompletedBy = status == "Completed" ? userIds[_random.Next(Math.Min(5, userIds.Count))] : (int?)null,
                ResponseData = status == "Completed" ? "{\"exported_tables\": [\"users\", \"accounts\", \"transactions\"]}" : null,
                FilePath = status == "Completed" ? $"/exports/kvkk/{Guid.NewGuid():N}.zip" : null
            });
        }
        Console.WriteLine("✅ 30 KVKK talebi eklendi.");
    }

    private async Task SeedMasakRecordsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM masak_records");
        if (count > 0) { Console.WriteLine($"MASAK Records: {count} kayıt var, atlanıyor..."); return; }

        var users = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var txIds = (await connection.QueryAsync<int>("SELECT transaction_id FROM transactions WHERE amount > 50000")).ToList();
        var recordTypes = new[] { "KycLog", "TransactionReport", "SuspiciousReport", "CustomerDueDiligence" };

        var sql = @"INSERT INTO masak_records (customer_id, transaction_id, record_type, data, created_at, retention_until)
                    VALUES (@CustomerId, @TxId, @RecordType, @Data, @CreatedAt, @RetentionUntil)";

        for (int i = 0; i < 50; i++)
        {
            var createdAt = DateTime.Now.AddDays(-_random.Next(1, 365));
            await connection.ExecuteAsync(sql, new
            {
                CustomerId = users[_random.Next(users.Count)],
                TxId = txIds.Count > 0 ? txIds[_random.Next(txIds.Count)] : (int?)null,
                RecordType = recordTypes[_random.Next(recordTypes.Length)],
                Data = $"{{\"report_id\": \"{Guid.NewGuid():N}\", \"risk_score\": {_random.Next(50, 100)}, \"amount\": {_random.Next(50000, 500000)}}}",
                CreatedAt = createdAt,
                RetentionUntil = createdAt.AddYears(10)
            });
        }
        Console.WriteLine("✅ 50 MASAK kaydı eklendi.");
    }

    private async Task SeedSuspiciousActivityReportsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM suspicious_activity_reports");
        if (count > 0) { Console.WriteLine($"Suspicious Activity Reports: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var txIds = (await connection.QueryAsync<int>("SELECT transaction_id FROM transactions WHERE fraud_score > 50")).ToList();
        var reportTypes = new[] { "Structuring", "LargeCash", "UnusualActivity", "TerrorismFinancing" };
        var statuses = new[] { "Submitted", "Submitted", "Draft", "Closed" };

        var sql = @"INSERT INTO suspicious_activity_reports (user_id, transaction_id, report_type, description, risk_score,
                    created_by, created_at, reported_to_masak, masak_report_date, masak_reference_number, status)
                    VALUES (@UserId, @TxId, @ReportType, @Description, @RiskScore, @CreatedBy, @CreatedAt,
                    @ReportedToMasak, @MasakReportDate, @MasakRef, @Status)";

        for (int i = 0; i < 25; i++)
        {
            var status = statuses[_random.Next(statuses.Length)];
            var createdAt = DateTime.Now.AddDays(-_random.Next(1, 180));
            var reportedToMasak = status == "Submitted";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userIds[_random.Next(userIds.Count)],
                TxId = txIds.Count > 0 ? txIds[_random.Next(txIds.Count)] : (int?)null,
                ReportType = reportTypes[_random.Next(reportTypes.Length)],
                Description = "Olağandışı işlem aktivitesi tespit edildi. Kısa sürede çok sayıda yüksek tutarlı transfer gerçekleştirildi.",
                RiskScore = _random.Next(60, 100),
                CreatedBy = userIds[_random.Next(Math.Min(5, userIds.Count))],
                CreatedAt = createdAt,
                ReportedToMasak = reportedToMasak,
                MasakReportDate = reportedToMasak ? createdAt.AddDays(_random.Next(1, 7)) : (DateTime?)null,
                MasakRef = reportedToMasak ? $"MASAK-{DateTime.Now.Year}-{_random.Next(10000, 99999)}" : null,
                Status = status
            });
        }
        Console.WriteLine("✅ 25 şüpheli işlem raporu eklendi.");
    }
// ==================== MODÜL 6: AUDIT & SECURITY ====================

    private async Task SeedAuditLogsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM audit_logs");
        if (count > 0) { Console.WriteLine($"Audit Logs: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var tables = new[] { "users", "accounts", "transactions", "credit_cards", "kyc_documents" };
        var actions = new[] { "INSERT", "UPDATE", "DELETE" };

        var sql = @"INSERT INTO audit_logs (table_name, record_id, action, old_value, new_value, changed_fields, changed_by, ip_address, timestamp)
                    VALUES (@TableName, @RecordId, @Action, @OldValue, @NewValue, @ChangedFields, @ChangedBy, @Ip, @Timestamp)";

        for (int i = 0; i < 1000; i++)
        {
            var table = tables[_random.Next(tables.Length)];
            var action = actions[_random.Next(actions.Length)];

            await connection.ExecuteAsync(sql, new
            {
                TableName = table,
                RecordId = _random.Next(1, 100).ToString(),
                Action = action,
                OldValue = action != "INSERT" ? $"{{\"status\": \"Active\", \"balance\": {_random.Next(1000, 50000)}}}" : null,
                NewValue = action != "DELETE" ? $"{{\"status\": \"Active\", \"balance\": {_random.Next(1000, 50000)}}}" : null,
                ChangedFields = action == "UPDATE" ? "[\"status\", \"balance\", \"updated_at\"]" : null,
                ChangedBy = userIds[_random.Next(userIds.Count)],
                Ip = $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}",
                Timestamp = DateTime.Now.AddDays(-_random.Next(0, 365)).AddHours(-_random.Next(0, 24)).AddMinutes(-_random.Next(0, 60))
            });
        }
        Console.WriteLine("✅ 1000 audit log eklendi.");
    }

    private async Task SeedSecurityEventsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM security_events");
        if (count > 0) { Console.WriteLine($"Security Events: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var eventTypes = new[] { "FailedLogin", "PasswordChange", "SuspiciousActivity", "AccountLocked", "UnusualLocation", "MultipleDevices", "BruteForceAttempt" };
        var severities = new[] { "Low", "Medium", "High", "Critical" };
        var descriptions = new Dictionary<string, string>
        {
            { "FailedLogin", "Başarısız giriş denemesi tespit edildi." },
            { "PasswordChange", "Kullanıcı şifresi değiştirildi." },
            { "SuspiciousActivity", "Şüpheli aktivite tespit edildi." },
            { "AccountLocked", "Hesap çok sayıda başarısız giriş nedeniyle kilitlendi." },
            { "UnusualLocation", "Alışılmadık bir konumdan giriş yapıldı." },
            { "MultipleDevices", "Aynı anda birden fazla cihazdan giriş yapıldı." },
            { "BruteForceAttempt", "Brute force saldırı girişimi tespit edildi." }
        };

        var sql = @"INSERT INTO security_events (event_type, user_id, severity, description, ip_address, user_agent, event_date, resolved, resolved_at, resolved_by)
                    VALUES (@EventType, @UserId, @Severity, @Description, @Ip, @UserAgent, @EventDate, @Resolved, @ResolvedAt, @ResolvedBy)";

        for (int i = 0; i < 300; i++)
        {
            var eventType = eventTypes[_random.Next(eventTypes.Length)];
            var eventDate = DateTime.Now.AddDays(-_random.Next(0, 180)).AddHours(-_random.Next(0, 24));
            var resolved = _random.Next(100) < 70;

            await connection.ExecuteAsync(sql, new
            {
                EventType = eventType,
                UserId = userIds[_random.Next(userIds.Count)],
                Severity = severities[_random.Next(severities.Length)],
                Description = descriptions[eventType],
                Ip = $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0",
                EventDate = eventDate,
                Resolved = resolved,
                ResolvedAt = resolved ? eventDate.AddHours(_random.Next(1, 72)) : (DateTime?)null,
                ResolvedBy = resolved ? userIds[_random.Next(Math.Min(5, userIds.Count))] : (int?)null
            });
        }
        Console.WriteLine("✅ 300 güvenlik olayı eklendi.");
    }

    private async Task SeedDataAccessLogAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM data_access_log");
        if (count > 0) { Console.WriteLine($"Data Access Log: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var dataTypes = new[] { "PersonalInfo", "FinancialData", "TransactionHistory", "KycDocuments", "AuditLogs" };
        var reasons = new[] { "CustomerSupport", "FraudInvestigation", "AuditRequest", "LegalCompliance", "DataExport" };

        var sql = @"INSERT INTO data_access_log (accessed_by_user_id, target_user_id, data_type, access_reason, access_timestamp, ip_address, is_sensitive)
                    VALUES (@AccessedBy, @TargetUser, @DataType, @Reason, @Timestamp, @Ip, @IsSensitive)";

        for (int i = 0; i < 500; i++)
        {
            var dataType = dataTypes[_random.Next(dataTypes.Length)];
            await connection.ExecuteAsync(sql, new
            {
                AccessedBy = userIds[_random.Next(Math.Min(10, userIds.Count))],
                TargetUser = userIds[_random.Next(userIds.Count)],
                DataType = dataType,
                Reason = reasons[_random.Next(reasons.Length)],
                Timestamp = DateTime.Now.AddDays(-_random.Next(0, 180)).AddHours(-_random.Next(0, 24)),
                Ip = $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}",
                IsSensitive = dataType == "FinancialData" || dataType == "KycDocuments"
            });
        }
        Console.WriteLine("✅ 500 veri erişim logu eklendi.");
    }

    private async Task SeedPciAuditLogAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM pci_audit_log");
        if (count > 0) { Console.WriteLine($"PCI Audit Log: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var actions = new[] { "CardView", "CardCreate", "CardUpdate", "CardDelete", "CvvAccess", "PanAccess" };
        var reasons = new[] { "CustomerRequest", "FraudCheck", "Verification", "SystemMaintenance" };

        var sql = @"INSERT INTO pci_audit_log (user_id, action, card_token, timestamp, ip_address, success, reason)
                    VALUES (@UserId, @Action, @Token, @Timestamp, @Ip, @Success, @Reason)";

        for (int i = 0; i < 200; i++)
        {
            var success = _random.Next(100) < 95;
            await connection.ExecuteAsync(sql, new
            {
                UserId = userIds[_random.Next(userIds.Count)],
                Action = actions[_random.Next(actions.Length)],
                Token = $"tok_{Guid.NewGuid():N}",
                Timestamp = DateTime.Now.AddDays(-_random.Next(0, 180)).AddHours(-_random.Next(0, 24)),
                Ip = $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}",
                Success = success,
                Reason = success ? reasons[_random.Next(reasons.Length)] : "Unauthorized access attempt"
            });
        }
        Console.WriteLine("✅ 200 PCI audit log eklendi.");
    }

    private async Task SeedEncryptionKeysAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM encryption_keys");
        if (count > 0) { Console.WriteLine($"Encryption Keys: {count} kayıt var, atlanıyor..."); return; }

        var keys = new[]
        {
            new { KeyName = "MasterKey_2024", KeyType = "AES256", Status = "Active" },
            new { KeyName = "MasterKey_2023", KeyType = "AES256", Status = "Retired" },
            new { KeyName = "CardEncryptionKey", KeyType = "RSA4096", Status = "Active" },
            new { KeyName = "TokenSigningKey", KeyType = "RSA2048", Status = "Active" },
            new { KeyName = "BackupKey", KeyType = "AES256", Status = "Active" },
            new { KeyName = "DataAtRestKey", KeyType = "AES256", Status = "Active" }
        };

        var sql = @"INSERT INTO encryption_keys (key_name, key_type, key_value_encrypted, created_at, expires_at, rotation_date, status)
                    VALUES (@KeyName, @KeyType, @KeyValue, @CreatedAt, @ExpiresAt, @RotationDate, @Status)";

        foreach (var key in keys)
        {
            var createdAt = DateTime.Now.AddDays(-_random.Next(30, 730));
            await connection.ExecuteAsync(sql, new
            {
                key.KeyName,
                key.KeyType,
                KeyValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                CreatedAt = createdAt,
                ExpiresAt = createdAt.AddYears(2),
                RotationDate = key.Status == "Retired" ? createdAt.AddYears(1) : (DateTime?)null,
                key.Status
            });
        }
        Console.WriteLine("✅ 6 şifreleme anahtarı eklendi.");
    }

    // ==================== MODÜL 7: FRAUD & RISK ====================

    private async Task SeedFraudRulesAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM fraud_rules");
        if (count > 0) { Console.WriteLine($"Fraud Rules: {count} kayıt var, atlanıyor..."); return; }

        var rules = new[]
        {
            new { RuleName = "Yüksek Tutarlı İşlem", RuleType = "Amount", Description = "75.000 TL üzeri işlemler için alarm", Conditions = "{\"operator\": \">\", \"amount\": 75000}", Weight = 30 },
            new { RuleName = "Hızlı Ardışık İşlem", RuleType = "Velocity", Description = "1 saat içinde 5'ten fazla işlem", Conditions = "{\"period\": \"1h\", \"count\": 5}", Weight = 25 },
            new { RuleName = "Yeni Alıcıya Yüksek Transfer", RuleType = "Beneficiary", Description = "İlk kez işlem yapılan alıcıya 10.000 TL üzeri", Conditions = "{\"new_beneficiary\": true, \"amount\": 10000}", Weight = 20 },
            new { RuleName = "Gece İşlemi", RuleType = "Time", Description = "00:00-06:00 arası yapılan işlemler", Conditions = "{\"start_hour\": 0, \"end_hour\": 6}", Weight = 15 },
            new { RuleName = "Farklı Şehir İşlemi", RuleType = "Location", Description = "Farklı şehirlerden kısa sürede işlem", Conditions = "{\"distance_km\": 500, \"period\": \"2h\"}", Weight = 35 },
            new { RuleName = "Yurtdışı Transfer", RuleType = "International", Description = "Yurtdışına para transferi", Conditions = "{\"international\": true}", Weight = 20 },
            new { RuleName = "Hesap Boşaltma", RuleType = "Pattern", Description = "Hesap bakiyesinin %90'ından fazlası tek seferde", Conditions = "{\"percentage\": 90}", Weight = 40 },
            new { RuleName = "Yapılandırma (Structuring)", RuleType = "Structuring", Description = "MASAK limiti altında parçalı işlemler", Conditions = "{\"amount_range\": [70000, 75000], \"count\": 3, \"period\": \"24h\"}", Weight = 50 }
        };

        var sql = @"INSERT INTO fraud_rules (rule_name, rule_type, rule_description, rule_conditions, risk_score_weight, is_active, created_at)
                    VALUES (@RuleName, @RuleType, @Description, @Conditions, @Weight, 1, @CreatedAt)";

        foreach (var rule in rules)
        {
            await connection.ExecuteAsync(sql, new
            {
                rule.RuleName,
                rule.RuleType,
                rule.Description,
                rule.Conditions,
                rule.Weight,
                CreatedAt = DateTime.Now.AddDays(-_random.Next(30, 365))
            });
        }
        Console.WriteLine("✅ 8 fraud kuralı eklendi.");
    }

    private async Task SeedFraudAlertsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM fraud_alerts");
        if (count > 0) { Console.WriteLine($"Fraud Alerts: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var txIds = (await connection.QueryAsync<int>("SELECT transaction_id FROM transactions WHERE fraud_score > 40")).ToList();
        var severities = new[] { "Low", "Medium", "High", "Critical" };
        var statuses = new[] { "Open", "UnderReview", "Closed", "FalsePositive" };

        var sql = @"INSERT INTO fraud_alerts (user_id, transaction_id, fraud_score, triggered_rules, alert_severity,
                    status, created_at, reviewed_at, reviewed_by, resolution_notes)
                    VALUES (@UserId, @TxId, @FraudScore, @Rules, @Severity, @Status, @CreatedAt, @ReviewedAt, @ReviewedBy, @Notes)";

        for (int i = 0; i < 50; i++)
        {
            var status = statuses[_random.Next(statuses.Length)];
            var createdAt = DateTime.Now.AddDays(-_random.Next(0, 90));
            var fraudScore = _random.Next(50, 100);

            await connection.ExecuteAsync(sql, new
            {
                UserId = userIds[_random.Next(userIds.Count)],
                TxId = txIds.Count > 0 ? txIds[_random.Next(txIds.Count)] : 1,
                FraudScore = fraudScore,
                Rules = "[\"Yüksek Tutarlı İşlem\", \"Hızlı Ardışık İşlem\"]",
                Severity = fraudScore > 80 ? "Critical" : (fraudScore > 60 ? "High" : severities[_random.Next(severities.Length)]),
                Status = status,
                CreatedAt = createdAt,
                ReviewedAt = status != "Open" ? createdAt.AddHours(_random.Next(1, 48)) : (DateTime?)null,
                ReviewedBy = status != "Open" ? userIds[_random.Next(Math.Min(5, userIds.Count))] : (int?)null,
                Notes = status == "FalsePositive" ? "Yanlış alarm, müşteri doğrulandı." : (status == "Closed" ? "İnceleme tamamlandı." : null)
            });
        }
        Console.WriteLine("✅ 50 fraud alarmı eklendi.");
    }

    private async Task SeedRiskProfilesAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM risk_profiles");
        if (count > 0) { Console.WriteLine($"Risk Profiles: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var riskLevels = new[] { "Low", "Low", "Low", "Medium", "Medium", "High" };

        var sql = @"INSERT INTO risk_profiles (user_id, risk_level, transaction_velocity_score, amount_anomaly_score,
                    geographic_risk_score, behavioral_score, last_calculated_at, factors)
                    VALUES (@UserId, @RiskLevel, @VelocityScore, @AmountScore, @GeoScore, @BehaviorScore, @CalculatedAt, @Factors)";

        foreach (var userId in userIds)
        {
            var riskLevel = riskLevels[_random.Next(riskLevels.Length)];
            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                RiskLevel = riskLevel,
                VelocityScore = _random.Next(0, 100),
                AmountScore = _random.Next(0, 100),
                GeoScore = _random.Next(0, 100),
                BehaviorScore = _random.Next(0, 100),
                CalculatedAt = DateTime.Now.AddHours(-_random.Next(1, 168)),
                Factors = $"{{\"account_age_days\": {_random.Next(30, 1000)}, \"avg_transaction\": {_random.Next(500, 10000)}, \"login_frequency\": {_random.Next(1, 30)}}}"
            });
        }
        Console.WriteLine("✅ Risk profilleri eklendi.");
    }
// ==================== MODÜL 8: SYSTEM & CONFIGURATION ====================

    private async Task SeedSystemSettingsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM system_settings");
        if (count > 0) { Console.WriteLine($"System Settings: {count} kayıt var, atlanıyor..."); return; }

        var settings = new[]
        {
            new { Key = "MaxDailyTransferLimit", Value = "500000", Type = "Decimal", Description = "Günlük maksimum transfer limiti (TL)" },
            new { Key = "MaxSingleTransferLimit", Value = "100000", Type = "Decimal", Description = "Tek seferde maksimum transfer limiti (TL)" },
            new { Key = "SessionTimeoutMinutes", Value = "30", Type = "Integer", Description = "Oturum zaman aşımı süresi (dakika)" },
            new { Key = "MaxLoginAttempts", Value = "5", Type = "Integer", Description = "Maksimum başarısız giriş denemesi" },
            new { Key = "PasswordExpiryDays", Value = "90", Type = "Integer", Description = "Şifre geçerlilik süresi (gün)" },
            new { Key = "MasakReportThreshold", Value = "75000", Type = "Decimal", Description = "MASAK bildirim eşiği (TL)" },
            new { Key = "FraudScoreThreshold", Value = "70", Type = "Integer", Description = "Fraud alarm eşiği (0-100)" },
            new { Key = "TwoFactorEnabled", Value = "true", Type = "Boolean", Description = "İki faktörlü doğrulama aktif mi" },
            new { Key = "MaintenanceMode", Value = "false", Type = "Boolean", Description = "Bakım modu aktif mi" },
            new { Key = "DefaultCurrency", Value = "TRY", Type = "String", Description = "Varsayılan para birimi" },
            new { Key = "InterestCalculationDay", Value = "1", Type = "Integer", Description = "Faiz hesaplama günü" },
            new { Key = "MinPasswordLength", Value = "8", Type = "Integer", Description = "Minimum şifre uzunluğu" }
        };

        var sql = @"INSERT INTO system_settings (setting_key, setting_value, setting_type, description, updated_at, updated_by)
                    VALUES (@Key, @Value, @Type, @Description, @UpdatedAt, @UpdatedBy)";

        var firstUserId = await connection.ExecuteScalarAsync<int>("SELECT TOP 1 user_id FROM users");

        foreach (var s in settings)
        {
            await connection.ExecuteAsync(sql, new
            {
                s.Key,
                s.Value,
                s.Type,
                s.Description,
                UpdatedAt = DateTime.Now.AddDays(-_random.Next(1, 90)),
                UpdatedBy = firstUserId
            });
        }
        Console.WriteLine("✅ 12 sistem ayarı eklendi.");
    }

    private async Task SeedNotificationTemplatesAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM notification_templates");
        if (count > 0) { Console.WriteLine($"Notification Templates: {count} kayıt var, atlanıyor..."); return; }

        var templates = new[]
        {
            new { Name = "WelcomeEmail", Type = "Email", Subject = "Hoş Geldiniz!", Body = "Sayın {{FirstName}} {{LastName}}, bankamıza hoş geldiniz. Hesabınız başarıyla oluşturuldu.", Variables = "[\"FirstName\", \"LastName\"]" },
            new { Name = "TransferConfirmation", Type = "Email", Subject = "Transfer Onayı", Body = "{{Amount}} {{Currency}} tutarındaki transferiniz {{BeneficiaryName}} hesabına başarıyla gönderildi.", Variables = "[\"Amount\", \"Currency\", \"BeneficiaryName\"]" },
            new { Name = "LoginAlert", Type = "Email", Subject = "Yeni Giriş Bildirimi", Body = "Hesabınıza {{DateTime}} tarihinde {{IpAddress}} IP adresinden giriş yapıldı.", Variables = "[\"DateTime\", \"IpAddress\"]" },
            new { Name = "OtpSms", Type = "SMS", Subject = (string?)null, Body = "Doğrulama kodunuz: {{OtpCode}}. Bu kod 5 dakika geçerlidir.", Variables = "[\"OtpCode\"]" },
            new { Name = "TransferSms", Type = "SMS", Subject = (string?)null, Body = "{{Amount}} TL transferiniz gerçekleşti. Güncel bakiye: {{Balance}} TL", Variables = "[\"Amount\", \"Balance\"]" },
            new { Name = "FraudAlert", Type = "Push", Subject = "Şüpheli İşlem Uyarısı", Body = "Hesabınızda şüpheli bir işlem tespit edildi. Lütfen kontrol edin.", Variables = "[]" },
            new { Name = "PaymentDue", Type = "Email", Subject = "Ödeme Hatırlatması", Body = "Sayın {{FirstName}}, {{CardLastFour}} ile biten kartınızın {{Amount}} TL tutarındaki asgari ödemesi {{DueDate}} tarihine kadar yapılmalıdır.", Variables = "[\"FirstName\", \"CardLastFour\", \"Amount\", \"DueDate\"]" },
            new { Name = "AccountLocked", Type = "Email", Subject = "Hesap Kilitlendi", Body = "Çok sayıda başarısız giriş denemesi nedeniyle hesabınız geçici olarak kilitlenmiştir.", Variables = "[]" },
            new { Name = "PasswordChanged", Type = "Email", Subject = "Şifre Değiştirildi", Body = "Hesap şifreniz başarıyla değiştirildi. Bu işlemi siz yapmadıysanız hemen bizimle iletişime geçin.", Variables = "[]" },
            new { Name = "KycApproved", Type = "Email", Subject = "Kimlik Doğrulama Onaylandı", Body = "Sayın {{FirstName}}, kimlik doğrulama işleminiz başarıyla tamamlandı. Artık tüm hizmetlerimizden yararlanabilirsiniz.", Variables = "[\"FirstName\"]" }
        };

        var sql = @"INSERT INTO notification_templates (template_name, template_type, subject, body, variables, language, is_active, created_at)
                    VALUES (@Name, @Type, @Subject, @Body, @Variables, 'TR', 1, @CreatedAt)";

        foreach (var t in templates)
        {
            await connection.ExecuteAsync(sql, new
            {
                t.Name,
                t.Type,
                t.Subject,
                t.Body,
                t.Variables,
                CreatedAt = DateTime.Now.AddDays(-_random.Next(30, 365))
            });
        }
        Console.WriteLine("✅ 10 bildirim şablonu eklendi.");
    }

    // ==================== MODÜL 9: CREDIT CARD APPLICATIONS ====================

    private async Task SeedCardApplicationsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM card_applications");
        if (count > 0) { Console.WriteLine($"Card Applications: {count} kayıt var, atlanıyor..."); return; }

        var userIds = (await connection.QueryAsync<int>("SELECT user_id FROM users")).ToList();
        var cardTypes = new[] { "Classic", "Gold", "Platinum", "Business" };
        var employmentStatuses = new[] { "Employed", "SelfEmployed", "Retired", "Student" };
        var statuses = new[] { "Approved", "Approved", "Approved", "Rejected", "Pending", "UnderReview" };
        var faker = new Faker("tr");

        var sql = @"INSERT INTO card_applications (user_id, card_type_requested, monthly_income, employment_status,
                    employer_name, application_date, status, rejection_reason, approved_by, approved_at, credit_limit_approved)
                    VALUES (@UserId, @CardType, @Income, @Employment, @Employer, @AppDate, @Status, @RejectionReason,
                    @ApprovedBy, @ApprovedAt, @CreditLimit)";

        for (int i = 0; i < 80; i++)
        {
            var status = statuses[_random.Next(statuses.Length)];
            var appDate = DateTime.Now.AddDays(-_random.Next(1, 180));
            var income = _random.Next(15000, 150000);

            await connection.ExecuteAsync(sql, new
            {
                UserId = userIds[_random.Next(userIds.Count)],
                CardType = cardTypes[_random.Next(cardTypes.Length)],
                Income = income,
                Employment = employmentStatuses[_random.Next(employmentStatuses.Length)],
                Employer = faker.Company.CompanyName(),
                AppDate = appDate,
                Status = status,
                RejectionReason = status == "Rejected" ? "Yetersiz gelir veya düşük kredi skoru" : null,
                ApprovedBy = status == "Approved" ? userIds[_random.Next(Math.Min(5, userIds.Count))] : (int?)null,
                ApprovedAt = status == "Approved" ? appDate.AddDays(_random.Next(1, 7)) : (DateTime?)null,
                CreditLimit = status == "Approved" ? (decimal?)(income * _random.Next(2, 5)) : null
            });
        }
        Console.WriteLine("✅ 80 kart başvurusu eklendi.");
    }

    private async Task SeedCardLimitsAsync(System.Data.IDbConnection connection)
    {
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM card_limits");
        if (count > 0) { Console.WriteLine($"Card Limits: {count} kayıt var, atlanıyor..."); return; }

        var cardIds = (await connection.QueryAsync<int>("SELECT card_id FROM credit_cards")).ToList();
        var limitTypes = new[] { "DailyOnline", "DailyContactless", "SingleTransaction", "DailyATM" };

        var sql = @"INSERT INTO card_limits (card_id, limit_type, limit_amount, used_amount, reset_date)
                    VALUES (@CardId, @LimitType, @LimitAmount, @UsedAmount, @ResetDate)";

        foreach (var cardId in cardIds)
        {
            foreach (var limitType in limitTypes)
            {
                var limitAmount = limitType switch
                {
                    "DailyOnline" => 20000m,
                    "DailyContactless" => 5000m,
                    "SingleTransaction" => 10000m,
                    "DailyATM" => 10000m,
                    _ => 5000m
                };

                await connection.ExecuteAsync(sql, new
                {
                    CardId = cardId,
                    LimitType = limitType,
                    LimitAmount = limitAmount,
                    UsedAmount = _random.Next(0, (int)(limitAmount * 0.5m)),
                    ResetDate = DateTime.Now.Date.AddDays(1)
                });
            }
        }
        Console.WriteLine("✅ Kart limitleri eklendi.");
    }

    // ==================== YARDIMCI SINIFLAR ====================

    private class UserSeed
    {
        public string TcKimlikNo { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = "";
        public bool EmailVerified { get; set; }
        public string? Phone { get; set; }
        public bool PhoneVerified { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string PasswordHash { get; set; } = "";
        public string PasswordSalt { get; set; } = "";
        public DateTime? PasswordChangedAt { get; set; }
        public string Status { get; set; } = "Active";
        public int FailedAttempts { get; set; }
        public string KycStatus { get; set; } = "Pending";
        public string RiskLevel { get; set; } = "Low";
        public bool IsPep { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}