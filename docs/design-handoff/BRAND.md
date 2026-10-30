# Tasvir.uz

**Murakkabni oddiy, zerikarlini qiziqarli qiling.**

Tasvir.uz — o‘qituvchilar va bo‘lajak o‘qituvchilarga o‘quv materialini vizual shaklga (infografika, sxema, diagramma, aqliy xarita, interaktiv animatsiya) keltirishni o‘rgatadigan va buni oson qiladigan platforma. Ikki qism: **Akademiya** (kurslar, amaliy topshiriqlar, sertifikat) va **Studiya** (tayyor shablonlar asosida tezkor yaratish). Asosiy ishonch omili — pedagogika fanlari bo‘yicha dissertatsiya tadqiqoti va tajriba-sinovdan o‘tgan metodika.

Ushbu tizim brifdan tuzilgan (hali kod bazasi yo‘q). Asosiy namuna — **LandingPage** komponenti: to‘liq bosh sahifa.

## Ovoz va matn

- **Til:** o‘zbek, lotin yozuvi. To‘g‘ri belgilar: **o‘, g‘** — U+2018 (‘), tutuq belgisi — U+02BC (ʼ) yoki ’. Hech qachon oddiy `'` yoki `` ` `` emas.
- **Ohang:** iliq, ilhomlantiruvchi, lekin professional. O‘qituvchiga “siz” deb murojaat. Qisqa gaplar, fe’l bilan boshlanuvchi tugmalar: “Bepul boshlash”, “Demo ko‘rish”, “Tadqiqot bilan tanishish”.
- **Shiorlar:** “Murakkabni oddiy, zerikarlini qiziqarli qiling” (asosiy); “Darsingizni ko‘rinadigan qiling”; “Har bir o‘qituvchi vizual dars yarata oladi”.
- **Halollik:** raqamlar, narxlar, sharhlar, hamkorlar faqat haqiqiy bo‘ladi. Ma’lumot yo‘q joyda aniq namuna belgisi: `—%`, `— so‘m`, `[Ism Familiya]`, “Hamkor logotipi”.
- Emoji ishlatilmaydi; o‘rniga chiziqli ikonkalar va stikerlar.

## Vizual asoslar

**Ranglar.** `brand` (chuqur binafsha-ko‘k, #3d35c6) — ishonch: asosiy tugma, havola, fokus, logotip. Ijodkorlik urg‘ulari: `coral`, `sun`, `mint` — stikerlar, diagramma bo‘laklari, qo‘lda chizilgan bezaklar. Urg‘u ranglari ustida matn doim #17162b. Ular rangli matn sifatida kerak bo‘lsa — `coral-ink`, `mint-ink` (4.5:1). Fon: `paper` (daftar qog‘ozi kabi iliq oq), muqobil bo‘limlar `paper-2`, kartochkalar `card`. Qorong‘i bloklar (yakuniy CTA, footer) — `night`. Ikkala rejimda barcha matn juftliklari ≥ 4.5:1.

**Tipografiya.** Sarlavhalar — **Bricolage Grotesque** 800 (xarakterli, zamonaviy display). Matn — **Onest** (o‘qilishi oson sans). Qo‘l yozuvi izohlari — **Caveat** (faqat bezak). Uchalasi ham Google Fonts’da, lotin to‘plamida ‘ va ʼ bor. Hero shiori 64px (telefonda 40px), bo‘lim sarlavhasi 44px (32px).

**Shakllar.** Yumshoq radiuslar: tugma `radius-md` 14px, kartochka `radius-lg` 22px, katta bloklar `radius-xl` 32px, stiker/chip `radius-pill`. Imzo detali — **stiker soyasi** (`shadow-sticker`: 4px qattiq siljigan soya + 2px qalin kontur): asosiy tugma, ajratilgan kartochka, slayder, hero vizuali. Oddiy kartochkalar yumshoq `shadow-card` bilan.

**Bezaklar (doska va daftar ruhi).** Sariq marker chizig‘i (`.tv-mark`), so‘z atrofida qo‘lda chizilgan doira (`.tv-circled`), egri strelkalar (`.tv-arrow`), qiyshiq stikerlar (`.tv-sticker`), Caveat izohlari (`.tv-hand`). Bitta blokda bitta-ikkita — bezak mazmunni bosib ketmasin. Gradient fonlar, emoji kartochkalar, chap rangli chegara ishlatilmaydi.

**Oraliqlar.** 4px asosli shkala: `space-1`…`space-24`. Bo‘limlar orasi 96px (telefonda 64px), kartochka ichi 24–32px, telefonda yon chegara 16px.

**Animatsiya.** Yengil: scroll paytida bloklar 24px pastdan silliq chiqadi (0.7s), raqamlar sanab chiqadi, hoverda kartochka 4px ko‘tariladi, tugma stikerdek siljiydi. Hero — matn infografikaga aylanuvchi 9 soniyali halqa. `prefers-reduced-motion` da hammasi o‘chadi va yakuniy holat ko‘rsatiladi.

## Ikonkalar

24×24 setkada chiziqli ikonkalar, 2px qalinlik, yumaloq uchlar (Lucide uslubiga yaqin), `currentColor`. Belgi plitasi ichida (`.tv-icon`) — tint fon + mos `*-ink` rang. Ikonka bezak bo‘lsa `aria-hidden="true"`.

## Kirish imkoniyati

- Har bir interaktiv element klaviaturadan ishlaydi; `:focus-visible` — 3px `brand` halqa.
- Animatsiyali/illyustrativ rasmlarga `role="img"` + mazmunli `aria-label`.
- Rang yagona signal emas: diagrammada legenda va yozuvlar bor.
- Teginish maydoni ≥ 44px.

## Foydalanish

Komponentlar — CSS klasslar kutubxonasi (`components/bundle.css`, `tv-` prefiksi), JS talab qilmaydi (slayder, filtr, sanoq uchun kichik skript LandingPage’da). `tokens.css` + `bundle.css` ni ulang va komponent qo‘llanmalaridagi HTML tuzilmasidan foydalaning.
