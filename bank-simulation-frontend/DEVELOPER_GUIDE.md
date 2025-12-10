## Bank Simulation - Developer Guide

Bu doküman, projeyi hızlıca ayağa kaldırmak ve geliştirmek isteyenler için işlevsel rehberdir.

### Hızlı Başlangıç
1) Backend:  
   - Komut: `cd ../BankSimulation && dotnet run`  
   - Varsayılan URL: `http://localhost:5161` (Swagger: `/swagger`).
2) Frontend:  
   - Komut: `npm install` ardından `npm run dev` (PowerShell policy engeli varsa `cmd /c npm run dev`).  
   - ENV: `.env.development` içinde `VITE_API_BASE_URL=http://localhost:5161/api`.
3) Build: `npm run build` (tsc + vite) / `cd ../BankSimulation && dotnet build`.

### Gereksinimler
- .NET 9 SDK
- Node.js 22+ ve npm
- SQL Server (lokalde `.\SQLEXPRESS` varsayımıyla çalışır; farklı instance kullanıyorsanız connection string’i güncelleyin)
- PowerShell policy engeli varsa npm komutlarını `cmd /c ...` ile çağırın.

### Veritabanı Kurulumu
1) Connection string: `../BankSimulation/src/BankSimulation.API/appsettings.json`  
   ```
   "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BankSimulationDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```
   İhtiyaca göre sunucu/kimlik bilgilerini güncelleyin.

2) Şemayı oluşturma (SQL script):  
   - Komut: `sqlcmd -S .\SQLEXPRESS -i BankSimulationDb.sql` (komutu `BankSimulation` dizininde çalıştırın).  
   - Bu script DB’yi drop/create eder, tüm tabloları oluşturur.

3) Veri doldurma seçenekleri:  
   - **Script içi seed yok**, ama API’de DataSeeder var.  
   - API’yi çalıştırdıktan sonra `http://localhost:5161/api/Seeder/seed-all` endpoint’ini çağırın (Swagger’dan veya curl: `curl -X POST http://localhost:5161/api/Seeder/seed-all`). Bu 100+ demo kullanıcısı, hesaplar, işlemler vb. ekler.

### Çalıştırma Adımları (Tam Akış)
1) Veritabanını oluşturun (yukarıdaki sqlcmd veya kendi SSMS ile script çalıştırma).  
2) Backend’i başlatın: `cd ../BankSimulation && dotnet run` (Swagger: `http://localhost:5161/swagger`).  
3) Frontend’i başlatın: `cd bank-simulation-frontend && npm install && npm run dev` (veya `cmd /c npm run dev`).  
4) Giriş: herhangi bir demo kullanıcı e-postası ve geçici parola `demo123` (AuthController’da tanımlı). Admin menüsü için `admin@bank.com` veya `root@bank.com`.  
5) Prod build denemesi: `npm run build`.

### Mimari & Dizinyapısı
- Frontend: React + Vite + TS + MUI. Modüller `src/features/*`, ortak bileşenler `src/components/*`, tipler `src/types/*`, servisler `src/api/*`.
- Backend: .NET 9 Web API, Dapper/SQL Server. Şema: `../BankSimulation/BankSimulationDb.sql`. Seeder: `DataSeeder` (100+ kullanıcı, hesap, işlem vb.).
- Router: `src/routes/index.tsx` (protected + admin rotaları). Layout: `src/components/layout/*`.

### Kimlik Doğrulama & Yetki
- Admin listesi: `src/utils/constants.ts` → `ADMIN_EMAILS` (`admin@bank.com`, `root@bank.com`). Admin menüsü bu listeyi baz alır.
- Geçici demo şifre: **`demo123`** tüm kullanıcılar için kabul ediliyor (dosya: `../BankSimulation/src/BankSimulation.API/Controllers/AuthController.cs`). Canlıya çıkarken kaldırın.
- Login flow: `userService.login` sonucunu camelCase’e normalize edip localStorage’a `authToken` + `user` yazar. Yetki kontrolü: `src/utils/auth.ts` → `isAdminUser`, `PrivateRoute`.

### Önemli Ekranlar / Rotalar
- Kullanıcı: dashboard, hesap listesi/detayı, kart listesi/detayı, transfer, KYC (`/compliance/kyc`), KVKK (`/compliance/kvkk`).
- Admin: `/admin/users`, `/admin/fraud`, `/admin/audit`, `/admin/system`.
- Şifre işlemleri: `/profile/security` (parola değiştirme), `/forgot-password` (yalnızca demo bilgilendirme; gerçek reset servisi yok).

### API & Servisler
- Endpoints: `src/api/endpoints.ts`
- Client: `src/api/client.ts` (axios interceptor’ları token’ı header’a ekler; 401’de localStorage temizlenir ve login’e yönlenir).
- Servis örnekleri: `src/api/services/*` (hesap, kart, işlem, kullanıcı, compliance, admin modülleri).

### Veri, Seed ve Test
- Seeder çalışma mantığı: `DataSeeder` otomatik 100+ kullanıcı, hesap, işlem üretir; şifreler salted SHA256 + `Test123!` tabanlı.
- Örnek sorgu ve şema: `../BankSimulation/BankSimulationDb.sql`.
- Smoke test akışı:  
  1) Backend’i başlat, Swagger’dan `/Auth/login` dene.  
  2) Frontend `npm run dev` ile aç, normal kullanıcıyla giriş (admin menüsü görünmemeli).  
  3) `admin@bank.com / demo123` ile giriş (admin menüsü görünmeli).  
  4) Hesap listesi → hesap detayı → kart detayı → transfer sayfalarını gez.

### Bilinen Sınırlamalar / Yapılacaklar
- Şifre sıfırlama için gerçek e-posta/endpoint yok; `/forgot-password` sadece demo mesajı gösterir.
- Demo şifresi güvenlik riski; yalnız geliştirme/demo ortamında kullanın, kaldırın veya sadece admin’e sınırlandırın.
- Büyük bundle uyarısı (vite build) mevcut; ihtiyaç olursa manuel chunking/dynamic import uygulanabilir.

### Sorun Giderme
- PowerShell policy hatası: `cmd /c npm run dev`.
- Admin menüsü herkes için görünüyorsa: localStorage’daki `user` admin email’i içeriyor mu kontrol edin; değilse çıkış yapıp tekrar giriş yapın (MainLayout her render’da localStorage okur).
- API 401/403: token süresi dolduysa localStorage temizlenir, login’e yönlenir; yeniden giriş yapın.

### Referans Dosyalar
- Admin listesi: `src/utils/constants.ts`
- Auth helper: `src/utils/auth.ts`
- Router: `src/routes/index.tsx`
- Layout: `src/components/layout/*`
- Tipler: `src/types/*`
- Servisler: `src/api/services/*`
