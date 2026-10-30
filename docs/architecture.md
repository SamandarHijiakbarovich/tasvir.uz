# Tasvir.uz — arxitektura

## Umumiy ko‘rinish

```
                 ┌──────────────────────┐
  Brauzer ──────▶│  Frontend (Next.js)  │  :3000
                 └──────────┬───────────┘
                            │ API_URL
                 ┌──────────▼───────────┐
                 │  API Gateway (YARP)  │  :5000   ← yagona kirish nuqtasi, CORS
                 └───┬──────────────┬───┘
/api/content, /api/identity│     │ /api/<yangi>/*  (kelajakda)
          ┌──────────▼─────────┐  ┌─▼──────────────────────────────┐
          │  Tasvir.Api (C#)   │  │  Boshqa servis (Python, Go, …) │
          │  modulli monolit   │  │  masalan: AI infografika       │
          │  ├─ Content        │  └────────────────────────────────┘
          │  ├─ Identity       │
          │  ├─ (Academy)      │
          │  └─ (Studio)       │  :5100
          └─────────┬──────────┘
                    │ har modul o‘z sxemasi / DbContext
              ┌─────▼─────┐
              │  Baza     │  SQLite (lokal) / PostgreSQL (server)
              └───────────┘
```

## Nega modulli monolit (mikroservis emas)

- Startup boshida bitta jamoa, bitta deploy — mikroservislar tarmoq, kuzatuv, ma’lumot sinxronizatsiyasi kabi
  ortiqcha murakkablik qo‘shadi.
- Har bir modul **o‘z chegarasida**: o‘z Domain / Application / Infrastructure / Presentation loyihalari,
  o‘z `DbContext`i va baza sxemasi. Modullar bir-birining jadvallariga murojaat qilmaydi.
- Shu sababli istalgan modul keyinchalik **alohida mikroservisga** ajratiladi: loyihalari yangi hostga ko‘chiriladi
  va Gateway’dagi marshrut manzili o‘zgartiriladi. Frontend hech narsani sezmaydi.

## Clean Architecture (har bir modul ichida)

```
Presentation  →  Application  →  Domain
                      ↑
               Infrastructure
```

| Qatlam | Vazifasi | Nimaga bog‘liq |
|---|---|---|
| **Domain** | Biznes obyektlari va qoidalari (`PricingPlan`, `FaqItem`…) | faqat `SharedKernel` |
| **Application** | Use case’lar (`GetLandingContentHandler`), DTO’lar, portlar (`ILandingContentRepository`) | Domain |
| **Infrastructure** | EF Core, baza, repository amalga oshirilishi, seed | Application |
| **Presentation** | HTTP endpointlar (`/api/content/*`) | Application |
| **Host** (`Tasvir.Api`) | Hamma modulni yig‘adi (composition root) | barcha modullar |

Bog‘liqlik faqat ichkariga qarab yo‘naladi: Domain hech narsani bilmaydi, baza texnologiyasi almashsa faqat
Infrastructure o‘zgaradi.

## Boshqa texnologiyadagi servisni qo‘shish

1. Servisni istalgan tilda yozing va o‘z portida ishga tushiring (masalan, Python FastAPI `:8000`).
2. `backend/src/Hosts/Tasvir.Gateway/appsettings.json` ga marshrut va klaster qo‘shing:

```json
"Routes": {
  "ai": { "ClusterId": "ai-service", "Match": { "Path": "/api/ai/{**catch-all}" } }
},
"Clusters": {
  "ai-service": { "Destinations": { "primary": { "Address": "http://localhost:8000/" } } }
}
```

3. Frontend `API_URL` orqali `/api/ai/...` ga murojaat qiladi.

Servislar o‘rtasidagi aloqa uchun kelajakda xabar navbati (RabbitMQ) qo‘shiladi — sinxron chaqiruvlar o‘rniga
hodisalar (masalan, `CourseCompleted` → sertifikat yaratish).

## Yangi C# modul qo‘shish

1. `backend/src/Modules/<Nom>/` ichida 4 ta loyiha: `Domain`, `Application`, `Infrastructure`, `Presentation`
   (Content modulini namuna sifatida oling).
2. `Tasvir.Api.csproj` ga havolalarni va `Program.cs` ga `Add<Nom>Application()`, `Add<Nom>Infrastructure()`,
   `Map<Nom>Endpoints()` ni qo‘shing.
3. Gateway’ga `/api/<nom>/*` marshrutini qo‘shing.
4. `Tasvir.slnx` ga loyihalarni qo‘shing.

## Autentifikatsiya (Identity moduli)

```
Brauzer ── httpOnly cookie ──▶ Next.js (server action, proxy) ── JSON + Bearer ──▶ Gateway ──▶ Identity moduli
```

- **Tokenlar brauzer JavaScript’iga chiqmaydi.** Access (JWT, 15 daqiqa) va refresh (tasodifiy 256 bit) tokenlar
  `httpOnly`, `SameSite=Lax`, ishlab chiqarishda `Secure` cookie’larda. `tasvir_session` cookie’si maxfiy emas,
  faqat navbar uchun.
- **Sezdirmasdan yangilash.** `src/proxy.ts` access cookie tugaganda refresh tokenni almashtiradi. Formalar CSRF’dan
  Next.js server action’larining Origin tekshiruvi va `SameSite=Lax` bilan himoyalangan.
- **Refresh rotation.** Har yangilashda token almashadi, bazada faqat SHA-256 xeshi saqlanadi. Eski token 30 soniyadan
  keyin qayta ishlatilsa, butun sessiya ("oila") yopiladi. Yangilash sessiya muddatini uzaytirmaydi.
- **Parollar** ASP.NET Core `PasswordHasher` (PBKDF2) bilan xeshlanadi: 8–128 belgi, keng tarqalgan parollar rad etiladi.
- **Terib ko‘rishga qarshi:** 5 ta xato urinishdan so‘ng hisob 15 daqiqaga qulflanadi; kirish, ro‘yxatdan o‘tish, parolni
  tiklash va havola marshrutlarida IP bo‘yicha daqiqasiga 20 ta so‘rov.
- **Email havolalari** bir martalik, xeshlangan va muddatli (tasdiqlash 48 soat, parol tiklash 60 daqiqa). Tasdiqlash
  tugma bilan bajariladi, pochta skanerlari havolani ochib qo‘ymasligi uchun.

### Ishlab chiqarishga chiqishdan oldin

| Sozlama | Izoh |
|---|---|
| `Identity__Jwt__SigningKey` | Kamida 32 bayt. Yo‘q bo‘lsa, ilova ishga tushmaydi |
| `Identity__Smtp__Host`, `__Port`, `__Username`, `__Password`, `__FromAddress` | Yo‘q bo‘lsa, ilova ishga tushmaydi. TLS majburiy |
| `Identity__FrontendUrl` | Emaildagi havolalar manzili |
| `ConnectionStrings__Identity`, `Database__Provider=Postgres` | Modulning alohida bazasi |
| `KnownProxies__0`, `KnownProxies__1` | Next.js va Gateway IP manzillari. Yo‘q bo‘lsa, barcha foydalanuvchilar bitta IP limitini bo‘lishadi |

Qolgan ishlar: EF Core migratsiyalari, muddati o‘tgan tokenlarni davriy tozalash, Google/SMS orqali kirish.
Boshqa tildagi servislar JWT’ni tekshirishi kerak bo‘lganda HS256 dan RS256 ga o‘tiladi, shunda servislarga maxfiy kalit
berilmaydi. Lokal ishlab chiqishda kalit har ishga tushganda yangilanadi: API qayta ishga tushsa, qayta kirish kerak bo‘ladi.

## Frontend tuzilmasi (feature-based)

```
frontend/src/
  app/                  Next.js marshrutlari (yupqa: faqat feature’ni chaqiradi)
  features/
    landing/
      api/              API chaqiruvlari va turlar (backend DTO bilan mos)
      components/       Bo‘limlar (Hero, Pricing, Faq…)
      LandingPage.tsx   Sahifa yig‘uvchisi
    auth/
      actions.ts        Server action’lar (ro‘yxatdan o‘tish, kirish, parol…)
      lib/              Identity API klienti, sessiya cookie’lari
      components/       Formalar, AuthShell
    (academy/, studio/ — keyingi bosqichlar)
  shared/
    config/             Muhit sozlamalari
    lib/                HTTP klient, umumiy hook’lar
    ui/                 Umumiy komponentlar (Logo, ikonkalar)
  styles/               Dizayn tokenlari va komponent stillari (design handoff)
```

Qoidalar: `features/*` bir-birini import qilmaydi, umumiy narsa `shared/` ga chiqariladi.
Serverda render qilinadi (SEO), interaktiv qismlar (`"use client"`) kichik komponentlarga ajratilgan.

## Keyingi qadamlar

- **Academy moduli**: kurslar, darslar, topshiriqlar, progress, sertifikat.
- **Studio moduli**: shablonlar va foydalanuvchi ishlari; shablonlar galereyasi API dan keladi.
- EF Core migratsiyalari (hozir `EnsureCreated`), PostgreSQL, Docker va CI/CD.
- Testlar: Identity uchun integration testlar bor (`backend/tests`); Content moduli va frontend uchun testlar qo‘shish.
