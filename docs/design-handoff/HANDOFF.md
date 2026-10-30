# Tasvir.uz — dizayndan kodga topshirish

## Papkada nima bor

| Fayl | Nima |
|---|---|
| `index.html` | Tayyor landing sahifa (16 blok). Brauzerda ochsangiz ishlaydi — bu **namuna (reference)** |
| `css/tokens.css` | Ranglar, oraliqlar, radiuslar, shriftlar — CSS o‘zgaruvchilar (yorug‘/qorong‘i) |
| `css/components.css` | Komponent stillari (`tv-` prefiksi): tugma, kartochka, navbar, narx, FAQ… |
| `tokens.json` | Xuddi shu tokenlar JSON ko‘rinishida (Tailwind/boshqa tizimga o‘tkazish uchun) |
| `BRAND.md` | Brend qoidalari: ovoz, ranglar, shrift, bezaklar, kirish imkoniyati |
| `assets/` | Logotip belgisi (rangli, oq, bir rangli) |

## Qoidalar (dasturchi uchun)

1. Ranglar va o‘lchamlarni **qattiq yozmang** — faqat `var(--brand)`, `var(--space-6)`, `var(--radius-lg)` kabi tokenlardan foydalaning.
2. Qorong‘i rejim: `<html data-theme="dark">`. Belgilanmasa, tizim sozlamasiga ergashadi.
3. O‘zbek belgilari: **o‘ g‘** — U+2018 (‘), tutuq — U+02BC (ʼ). Oddiy `'` ishlatilmasin.
4. Namuna joylar (`—%`, `— so‘m`, `[Ism Familiya]`, “Hamkor logotipi”) — **haqiqiy ma’lumot kelguncha shunday qoladi**, o‘ylab topilgan raqam qo‘yilmaydi.
5. Natija raqamlari: `data-count="72"`, diagramma ustunlari: `data-v="72"` — qiymat yozilsa animatsiya o‘zi ishlaydi.
6. Animatsiyalar `prefers-reduced-motion` ni hurmat qilishi shart; klaviatura bilan boshqarish va `alt`/`aria-label` saqlansin.
7. Telefon (≤520px), planshet (≤960px), kompyuter — gorizontal siljish bo‘lmasin.

## Tavsiya etilgan tuzilma (ASP.NET Core)

```
wwwroot/
  css/tokens.css
  css/components.css
  css/landing.css        ← index.html dagi <style> qismi
  js/landing.js          ← index.html dagi <script> qismi
  img/tasvir-mark*.svg
Pages/
  Shared/_Layout.cshtml  ← <head>, Navbar, Footer
  Shared/_Navbar.cshtml
  Shared/_Footer.cshtml
  Index.cshtml           ← bo‘limlar; har biri alohida partial bo‘lishi mumkin
```

Narxlar, sharhlar, FAQ, natija raqamlari keyinchalik bazadan (EF Core) yoki `appsettings` dan kelishi uchun model bilan render qilinsin.
