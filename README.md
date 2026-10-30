# Tasvir.uz

**Murakkabni oddiy, zerikarlini qiziqarli qiling.**

O‘qituvchilarga o‘quv materialini vizual shaklga keltirishni o‘rgatadigan va buni oson qiladigan platforma.

| Qism | Texnologiya | Port |
|---|---|---|
| Frontend | Next.js 16, React 19, TypeScript | 3000 |
| API Gateway | ASP.NET Core 10 + YARP | 5000 |
| API (modulli monolit) | ASP.NET Core 10, EF Core, Clean Architecture | 5100 |

Arxitektura haqida batafsil: [docs/architecture.md](docs/architecture.md).
Dizayn qoidalari: [docs/design-handoff/BRAND.md](docs/design-handoff/BRAND.md).

## Talablar

- .NET 10 SDK
- Node.js 22+ (LTS)

## Ishga tushirish

Uchta terminal oching.

**1. API**

```bash
dotnet run --project backend/src/Hosts/Tasvir.Api
```

Birinchi ishga tushishda `tasvir-content.db` (SQLite) yaratiladi va boshlang‘ich ma’lumotlar yoziladi.
OpenAPI sxemasi: http://localhost:5100/openapi/v1.json

**2. Gateway**

```bash
dotnet run --project backend/src/Hosts/Tasvir.Gateway
```

Tekshirish: http://localhost:5000/api/content/landing

**3. Frontend**

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```

Sayt: http://localhost:3000

## Ro‘yxatdan o‘tish va kirish

Sahifalar: `/royxatdan-otish`, `/kirish`, `/parolni-tiklash`, `/kabinet`.

Lokal ishlab chiqishda xatlar yuborilmaydi: tasdiqlash va parol tiklash havolalari API terminaliga
`[DEV EMAIL]` belgisi bilan chiqadi. Foydalanuvchilar `tasvir-identity.db` faylida saqlanadi.
Ishlab chiqarish sozlamalari: [docs/architecture.md](docs/architecture.md#autentifikatsiya-identity-moduli).

## Akademiya

Kirgandan keyin `/kabinet` yoki `/akademiya` sahifasida "Birinchi darsni boshlash" tugmasi chiqadi.
Kurs mazmuni `AcademySeeder` da, progress `tasvir-academy.db` da saqlanadi.
Tuzilgan sxemalar `/akademiya/ishlarim` sahifasidan PNG va SVG ko‘rinishida yuklab olinadi.

## Yordamchi

Har sahifadagi "Savol bering" tugmasi. Oddiy savollarga `KnowledgeBase.cs` asosida javob beradi, tashqi xizmat kerak emas.

## Testlar

```bash
dotnet test backend/Tasvir.slnx
```

## Loyiha tuzilmasi

```
tasvir-uz/
  backend/
    Tasvir.slnx
    src/
      BuildingBlocks/Tasvir.SharedKernel/      umumiy bazaviy sinflar
      Hosts/
        Tasvir.Api/                            modullarni yig‘uvchi host
        Tasvir.Gateway/                        YARP API Gateway
      Modules/
        Content/                               landing kontenti moduli
          Tasvir.Modules.Content.Domain/
          Tasvir.Modules.Content.Application/
          Tasvir.Modules.Content.Infrastructure/
          Tasvir.Modules.Content.Presentation/
        Identity/                              ro‘yxatdan o‘tish, kirish, parol, email
        Academy/                               kurslar, darslar, test, topshiriq, progress
        Assistant/                             oddiy savollarga javob beruvchi yordamchi
    tests/                                     integratsion testlar
  frontend/                                    Next.js ilova
  docs/                                        arxitektura va dizayn hujjatlari
```

## Ma’lumotlar haqida

Narxlar, natija raqamlari, sharhlar va hamkorlar hozircha **namuna joy** (`—`, `[Ism Familiya]`).
Ular bazada saqlanadi va real ma’lumot kelganda o‘zgartiriladi — o‘ylab topilgan raqam qo‘yilmaydi.
