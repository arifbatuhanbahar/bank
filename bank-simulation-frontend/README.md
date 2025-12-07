# Bank Simulation Frontend

Modern React + TypeScript arayüzü, Bank Simulation API ile çalışmak üzere hazırlanmıştır.

## Hızlı Başlangıç

```bash
cd bank-simulation-frontend
npm install
npm run dev
```

Tarayıcı: http://localhost:3000  
API varsayılanı: http://localhost:5161/api

## Teknoloji Yığını
- React 19, TypeScript, Vite 7
- Material UI 7, Tailwind CSS 4 (yardımcı stiller)
- React Router 7, React Query 5
- React Hook Form + Yup, Axios

## Dizin Yapısı
```
src/
  api/          # API istemcisi ve servisler
  components/   # Ortak bileşenler
  features/     # Sayfa/modül bazlı özellikler
  hooks/        # Özel hook'lar
  routes/       # Router tanımı ve korumalar
  styles/       # Tema ve global stiller
  types/        # Tip tanımları
  utils/        # Yardımcı fonksiyonlar
```

## Ortam Değişkenleri
`.env.development` örneği:
```
VITE_API_BASE_URL=http://localhost:5161/api
VITE_APP_NAME=Bank Simulation
VITE_APP_VERSION=1.0.0
```

## npm Scriptleri
- `npm run dev` – Geliştirme sunucusu (3000)
- `npm run build` – Üretim derlemesi
- `npm run preview` – Üretim derlemesini önizleme
- `npm run lint` – ESLint çalıştır

## Mevcut Durum
- Tamamlananlar: Login sayfası, Dashboard, Hesaplar, İşlem Geçmişi, Para Transferi, Kartlar, Kart Başvurusu, KYC/KVKK sayfaları, temel layout (Header/Sidebar), API servisleri.
- Eksikler: Admin ekranları, hesap/kart detay sayfaları, kayıt sayfası.

## Notlar
- Mock login e-posta eşleştirmesiyle çalışır; gerçek kimlik doğrulama için backend auth endpointlerine geçilmesi gerekir.
