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

Ikki kurs bor: "Vizuallashtirish asoslari" va "Boshlang‘ich matematika" (YouTube video + misol va masalalar).
Kirgandan keyin `/kabinet` yoki `/akademiya` sahifasida "Birinchi darsni boshlash" tugmasi chiqadi.
Kurs mazmuni `AcademySeeder` da, progress `tasvir-academy.db` da saqlanadi.
Tuzilgan sxemalar `/akademiya/ishlarim` sahifasidan PNG va SVG ko‘rinishida yuklab olinadi.

## Studiya

`/studiya`: shablondan material yaratish, saqlash, PNG/SVG/PDF eksport va darsda bosqichma-bosqich ko‘rsatish
(strelkalar yoki bo‘sh joy, F bilan to‘liq ekran). Materiallar `tasvir-studio.db` da saqlanadi.

## Guruhlar

`/guruhlar`: guruh yaratgan kishi uning o‘qituvchisi, kod bilan qo‘shilgan kishi talabasi. Talaba Studiyadagi materialini topshiradi,
o‘qituvchi uni dissertatsiyadagi 2.1.2-jadval mezonlari (8 mezon, 0–3 ball) bo‘yicha baholaydi. O‘qituvchi panelida progress, test savollari
tahlili (qiyinlik va farqlash indeksi) va so‘rovnoma natijalari bor, ular Excel uchun CSV ko‘rinishida yuklab olinadi.
Ma’lumotlar `tasvir-classroom.db` da saqlanadi.

Maxfiylik siyosati `/maxfiylik` sahifasida: rozilik ro‘yxatdan o‘tishda, natijalarni o‘qituvchiga ko‘rsatish roziligi guruhga qo‘shilishda so‘raladi.
Sahifadagi aloqa ma’lumotlari va huquqiy matnni ishga tushirishdan oldin to‘ldirib, yurist bilan tekshirish kerak.

## Yordamchi

Har sahifadagi "Savol bering" tugmasi. Oddiy savollarga `KnowledgeBase.cs` asosida javob beradi, tashqi xizmat kerak emas.

## To‘lov (Payme va Click)

`/tariflar` — tariflar, `/tolov?tarif=...` — to‘lov tizimini tanlash, `/tolov/natija` — natija, kabinetda — obuna holati.
Buyurtmalar `tasvir-payments.db` da saqlanadi. Obuna alohida yozilmaydi: to‘langan buyurtmalardan hisoblanadi
(yangi to‘lov muddatni amaldagi obuna oxiridan uzaytiradi, qaytarilgan to‘lov hisobdan chiqadi).

Kod tayyor, ishga tushirish uchun faqat **narx** va **kalitlar** kerak. Kalitsiz tizim saytda "Hali ulanmagan" bo‘lib ko‘rinadi,
narxi yo‘q tarif "Tez orada" bo‘lib turadi.

### 1. Narxlar

`backend/src/Hosts/Tasvir.Api/appsettings.json` → `Payments:Plans`: har tarifga `Price` (so‘mda) yozing.
Tarif qo‘shish yoki olib tashlash ham shu yerda (`Key`, `Name`, `Months`, `Features`).

### 2. Payme (merchant.paycom.uz)

1. Kassa yarating, "Merchant API" turini tanlang.
2. Kassa sozlamalarida **endpoint URL**: `https://<domen>/api/payments/payme`
3. Kassadan oling: **Kassa ID** → `Payments:Payme:MerchantId`, **kalit** → `Payments:Payme:Key`.
   Sinov paytida test kaliti va `CheckoutUrl = https://checkout.test.paycom.uz`, ishga tushganda asosiy kalit va
   `https://checkout.paycom.uz`.
4. Hisob maydoni (account) nomi: `order_id`.
5. Payme sandbox (test.paycom.uz) dagi barcha ssenariylardan o‘ting — keyin kassa faollashtiriladi.

### 3. Click (merchant.click.uz)

1. Servis yarating (SHOP API).
2. **Prepare URL**: `https://<domen>/api/payments/click/prepare`, **Complete URL**: `https://<domen>/api/payments/click/complete`
3. Kabinetdan oling: **Service ID** → `Payments:Click:ServiceId`, **Merchant ID** → `Payments:Click:MerchantId`,
   **Secret key** → `Payments:Click:SecretKey`.

### Kalitlarni qo‘yish

Kalitlar **repozitoriyga yozilmaydi**. Lokal kompyuterda:

```bash
cd backend/src/Hosts/Tasvir.Api
dotnet user-secrets init
dotnet user-secrets set "Payments:Payme:MerchantId" "<kassa-id>"
dotnet user-secrets set "Payments:Payme:Key" "<kalit>"
dotnet user-secrets set "Payments:Click:ServiceId" "<service-id>"
dotnet user-secrets set "Payments:Click:MerchantId" "<merchant-id>"
dotnet user-secrets set "Payments:Click:SecretKey" "<secret-key>"
```

Serverda muhit o‘zgaruvchilari orqali (`:` o‘rniga `__`):

```
Payments__Payme__MerchantId=...
Payments__Payme__Key=...
Payments__Payme__CheckoutUrl=https://checkout.paycom.uz
Payments__Click__ServiceId=...
Payments__Click__MerchantId=...
Payments__Click__SecretKey=...
Payments__FrontendUrl=https://<domen>
```

Qo‘shimcha himoya: Payme va Click o‘z serverlari IP manzillarini beradi — ularni `Payments:Payme:AllowedIps` va
`Payments:Click:AllowedIps` ga yozsangiz, boshqa manzildan kelgan so‘rovlar rad etiladi (imzo/kalit baribir tekshiriladi).
Server proksi orqasida bo‘lsa, `KnownProxies` ni sozlang, aks holda haqiqiy IP ko‘rinmaydi.

Callback manzillari Gateway orqali ochiq bo‘lishi va HTTPS da ishlashi kerak. Kalit almashtirilgach API qayta ishga tushiriladi.

## Serverga joylashtirish

Bepul variant (Neon + Render + Vercel + Brevo) bosqichma-bosqich: [docs/deploy.md](docs/deploy.md).

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
        Studio/                                o‘qituvchi materiallari
        Classroom/                             guruhlar, baholash, diagnostika
        Payments/                              tariflar, buyurtmalar, Payme va Click
    tests/                                     integratsion testlar
  frontend/                                    Next.js ilova
  docs/                                        arxitektura va dizayn hujjatlari
```

## Ma’lumotlar haqida

Narxlar, natija raqamlari, sharhlar va hamkorlar hozircha **namuna joy** (`—`, `[Ism Familiya]`).
Ular bazada saqlanadi va real ma’lumot kelganda o‘zgartiriladi — o‘ylab topilgan raqam qo‘yilmaydi.
