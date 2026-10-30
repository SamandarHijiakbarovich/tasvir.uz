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
       /api/content/*│              │ /api/<yangi>/*  (kelajakda)
          ┌──────────▼─────────┐  ┌─▼──────────────────────────────┐
          │  Tasvir.Api (C#)   │  │  Boshqa servis (Python, Go, …) │
          │  modulli monolit   │  │  masalan: AI infografika       │
          │  ├─ Content        │  └────────────────────────────────┘
          │  ├─ (Identity)     │
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

## Frontend tuzilmasi (feature-based)

```
frontend/src/
  app/                  Next.js marshrutlari (yupqa: faqat feature’ni chaqiradi)
  features/
    landing/
      api/              API chaqiruvlari va turlar (backend DTO bilan mos)
      components/       Bo‘limlar (Hero, Pricing, Faq…)
      LandingPage.tsx   Sahifa yig‘uvchisi
    (academy/, studio/, auth/ — keyingi bosqichlar)
  shared/
    config/             Muhit sozlamalari
    lib/                HTTP klient, umumiy hook’lar
    ui/                 Umumiy komponentlar (Logo, ikonkalar)
  styles/               Dizayn tokenlari va komponent stillari (design handoff)
```

Qoidalar: `features/*` bir-birini import qilmaydi, umumiy narsa `shared/` ga chiqariladi.
Serverda render qilinadi (SEO), interaktiv qismlar (`"use client"`) kichik komponentlarga ajratilgan.

## Keyingi qadamlar

- **Identity moduli**: ro‘yxatdan o‘tish va kirish (JWT), "Bepul boshlash" tugmalarini ulash.
- **Academy moduli**: kurslar, darslar, topshiriqlar, progress, sertifikat.
- **Studio moduli**: shablonlar va foydalanuvchi ishlari; shablonlar galereyasi API dan keladi.
- EF Core migratsiyalari (hozir `EnsureCreated`), PostgreSQL, Docker va CI/CD.
- Testlar: Domain va Application uchun unit, endpointlar uchun integration testlar.
