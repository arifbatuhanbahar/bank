## Proje Final Dokumani

**Ogrenci Adi Soyadi:** ...  
**Ogrenci Numarasi:** ...

T.C. MARMARA UNIVERSITESI TEKNOLOJI FAKULTESI  
2025-2026 EGITIM OGRETIM YILI  
GUZ DONEMI BILGISAYAR MUHENDISLIGI BOLUMU  
VERITABANI YONETIM SISTEMLERI DERSI ? PROJE FINAL DOKUMANI

> Not: Bu rapor Bank Simulation projesi icindir; gercek ogrenci bilgilerini doldurunuz.

---

### 1. Grup ve Bilesenler (5p)
- Grup: Arif Batuhan Bahar, Ahmet Seker, Bugra Alpaslan
- Veritabani: SQL Server (BankSimulationDb).
- Backend: .NET 9 Web API (katmanli: API, Domain, Infrastructure).
- Frontend: React + Vite + MUI.
- Ana moduller: Hesap yonetimi, para transferi, kart basvurusu/islemleri, KYC/KVKK uyumluluk, raporlama.

### 2. GitHub Baglantilari (5p)
- Tek repo (backend + frontend + DB): https://github.com/arifbatuhanbahar/bank.git

### 3. Projenin Amaci (10p)
Musterilerin coklu para birimli hesaplarini yonetebildigi, para transferi ve kart islemleri yapabildigi, KYC/KVKK uyumluluk adimlarini tamamlayabildigi, guvenlik (audit/fraud) kayitlarini tutan bir bankacilik simulasyonu gelistirmek. Amac; uctan uca VTYS uygulamasinda sema tasarimi, is kurallari ve servis entegrasyonunu gostermek.

### 4. VTYS Mimarisi ve Iliskiler (10p)
- Tablolar (ornek):
  - Users (UserId PK, FirstName, LastName, Email, Phone, CreatedAt, Status)
  - Accounts (AccountId PK, UserId FK, Currency, Balance, CreatedAt, Status)
  - Transactions (TransactionId PK, FromAccountId FK, ToAccountId FK, Amount, Currency, Type, Status, CreatedAt, Description)
  - Cards (CardId PK, UserId FK, CardType, Prestige, CreditLimit, Balance, Status, CreatedAt)
  - CardApplications (ApplicationId PK, UserId FK, PrestigeLevel, Income, Status, CreatedAt)
  - KycDocuments (DocumentId PK, UserId FK, Type, Status, UploadedAt)
  - KvkkConsents (ConsentId PK, UserId FK, Type, GivenAt, RevokedAt)
- Iliskiler:
  - Bir User cok Account, Card, Transaction (giden/gelen), CardApplication, KycDocument, KvkkConsent kaydina sahiptir.
  - Transactions hem kaynak hem hedef hesap FK icerir (self-reference).
  - CardApplications -> Cards (onay sonrasi).

### 5. ER Diyagrami (10p)
Bilgisayar ortaminda cizilmis ER diyagramini ekleyin (ornek: docs/er-diagram.png):
```
![ER Diagram](docs/er-diagram.png)
```

### 6. Iki Tablonun DDL Kodlari (10p)
```sql
CREATE TABLE Users (
  UserId       INT IDENTITY(1,1) PRIMARY KEY,
  FirstName    NVARCHAR(100) NOT NULL,
  LastName     NVARCHAR(100) NOT NULL,
  Email        NVARCHAR(200) NOT NULL UNIQUE,
  Phone        NVARCHAR(50),
  Status       NVARCHAR(30) NOT NULL DEFAULT 'Active',
  CreatedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Accounts (
  AccountId    INT IDENTITY(1,1) PRIMARY KEY,
  UserId       INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
  Currency     CHAR(3) NOT NULL,
  Balance      DECIMAL(18,2) NOT NULL DEFAULT 0,
  Status       NVARCHAR(30) NOT NULL DEFAULT 'Active',
  CreatedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
```

### 7. Bes Adet DML (10p)
```sql
-- 1) Insert hesap
INSERT INTO Accounts (UserId, Currency, Balance) VALUES (1, 'TRY', 10000);

-- 2) Insert islem
INSERT INTO Transactions (FromAccountId, ToAccountId, Amount, Currency, Type, Status, CreatedAt, Description)
VALUES (1, 2, 500, 'TRY', 'Transfer', 'Completed', SYSDATETIME(), 'Kira odemesi');

-- 3) Update bakiye (gonderen)
UPDATE Accounts SET Balance = Balance - 500 WHERE AccountId = 1;

-- 4) Update bakiye (alici)
UPDATE Accounts SET Balance = Balance + 500 WHERE AccountId = 2;

-- 5) Delete bekleyen islem (ornek)
DELETE FROM Transactions WHERE TransactionId = 999 AND Status = 'Pending';
```

### 8. Bes Adet Gelismis SQL Sorgusu (10p)
> Kendi verinize gore uyarlayin, ciktilarini ekran goruntusuyle ekleyin.

1) Kullanici bazinda toplam bakiye (TRY) ve hesap sayisi
```sql
SELECT u.UserId, u.FirstName, u.LastName,
       COUNT(a.AccountId) AS AccountCount,
       SUM(a.Balance) AS TotalBalanceTRY
FROM Users u
JOIN Accounts a ON a.UserId = u.UserId
GROUP BY u.UserId, u.FirstName, u.LastName
HAVING SUM(a.Balance) > 0;
```

2) Son 30 gunde transfer ozeti (duruma gore)
```sql
SELECT t.Status, COUNT(*) AS TxCount, SUM(t.Amount) AS TotalAmount
FROM Transactions t
WHERE t.Type = 'Transfer' AND t.CreatedAt >= DATEADD(DAY, -30, SYSDATETIME())
GROUP BY t.Status;
```

3) Kart prestij seviyesine gore ortalama limit ve kart sayisi
```sql
SELECT c.Prestige, COUNT(*) AS CardCount, AVG(c.CreditLimit) AS AvgLimit
FROM Cards c
GROUP BY c.Prestige
HAVING COUNT(*) > 0;
```

4) Kullanici bazinda KYC durumu ve son islem tarihi
```sql
SELECT u.UserId, u.FirstName, u.LastName,
       kd.Status AS KycStatus,
       MAX(t.CreatedAt) AS LastTransactionDate
FROM Users u
LEFT JOIN KycDocuments kd ON kd.UserId = u.UserId
LEFT JOIN Accounts a ON a.UserId = u.UserId
LEFT JOIN Transactions t ON t.FromAccountId = a.AccountId OR t.ToAccountId = a.AccountId
GROUP BY u.UserId, u.FirstName, u.LastName, kd.Status;
```

5) Son 90 gun kart harcamasi toplami
```sql
SELECT u.UserId, u.FirstName, u.LastName, SUM(ct.Amount) AS TotalCardSpend
FROM Users u
JOIN Cards c ON c.UserId = u.UserId
JOIN CardTransactions ct ON ct.CardId = c.CardId
WHERE ct.CreatedAt >= DATEADD(DAY, -90, SYSDATETIME())
GROUP BY u.UserId, u.FirstName, u.LastName
HAVING SUM(ct.Amount) > 0;
```

### 9. Baglanti ve UI Ornegi (10p)
- DB baglantisi: .NET API?de appsettings.json -> ConnectionStrings:Default ile SQL Server?a baglanilir; Dapper/ADO.NET kullanilir.
- UI: React + MUI dashboard; giris sonrasi hesap kartlari, son islemler, hizli islemler gosterilir. Ornek ekran: docs/ui-dashboard.png (kendi ekran goruntunuzu ekleyin).
- Akis: API (/Accounts, /Transactions) -> frontend servisleri (accountService, transactionService).

### 10. Transaction Aciklamasi ve Ornek (10p)
Transaction, ACID garantisiyle ?ya hep ya hic? calisan islem birimidir. Ornek para transferi:
```sql
BEGIN TRAN
  UPDATE Accounts SET Balance = Balance - 500 WHERE AccountId = 1;
  UPDATE Accounts SET Balance = Balance + 500 WHERE AccountId = 2;
  INSERT INTO Transactions (FromAccountId, ToAccountId, Amount, Currency, Type, Status, CreatedAt)
  VALUES (1, 2, 500, 'TRY', 'Transfer', 'Completed', SYSDATETIME());
COMMIT TRAN;
-- Hata durumunda ROLLBACK TRAN;
```

### 11. View ve Stored Procedure (10p)
```sql
-- View: Kullanici bazinda hesap bakiyesi ozeti
CREATE VIEW vw_UserBalances AS
SELECT u.UserId, u.FirstName, u.LastName,
       COUNT(a.AccountId) AS AccountCount,
       SUM(a.Balance) AS TotalBalance
FROM Users u
LEFT JOIN Accounts a ON a.UserId = u.UserId
GROUP BY u.UserId, u.FirstName, u.LastName;

-- Stored Procedure: Hesap bakiyesi guncelle ve islem kaydi olustur
CREATE PROCEDURE sp_DoTransfer
  @FromAccountId INT,
  @ToAccountId INT,
  @Amount DECIMAL(18,2),
  @Currency CHAR(3)
AS
BEGIN
  SET NOCOUNT ON;
  BEGIN TRAN;
    UPDATE Accounts SET Balance = Balance - @Amount WHERE AccountId = @FromAccountId;
    UPDATE Accounts SET Balance = Balance + @Amount WHERE AccountId = @ToAccountId;
    INSERT INTO Transactions (FromAccountId, ToAccountId, Amount, Currency, Type, Status, CreatedAt)
    VALUES (@FromAccountId, @ToAccountId, @Amount, @Currency, 'Transfer', 'Completed', SYSDATETIME());
  COMMIT TRAN;
END;
```

---
Bu belgeyi kendi proje verilerinize gore guncelleyip; ER diyagrami, ekran goruntuleri ve gercek GitHub baglantilarini ekleyerek teslim ediniz.
