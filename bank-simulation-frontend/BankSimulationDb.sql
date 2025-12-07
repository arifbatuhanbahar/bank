-- =============================================
-- BANKA SİMÜLASYONU VERİTABANI ŞEMASI
-- Temiz SQL Script (EF Core Bağımlılığı Yok)
-- 38 Tablo - 9 Modül
-- =============================================

-- Veritabanını Oluştur
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'BankSimulationDb')
BEGIN
    ALTER DATABASE BankSimulationDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BankSimulationDb;
END
GO

CREATE DATABASE BankSimulationDb;
GO

USE BankSimulationDb;
GO

-- =============================================
-- MODÜL 1: USER MANAGEMENT (5 Tablo)
-- =============================================

-- 1.1 Users (Ana Kullanıcı Tablosu)
CREATE TABLE [users] (
    [user_id] int NOT NULL IDENTITY(1,1),
    [tc_kimlik_no] nchar(11) NOT NULL,
    [first_name] nvarchar(100) NOT NULL,
    [last_name] nvarchar(100) NOT NULL,
    [date_of_birth] date NOT NULL,
    [email] nvarchar(255) NOT NULL,
    [email_verified] bit NOT NULL DEFAULT 0,
    [phone] nvarchar(20) NULL,
    [phone_verified] bit NOT NULL DEFAULT 0,
    [address_line1] nvarchar(255) NULL,
    [address_line2] nvarchar(255) NULL,
    [city] nvarchar(100) NULL,
    [postal_code] nvarchar(10) NULL,
    [country] nvarchar(2) NOT NULL DEFAULT N'TR',
    [password_hash] nvarchar(512) NOT NULL,
    [password_salt] nvarchar(256) NOT NULL,
    [password_changed_at] datetime2 NULL,
    [status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [failed_login_attempts] int NOT NULL DEFAULT 0,
    [locked_until] datetime2 NULL,
    [kyc_status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [risk_level] nvarchar(10) NOT NULL DEFAULT N'Low',
    [is_pep] bit NOT NULL DEFAULT 0,
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [updated_at] datetime2 NULL,
    [last_login_at] datetime2 NULL,
    [deleted_at] datetime2 NULL,
    [gdpr_anonymized] bit NOT NULL DEFAULT 0,
    CONSTRAINT [PK_users] PRIMARY KEY ([user_id])
);

-- Users Indexleri
CREATE UNIQUE INDEX [IX_users_tc_kimlik_no] ON [users] ([tc_kimlik_no]);
CREATE UNIQUE INDEX [IX_users_email] ON [users] ([email]);
CREATE INDEX [IX_users_status_kyc] ON [users] ([status], [kyc_status]);

-- 1.2 User Roles (Kullanıcı Rolleri)
CREATE TABLE [user_roles] (
    [role_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [role_name] nvarchar(20) NOT NULL,
    [assigned_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [assigned_by] int NULL,
    [expires_at] datetime2 NULL,
    CONSTRAINT [PK_user_roles] PRIMARY KEY ([role_id]),
    CONSTRAINT [FK_user_roles_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]),
    CONSTRAINT [FK_user_roles_users_assigned_by] FOREIGN KEY ([assigned_by]) REFERENCES [users] ([user_id])
);

CREATE UNIQUE INDEX [IX_user_roles_user_role_unique] ON [user_roles] ([user_id], [role_name]);

-- 1.3 User Sessions (Oturum Yönetimi)
CREATE TABLE [user_sessions] (
    [session_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [session_token] nvarchar(512) NOT NULL,
    [ip_address] nvarchar(45) NULL,
    [user_agent] nvarchar(500) NULL,
    [device_info] nvarchar(500) NULL,
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [expires_at] datetime2 NOT NULL,
    [last_activity_at] datetime2 NULL,
    [is_active] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_user_sessions] PRIMARY KEY ([session_id]),
    CONSTRAINT [FK_user_sessions_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id])
);

CREATE UNIQUE INDEX [IX_user_sessions_token] ON [user_sessions] ([session_token]);
CREATE INDEX [IX_user_sessions_user_active] ON [user_sessions] ([user_id], [is_active]);

-- 1.4 Login Attempts (Giriş Denemeleri)
CREATE TABLE [login_attempts] (
    [attempt_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NULL,
    [email_attempted] nvarchar(255) NOT NULL,
    [ip_address] nvarchar(45) NULL,
    [success] bit NOT NULL,
    [failure_reason] nvarchar(255) NULL,
    [attempted_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [user_agent] nvarchar(500) NULL,
    CONSTRAINT [PK_login_attempts] PRIMARY KEY ([attempt_id]),
    CONSTRAINT [FK_login_attempts_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id])
);

CREATE INDEX [IX_login_attempts_ip_time] ON [login_attempts] ([ip_address], [attempted_at]);
CREATE INDEX [IX_login_attempts_email_time] ON [login_attempts] ([email_attempted], [attempted_at]);

-- 1.5 Password History (Şifre Geçmişi)
CREATE TABLE [password_history] (
    [history_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [password_hash] nvarchar(512) NOT NULL,
    [changed_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [changed_by] int NULL,
    CONSTRAINT [PK_password_history] PRIMARY KEY ([history_id]),
    CONSTRAINT [FK_password_history_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]),
    CONSTRAINT [FK_password_history_users_changed_by] FOREIGN KEY ([changed_by]) REFERENCES [users] ([user_id])
);

CREATE INDEX [IX_password_history_user_date] ON [password_history] ([user_id], [changed_at]);

-- =============================================
-- MODÜL 2: ACCOUNT MANAGEMENT (4 Tablo)
-- =============================================

-- 2.1 Account Types (Hesap Türleri Tanım)
CREATE TABLE [account_types] (
    [type_id] int NOT NULL IDENTITY(1,1),
    [type_name] nvarchar(50) NOT NULL,
    [description] nvarchar(500) NULL,
    [min_balance] decimal(18,2) NOT NULL DEFAULT 0,
    [interest_rate] decimal(5,2) NOT NULL DEFAULT 0,
    [features] nvarchar(max) NULL,
    [is_active] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_account_types] PRIMARY KEY ([type_id])
);

-- 2.2 Accounts (Banka Hesapları)
CREATE TABLE [accounts] (
    [account_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [account_number] nvarchar(34) NOT NULL,
    [account_type] nvarchar(20) NOT NULL,
    [currency] nvarchar(3) NOT NULL DEFAULT N'TRY',
    [balance] decimal(18,2) NOT NULL DEFAULT 0,
    [available_balance] decimal(18,2) NOT NULL DEFAULT 0,
    [daily_transfer_limit] decimal(18,2) NOT NULL DEFAULT 50000,
    [daily_withdrawal_limit] decimal(18,2) NOT NULL DEFAULT 10000,
    [interest_rate] decimal(5,2) NOT NULL DEFAULT 0,
    [interest_calculation_date] datetime2 NULL,
    [status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [opened_date] datetime2 NOT NULL DEFAULT GETDATE(),
    [closed_date] datetime2 NULL,
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [updated_at] datetime2 NULL,
    CONSTRAINT [PK_accounts] PRIMARY KEY ([account_id]),
    CONSTRAINT [FK_accounts_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id])
);

CREATE UNIQUE INDEX [IX_accounts_account_number] ON [accounts] ([account_number]);
CREATE INDEX [IX_accounts_user_status] ON [accounts] ([user_id], [status]);

-- 2.3 Account Beneficiaries (Kayıtlı Alıcılar)
CREATE TABLE [account_beneficiaries] (
    [beneficiary_id] int NOT NULL IDENTITY(1,1),
    [account_id] int NOT NULL,
    [beneficiary_name] nvarchar(200) NOT NULL,
    [beneficiary_iban] nvarchar(34) NOT NULL,
    [beneficiary_bank] nvarchar(100) NULL,
    [nickname] nvarchar(50) NULL,
    [is_verified] bit NOT NULL DEFAULT 0,
    [added_at] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_account_beneficiaries] PRIMARY KEY ([beneficiary_id]),
    CONSTRAINT [FK_account_beneficiaries_accounts] FOREIGN KEY ([account_id]) REFERENCES [accounts] ([account_id]) ON DELETE CASCADE
);

CREATE INDEX [IX_account_beneficiaries_account] ON [account_beneficiaries] ([account_id]);

-- 2.4 Account Limits (Hesap Limitleri)
CREATE TABLE [account_limits] (
    [limit_id] int NOT NULL IDENTITY(1,1),
    [account_id] int NOT NULL,
    [limit_type] nvarchar(20) NOT NULL,
    [limit_amount] decimal(18,2) NOT NULL,
    [used_amount] decimal(18,2) NOT NULL DEFAULT 0,
    [reset_date] datetime2 NOT NULL,
    [last_updated] datetime2 NULL,
    CONSTRAINT [PK_account_limits] PRIMARY KEY ([limit_id]),
    CONSTRAINT [FK_account_limits_accounts] FOREIGN KEY ([account_id]) REFERENCES [accounts] ([account_id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_account_limits_account_type_unique] ON [account_limits] ([account_id], [limit_type]);

-- =============================================
-- MODÜL 3: TRANSACTION MANAGEMENT (6 Tablo)
-- =============================================

-- 3.1 Transaction Types (İşlem Türleri Tanım)
CREATE TABLE [transaction_types] (
    [type_id] int NOT NULL IDENTITY(1,1),
    [type_name] nvarchar(50) NOT NULL,
    [type_code] nvarchar(20) NOT NULL,
    [description] nvarchar(255) NULL,
    [fee_fixed] decimal(18,2) NOT NULL DEFAULT 0,
    [fee_percentage] decimal(5,4) NOT NULL DEFAULT 0,
    [is_active] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_transaction_types] PRIMARY KEY ([type_id])
);

-- 3.2 Transactions (Ana İşlem Tablosu)
CREATE TABLE [transactions] (
    [transaction_id] int NOT NULL IDENTITY(1,1),
    [from_account_id] int NULL,
    [to_account_id] int NULL,
    [amount] decimal(18,2) NOT NULL,
    [currency] nvarchar(3) NOT NULL DEFAULT N'TRY',
    [transaction_type] nvarchar(20) NOT NULL,
    [description] nvarchar(255) NULL,
    [reference_number] nvarchar(50) NOT NULL,
    [status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [fraud_score] int NOT NULL DEFAULT 0,
    [fraud_flags] nvarchar(max) NULL,
    [requires_review] bit NOT NULL DEFAULT 0,
    [reviewed_by] int NULL,
    [reviewed_at] datetime2 NULL,
    [transaction_date] datetime2 NOT NULL DEFAULT GETDATE(),
    [completed_at] datetime2 NULL,
    [created_by] int NULL,
    [ip_address] nvarchar(45) NULL,
    [user_agent] nvarchar(500) NULL,
    [is_suspicious] bit NOT NULL DEFAULT 0,
    [suspicious_reason] nvarchar(255) NULL,
    [reported_to_masak] bit NOT NULL DEFAULT 0,
    [masak_report_date] datetime2 NULL,
    CONSTRAINT [PK_transactions] PRIMARY KEY ([transaction_id]),
    CONSTRAINT [FK_transactions_accounts_from] FOREIGN KEY ([from_account_id]) REFERENCES [accounts] ([account_id]),
    CONSTRAINT [FK_transactions_accounts_to] FOREIGN KEY ([to_account_id]) REFERENCES [accounts] ([account_id]),
    CONSTRAINT [FK_transactions_users_reviewed_by] FOREIGN KEY ([reviewed_by]) REFERENCES [users] ([user_id]),
    CONSTRAINT [FK_transactions_users_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([user_id])
);

CREATE UNIQUE INDEX [IX_transactions_reference_number] ON [transactions] ([reference_number]);
CREATE INDEX [IX_transactions_date] ON [transactions] ([transaction_date]);
CREATE INDEX [IX_transactions_from_date] ON [transactions] ([from_account_id], [transaction_date]);
CREATE INDEX [IX_transactions_to_date] ON [transactions] ([to_account_id], [transaction_date]);

-- 3.3 Transaction Fees (İşlem Ücretleri)
CREATE TABLE [transaction_fees] (
    [fee_id] int NOT NULL IDENTITY(1,1),
    [transaction_id] int NOT NULL,
    [fee_type] nvarchar(50) NOT NULL,
    [fee_amount] decimal(18,2) NOT NULL,
    [applied_at] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_transaction_fees] PRIMARY KEY ([fee_id]),
    CONSTRAINT [FK_transaction_fees_transactions] FOREIGN KEY ([transaction_id]) REFERENCES [transactions] ([transaction_id]) ON DELETE CASCADE
);

CREATE INDEX [IX_transaction_fees_transaction_id] ON [transaction_fees] ([transaction_id]);

-- 3.4 Scheduled Transactions (Planlanmış İşlemler)
CREATE TABLE [scheduled_transactions] (
    [schedule_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [from_account_id] int NOT NULL,
    [to_account_id] int NOT NULL,
    [amount] decimal(18,2) NOT NULL,
    [frequency] nvarchar(20) NOT NULL,
    [start_date] date NOT NULL,
    [end_date] date NULL,
    [next_execution_date] date NOT NULL,
    [last_execution_date] datetime2 NULL,
    [status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_scheduled_transactions] PRIMARY KEY ([schedule_id]),
    CONSTRAINT [FK_scheduled_transactions_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]),
    CONSTRAINT [FK_scheduled_transactions_accounts_from] FOREIGN KEY ([from_account_id]) REFERENCES [accounts] ([account_id]),
    CONSTRAINT [FK_scheduled_transactions_accounts_to] FOREIGN KEY ([to_account_id]) REFERENCES [accounts] ([account_id])
);

-- 3.5 Transaction Approvals (İşlem Onayları)
CREATE TABLE [transaction_approvals] (
    [approval_id] int NOT NULL IDENTITY(1,1),
    [transaction_id] int NOT NULL,
    [approver_id] int NOT NULL,
    [approval_level] int NOT NULL DEFAULT 1,
    [status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [comments] nvarchar(500) NULL,
    [approved_at] datetime2 NULL,
    CONSTRAINT [PK_transaction_approvals] PRIMARY KEY ([approval_id]),
    CONSTRAINT [FK_transaction_approvals_transactions] FOREIGN KEY ([transaction_id]) REFERENCES [transactions] ([transaction_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_transaction_approvals_users] FOREIGN KEY ([approver_id]) REFERENCES [users] ([user_id])
);

-- 3.6 General Ledger (Muhasebe Kayıtları)
CREATE TABLE [general_ledger] (
    [ledger_id] int NOT NULL IDENTITY(1,1),
    [transaction_id] int NOT NULL,
    [account_id] int NOT NULL,
    [debit_amount] decimal(18,2) NOT NULL DEFAULT 0,
    [credit_amount] decimal(18,2) NOT NULL DEFAULT 0,
    [entry_type] nvarchar(10) NOT NULL,
    [entry_date] datetime2 NOT NULL DEFAULT GETDATE(),
    [description] nvarchar(255) NULL,
    CONSTRAINT [PK_general_ledger] PRIMARY KEY ([ledger_id]),
    CONSTRAINT [FK_general_ledger_transactions] FOREIGN KEY ([transaction_id]) REFERENCES [transactions] ([transaction_id]),
    CONSTRAINT [FK_general_ledger_accounts] FOREIGN KEY ([account_id]) REFERENCES [accounts] ([account_id])
);

-- =============================================
-- MODÜL 4: PAYMENT & CARDS (5 Tablo)
-- =============================================

-- 4.1 Credit Cards (Kredi Kartları)
CREATE TABLE [credit_cards] (
    [card_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [card_number_encrypted] nvarchar(512) NOT NULL,
    [card_last_four] nvarchar(4) NOT NULL,
    [card_type] nvarchar(20) NOT NULL,
    [card_brand] nvarchar(20) NOT NULL,
    [credit_limit] decimal(18,2) NOT NULL,
    [available_limit] decimal(18,2) NOT NULL,
    [current_balance] decimal(18,2) NOT NULL DEFAULT 0,
    [minimum_payment] decimal(18,2) NOT NULL DEFAULT 0,
    [payment_due_date] date NOT NULL,
    [interest_rate] decimal(5,2) NOT NULL,
    [expiry_month] int NOT NULL,
    [expiry_year] int NOT NULL,
    [cvv_encrypted] nvarchar(512) NOT NULL,
    [status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [issued_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [activated_at] datetime2 NULL,
    CONSTRAINT [PK_credit_cards] PRIMARY KEY ([card_id]),
    CONSTRAINT [FK_credit_cards_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id])
);

CREATE INDEX [IX_credit_cards_user_id] ON [credit_cards] ([user_id]);

-- 4.2 Card Transactions (Kart Harcamaları)
CREATE TABLE [card_transactions] (
    [card_transaction_id] int NOT NULL IDENTITY(1,1),
    [card_id] int NOT NULL,
    [merchant_name] nvarchar(100) NOT NULL,
    [merchant_category] nvarchar(50) NOT NULL,
    [amount] decimal(18,2) NOT NULL,
    [currency] nvarchar(3) NOT NULL DEFAULT N'TRY',
    [transaction_date] datetime2 NOT NULL DEFAULT GETDATE(),
    [status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [authorization_code] nvarchar(50) NOT NULL,
    [ip_address] nvarchar(45) NULL,
    [location] nvarchar(100) NULL,
    CONSTRAINT [PK_card_transactions] PRIMARY KEY ([card_transaction_id]),
    CONSTRAINT [FK_card_transactions_credit_cards] FOREIGN KEY ([card_id]) REFERENCES [credit_cards] ([card_id])
);

CREATE INDEX [IX_card_transactions_card_id] ON [card_transactions] ([card_id]);

-- 4.3 Payment Methods (Kayıtlı Ödeme Yöntemleri)
CREATE TABLE [payment_methods] (
    [payment_method_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [method_type] nvarchar(20) NOT NULL,
    [card_token] nvarchar(512) NOT NULL,
    [card_last_four] nvarchar(4) NOT NULL,
    [card_brand] nvarchar(20) NOT NULL,
    [expiry_month] int NOT NULL,
    [expiry_year] int NOT NULL,
    [cardholder_name] nvarchar(100) NOT NULL,
    [is_default] bit NOT NULL DEFAULT 0,
    [status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [last_used_at] datetime2 NULL,
    CONSTRAINT [PK_payment_methods] PRIMARY KEY ([payment_method_id]),
    CONSTRAINT [FK_payment_methods_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE
);

CREATE INDEX [IX_payment_methods_user_id] ON [payment_methods] ([user_id]);

-- 4.4 Recurring Payments (Düzenli Ödemeler)
CREATE TABLE [recurring_payments] (
    [recurring_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [payment_method_id] int NOT NULL,
    [merchant_name] nvarchar(100) NOT NULL,
    [amount] decimal(18,2) NOT NULL,
    [frequency] nvarchar(20) NOT NULL,
    [next_payment_date] date NOT NULL,
    [last_payment_date] date NULL,
    [status] nvarchar(20) NOT NULL DEFAULT N'Active',
    CONSTRAINT [PK_recurring_payments] PRIMARY KEY ([recurring_id]),
    CONSTRAINT [FK_recurring_payments_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]),
    CONSTRAINT [FK_recurring_payments_payment_methods] FOREIGN KEY ([payment_method_id]) REFERENCES [payment_methods] ([payment_method_id])
);

-- 4.5 Payment Gateways (Ödeme Sağlayıcıları)
CREATE TABLE [payment_gateways] (
    [gateway_id] int NOT NULL IDENTITY(1,1),
    [gateway_name] nvarchar(100) NOT NULL,
    [gateway_type] nvarchar(20) NOT NULL,
    [api_endpoint] nvarchar(255) NOT NULL,
    [api_key_encrypted] nvarchar(512) NOT NULL,
    [is_active] bit NOT NULL DEFAULT 1,
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_payment_gateways] PRIMARY KEY ([gateway_id])
);

-- =============================================
-- MODÜL 5: COMPLIANCE & KYC (6 Tablo)
-- =============================================

-- 5.1 KYC Documents (KYC Belgeleri)
CREATE TABLE [kyc_documents] (
    [document_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [document_type] nvarchar(50) NOT NULL,
    [document_number] nvarchar(50) NOT NULL,
    [document_file_path] nvarchar(255) NOT NULL,
    [document_hash] nvarchar(255) NULL,
    [expiry_date] date NULL,
    [upload_date] datetime2 NOT NULL DEFAULT GETDATE(),
    [verified_at] datetime2 NULL,
    [verified_by] int NULL,
    [verification_status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [rejection_reason] nvarchar(255) NULL,
    CONSTRAINT [PK_kyc_documents] PRIMARY KEY ([document_id]),
    CONSTRAINT [FK_kyc_documents_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_kyc_documents_users_verified_by] FOREIGN KEY ([verified_by]) REFERENCES [users] ([user_id])
);

-- 5.2 KYC Verifications (Doğrulama İşlemleri)
CREATE TABLE [kyc_verifications] (
    [verification_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [verification_type] nvarchar(20) NOT NULL,
    [verification_method] nvarchar(20) NOT NULL,
    [verification_status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [verification_code] nvarchar(10) NULL,
    [verified_at] datetime2 NULL,
    [attempts] int NOT NULL DEFAULT 0,
    [expires_at] datetime2 NOT NULL,
    CONSTRAINT [PK_kyc_verifications] PRIMARY KEY ([verification_id]),
    CONSTRAINT [FK_kyc_verifications_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE
);

-- 5.3 KVKK Consents (KVKK Onayları)
CREATE TABLE [kvkk_consents] (
    [consent_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [consent_type] nvarchar(50) NOT NULL,
    [consent_given] bit NOT NULL,
    [consent_text] nvarchar(max) NOT NULL,
    [consent_version] nvarchar(20) NOT NULL,
    [granted_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [revoked_at] datetime2 NULL,
    [ip_address] nvarchar(45) NULL,
    CONSTRAINT [PK_kvkk_consents] PRIMARY KEY ([consent_id]),
    CONSTRAINT [FK_kvkk_consents_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE
);

-- 5.4 KVKK Data Requests (Veri Talepleri)
CREATE TABLE [kvkk_data_requests] (
    [request_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [request_type] nvarchar(20) NOT NULL,
    [request_date] datetime2 NOT NULL DEFAULT GETDATE(),
    [status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [completed_at] datetime2 NULL,
    [completed_by] int NULL,
    [response_data] nvarchar(max) NULL,
    [response_file_path] nvarchar(255) NULL,
    CONSTRAINT [PK_kvkk_data_requests] PRIMARY KEY ([request_id]),
    CONSTRAINT [FK_kvkk_data_requests_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_kvkk_data_requests_users_completed_by] FOREIGN KEY ([completed_by]) REFERENCES [users] ([user_id])
);

-- 5.5 MASAK Records (MASAK Kayıtları)
CREATE TABLE [masak_records] (
    [record_id] int NOT NULL IDENTITY(1,1),
    [customer_id] int NOT NULL,
    [transaction_id] int NULL,
    [record_type] nvarchar(50) NOT NULL,
    [data] nvarchar(max) NOT NULL,
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [retention_until] date NOT NULL,
    CONSTRAINT [PK_masak_records] PRIMARY KEY ([record_id]),
    CONSTRAINT [FK_masak_records_users] FOREIGN KEY ([customer_id]) REFERENCES [users] ([user_id]),
    CONSTRAINT [FK_masak_records_transactions] FOREIGN KEY ([transaction_id]) REFERENCES [transactions] ([transaction_id])
);

-- 5.6 Suspicious Activity Reports (Şüpheli İşlem Raporları)
CREATE TABLE [suspicious_activity_reports] (
    [sar_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [transaction_id] int NULL,
    [report_type] nvarchar(50) NOT NULL,
    [description] nvarchar(1000) NOT NULL,
    [risk_score] int NOT NULL DEFAULT 0,
    [created_by] int NULL,
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [reported_to_masak] bit NOT NULL DEFAULT 0,
    [masak_report_date] datetime2 NULL,
    [masak_reference_number] nvarchar(100) NULL,
    [status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    CONSTRAINT [PK_suspicious_activity_reports] PRIMARY KEY ([sar_id]),
    CONSTRAINT [FK_suspicious_activity_reports_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]),
    CONSTRAINT [FK_suspicious_activity_reports_transactions] FOREIGN KEY ([transaction_id]) REFERENCES [transactions] ([transaction_id]),
    CONSTRAINT [FK_suspicious_activity_reports_users_created_by] FOREIGN KEY ([created_by]) REFERENCES [users] ([user_id])
);

-- =============================================
-- MODÜL 6: AUDIT & SECURITY (5 Tablo)
-- =============================================

-- 6.1 Audit Logs (Denetim Kayıtları)
CREATE TABLE [audit_logs] (
    [log_id] int NOT NULL IDENTITY(1,1),
    [table_name] nvarchar(100) NOT NULL,
    [record_id] nvarchar(50) NOT NULL,
    [action] nvarchar(20) NOT NULL,
    [old_value] nvarchar(max) NULL,
    [new_value] nvarchar(max) NULL,
    [changed_fields] nvarchar(max) NULL,
    [changed_by] int NULL,
    [ip_address] nvarchar(45) NULL,
    [timestamp] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_audit_logs] PRIMARY KEY ([log_id])
);

-- 6.2 Security Events (Güvenlik Olayları)
CREATE TABLE [security_events] (
    [event_id] int NOT NULL IDENTITY(1,1),
    [event_type] nvarchar(50) NOT NULL,
    [user_id] int NULL,
    [severity] nvarchar(20) NOT NULL,
    [description] nvarchar(500) NOT NULL,
    [ip_address] nvarchar(45) NULL,
    [user_agent] nvarchar(500) NULL,
    [event_date] datetime2 NOT NULL DEFAULT GETDATE(),
    [resolved] bit NOT NULL DEFAULT 0,
    [resolved_at] datetime2 NULL,
    [resolved_by] int NULL,
    CONSTRAINT [PK_security_events] PRIMARY KEY ([event_id]),
    CONSTRAINT [FK_security_events_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE SET NULL,
    CONSTRAINT [FK_security_events_users_resolved_by] FOREIGN KEY ([resolved_by]) REFERENCES [users] ([user_id])
);

-- 6.3 Data Access Log (Veri Erişim Kaydı)
CREATE TABLE [data_access_log] (
    [log_id] int NOT NULL IDENTITY(1,1),
    [accessed_by_user_id] int NOT NULL,
    [target_user_id] int NOT NULL,
    [data_type] nvarchar(50) NOT NULL,
    [access_reason] nvarchar(255) NULL,
    [access_timestamp] datetime2 NOT NULL DEFAULT GETDATE(),
    [ip_address] nvarchar(45) NULL,
    [is_sensitive] bit NOT NULL DEFAULT 0,
    CONSTRAINT [PK_data_access_log] PRIMARY KEY ([log_id]),
    CONSTRAINT [FK_data_access_log_users_accessed_by] FOREIGN KEY ([accessed_by_user_id]) REFERENCES [users] ([user_id]),
    CONSTRAINT [FK_data_access_log_users_target] FOREIGN KEY ([target_user_id]) REFERENCES [users] ([user_id])
);

-- 6.4 PCI Audit Log (Kart Bilgisi Erişim Logu)
CREATE TABLE [pci_audit_log] (
    [log_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [action] nvarchar(50) NOT NULL,
    [card_token] nvarchar(512) NOT NULL,
    [timestamp] datetime2 NOT NULL DEFAULT GETDATE(),
    [ip_address] nvarchar(45) NULL,
    [success] bit NOT NULL,
    [reason] nvarchar(255) NULL,
    CONSTRAINT [PK_pci_audit_log] PRIMARY KEY ([log_id]),
    CONSTRAINT [FK_pci_audit_log_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id])
);

-- 6.5 Encryption Keys (Şifreleme Anahtarları)
CREATE TABLE [encryption_keys] (
    [key_id] int NOT NULL IDENTITY(1,1),
    [key_name] nvarchar(100) NOT NULL,
    [key_type] nvarchar(20) NOT NULL,
    [key_value_encrypted] nvarchar(1024) NOT NULL,
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [expires_at] date NOT NULL,
    [rotation_date] datetime2 NULL,
    [status] nvarchar(20) NOT NULL DEFAULT N'Active',
    CONSTRAINT [PK_encryption_keys] PRIMARY KEY ([key_id])
);

-- =============================================
-- MODÜL 7: FRAUD & RISK (3 Tablo)
-- =============================================

-- 7.1 Fraud Rules (Dolandırıcılık Kuralları)
CREATE TABLE [fraud_rules] (
    [rule_id] int NOT NULL IDENTITY(1,1),
    [rule_name] nvarchar(100) NOT NULL,
    [rule_type] nvarchar(50) NOT NULL,
    [rule_description] nvarchar(500) NULL,
    [rule_conditions] nvarchar(max) NOT NULL,
    [risk_score_weight] int NOT NULL DEFAULT 0,
    [is_active] bit NOT NULL DEFAULT 1,
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [updated_at] datetime2 NULL,
    CONSTRAINT [PK_fraud_rules] PRIMARY KEY ([rule_id])
);

-- 7.2 Fraud Alerts (Dolandırıcılık Alarmları)
CREATE TABLE [fraud_alerts] (
    [alert_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [transaction_id] int NOT NULL,
    [fraud_score] int NOT NULL,
    [triggered_rules] nvarchar(max) NULL,
    [alert_severity] nvarchar(20) NOT NULL,
    [status] nvarchar(20) NOT NULL DEFAULT N'Open',
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [reviewed_at] datetime2 NULL,
    [reviewed_by] int NULL,
    [resolution_notes] nvarchar(500) NULL,
    CONSTRAINT [PK_fraud_alerts] PRIMARY KEY ([alert_id]),
    CONSTRAINT [FK_fraud_alerts_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]),
    CONSTRAINT [FK_fraud_alerts_transactions] FOREIGN KEY ([transaction_id]) REFERENCES [transactions] ([transaction_id]),
    CONSTRAINT [FK_fraud_alerts_users_reviewed_by] FOREIGN KEY ([reviewed_by]) REFERENCES [users] ([user_id])
);

-- 7.3 Risk Profiles (Risk Profilleri)
CREATE TABLE [risk_profiles] (
    [profile_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [risk_level] nvarchar(20) NOT NULL DEFAULT N'Low',
    [transaction_velocity_score] int NOT NULL DEFAULT 0,
    [amount_anomaly_score] int NOT NULL DEFAULT 0,
    [geographic_risk_score] int NOT NULL DEFAULT 0,
    [behavioral_score] int NOT NULL DEFAULT 0,
    [last_calculated_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [factors] nvarchar(max) NULL,
    CONSTRAINT [PK_risk_profiles] PRIMARY KEY ([profile_id]),
    CONSTRAINT [FK_risk_profiles_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE
);

-- =============================================
-- MODÜL 8: SYSTEM & CONFIGURATION (2 Tablo)
-- =============================================

-- 8.1 System Settings (Sistem Ayarları)
CREATE TABLE [system_settings] (
    [setting_id] int NOT NULL IDENTITY(1,1),
    [setting_key] nvarchar(100) NOT NULL,
    [setting_value] nvarchar(500) NOT NULL,
    [setting_type] nvarchar(20) NOT NULL,
    [description] nvarchar(255) NULL,
    [updated_at] datetime2 NOT NULL DEFAULT GETDATE(),
    [updated_by] int NULL,
    CONSTRAINT [PK_system_settings] PRIMARY KEY ([setting_id]),
    CONSTRAINT [FK_system_settings_users] FOREIGN KEY ([updated_by]) REFERENCES [users] ([user_id])
);

CREATE UNIQUE INDEX [IX_system_settings_setting_key] ON [system_settings] ([setting_key]);

-- 8.2 Notification Templates (Bildirim Şablonları)
CREATE TABLE [notification_templates] (
    [template_id] int NOT NULL IDENTITY(1,1),
    [template_name] nvarchar(100) NOT NULL,
    [template_type] nvarchar(20) NOT NULL,
    [subject] nvarchar(255) NULL,
    [body] nvarchar(max) NOT NULL,
    [variables] nvarchar(max) NULL,
    [language] nvarchar(5) NOT NULL DEFAULT N'TR',
    [is_active] bit NOT NULL DEFAULT 1,
    [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_notification_templates] PRIMARY KEY ([template_id])
);

-- =============================================
-- MODÜL 9: CREDIT CARD APPLICATIONS (2 Tablo)
-- =============================================

-- 9.1 Card Applications (Kart Başvuruları)
CREATE TABLE [card_applications] (
    [application_id] int NOT NULL IDENTITY(1,1),
    [user_id] int NOT NULL,
    [card_type_requested] nvarchar(50) NOT NULL,
    [monthly_income] decimal(18,2) NOT NULL,
    [employment_status] nvarchar(50) NOT NULL,
    [employer_name] nvarchar(100) NOT NULL,
    [application_date] datetime2 NOT NULL DEFAULT GETDATE(),
    [status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [rejection_reason] nvarchar(255) NULL,
    [approved_by] int NULL,
    [approved_at] datetime2 NULL,
    [credit_limit_approved] decimal(18,2) NULL,
    CONSTRAINT [PK_card_applications] PRIMARY KEY ([application_id]),
    CONSTRAINT [FK_card_applications_users] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_card_applications_users_approved_by] FOREIGN KEY ([approved_by]) REFERENCES [users] ([user_id])
);

-- 9.2 Card Limits (Kart Limitleri)
CREATE TABLE [card_limits] (
    [limit_id] int NOT NULL IDENTITY(1,1),
    [card_id] int NOT NULL,
    [limit_type] nvarchar(50) NOT NULL,
    [limit_amount] decimal(18,2) NOT NULL,
    [used_amount] decimal(18,2) NOT NULL DEFAULT 0,
    [reset_date] datetime2 NULL,
    CONSTRAINT [PK_card_limits] PRIMARY KEY ([limit_id]),
    CONSTRAINT [FK_card_limits_credit_cards] FOREIGN KEY ([card_id]) REFERENCES [credit_cards] ([card_id]) ON DELETE CASCADE
);

-- =============================================
-- ÖZET: 38 TABLO OLUŞTURULDU
-- =============================================
-- Modül 1: User Management      -> 5 tablo
-- Modül 2: Account Management   -> 4 tablo
-- Modül 3: Transaction Mgmt     -> 6 tablo
-- Modül 4: Payment & Cards      -> 5 tablo
-- Modül 5: Compliance & KYC     -> 6 tablo
-- Modül 6: Audit & Security     -> 5 tablo
-- Modül 7: Fraud & Risk         -> 3 tablo
-- Modül 8: System & Config      -> 2 tablo
-- Modül 9: Credit Card Apps     -> 2 tablo
-- TOPLAM                        -> 38 tablo
-- =============================================

PRINT 'Veritabanı başarıyla oluşturuldu: BankSimulationDb';
PRINT 'Toplam 38 tablo oluşturuldu.';
GO