# Bepul serverga joylashtirish

| Qism | Xizmat | Bepul tarif |
|---|---|---|
| Baza (PostgreSQL) | [Neon](https://neon.tech) | 0,5 GB, karta kerak emas |
| API (.NET, barcha modullar) | [Render](https://render.com) | 512 MB RAM; 15 daqiqa so‘rov bo‘lmasa uxlaydi |
| Sayt (Next.js) | [Vercel](https://vercel.com) | Hobby tarif |
| Xat yuborish | [Brevo](https://www.brevo.com) | kuniga 300 ta xat |

Gateway serverda kerak emas: sayt to‘g‘ridan-to‘g‘ri API ga murojaat qiladi.

Hammasi GitHub repozitoriyidan quriladi. `main` ga yangi commit tushsa, Render va Vercel o‘zi qayta joylaydi.

## 1. Neon — baza

1. neon.tech → GitHub bilan kiring → **New project** (region: *AWS Europe Central (Frankfurt)*).
2. **Connect** oynasida **Connection pooling** ni o‘chiring va `postgresql://...` havolani nusxalang.
   Bu havolada parol bor: uni hech kimga yubormang va repozitoriyga yozmang.

API birinchi ishga tushganda shu serverda har modul uchun alohida baza yaratadi: `tasvir_content`, `tasvir_identity`,
`tasvir_academy`, `tasvir_studio`, `tasvir_classroom`, `tasvir_payments`.

## 2. Brevo — xat yuborish

Render bepul tarifida SMTP portlari (25, 465, 587) yopiq, shuning uchun Gmail SMTP ishlamaydi. Xat Brevo API (HTTPS) orqali ketadi.

1. brevo.com → ro‘yxatdan o‘ting.
2. **Senders, domains & dedicated IPs → Senders → Add a sender**: jo‘natuvchi sifatida Gmail manzilingizni qo‘shing va
   unga kelgan xat orqali tasdiqlang.
3. **SMTP & API → API keys → Generate a new API key**: kalitni nusxalang.

## 3. Render — API

1. render.com → GitHub bilan kiring → **New → Blueprint** → `tasvir.uz` repozitoriyini tanlang.
   Render ildizdagi `render.yaml` ni o‘qiydi va `tasvir-api` servisini taklif qiladi.
2. So‘ralgan qiymatlarni kiriting:

   | O‘zgaruvchi | Qiymat |
   |---|---|
   | `Database__PostgresUrl` | Neon havolasi (1-qadam) |
   | `Identity__FrontendUrl` | Vercel manzili, masalan `https://tasvir-uz.vercel.app` (4-qadamdan keyin aniqlanadi; avval vaqtincha yozib, keyin almashtirsa bo‘ladi) |
   | `Payments__FrontendUrl` | xuddi shu manzil |
   | `Identity__Brevo__ApiKey` | Brevo API kaliti |
   | `Identity__Smtp__FromAddress` | Brevo da tasdiqlangan Gmail manzil |

   `Identity__Jwt__SigningKey` ni Render o‘zi yaratadi.
3. **Apply**. Birinchi qurish 5–10 daqiqa oladi. Tayyor bo‘lgach, `https://tasvir-api.onrender.com/health` → `Healthy`.
   Manzil boshqacha bo‘lsa (masalan, nom band bo‘lgan bo‘lsa), Render ko‘rsatgan manzilni oling.

## 4. Vercel — sayt

1. vercel.com → GitHub bilan kiring → **Add New → Project** → `tasvir.uz` repozitoriyi → **Import**.
2. **Root Directory**: `frontend`. Framework avtomatik aniqlanadi (Next.js).
3. **Environment Variables**: `API_URL` = Render manzili, masalan `https://tasvir-api.onrender.com`.
4. **Deploy**. Tayyor bo‘lgach, sayt manzilini Render dagi `Identity__FrontendUrl` va `Payments__FrontendUrl` ga yozing.

## 5. Uxlab qolmasligi uchun (tavsiya)

Render bepul servisi 15 daqiqa so‘rov bo‘lmasa uxlaydi, keyingi birinchi so‘rov ~1 daqiqa kutadi. Payme/Click to‘lov
tasdig‘ini kutib turmaydi, shuning uchun to‘lov ulangach servis uyg‘oq turishi kerak.

[cron-job.org](https://cron-job.org) da bepul vazifa yarating: har 10 daqiqada `https://tasvir-api.onrender.com/health` ni ochsin.
Render bepul tarifida oyiga 750 soat beriladi, bitta servis kecha-kunduz ishlashi uchun bu yetadi.

## 6. To‘lov kalitlari (keyinroq)

Render → `tasvir-api` → **Environment** ga qo‘shing (README dagi "To‘lov" bo‘limi):

```
Payments__Plans__0__Price=...
Payments__Payme__MerchantId=...
Payments__Payme__Key=...
Payments__Payme__CheckoutUrl=https://checkout.paycom.uz
Payments__Click__ServiceId=...
Payments__Click__MerchantId=...
Payments__Click__SecretKey=...
```

Callback manzillari: `https://tasvir-api.onrender.com/api/payments/payme`, `.../api/payments/click/prepare`,
`.../api/payments/click/complete`.

## Cheklovlar

- **Vercel Hobby** tarifi tijoriy foydalanish uchun emas. Pullik obuna sotila boshlagach, Vercel Pro ga yoki boshqa xostingga o‘tish kerak.
- **Neon** bepul tarifida baza 5 daqiqa ishlatilmasa to‘xtaydi va keyingi so‘rovda bir soniyada uyg‘onadi.
- Lokal kompyuterdagi SQLite ma’lumotlari serverga ko‘chmaydi: server toza bazadan boshlanadi (darslar, shablonlar va
  landing matnlari avtomatik yoziladi), hisoblarni qaytadan ochish kerak.
