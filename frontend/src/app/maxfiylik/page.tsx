import type { Metadata } from "next";
import { Logo } from "@/shared/ui/Logo";
import { ThemeToggle } from "@/shared/ui/ThemeToggle";
import "@/styles/classroom.css";

export const metadata: Metadata = {
  title: "Maxfiylik siyosati — Tasvir.uz",
  description: "Tasvir.uz qanday ma’lumotlarni yig‘adi, nima uchun ishlatadi va ularni kim ko‘radi.",
};

export default function PrivacyPage() {
  return (
    <div className="tv-legal">
      <header className="tv-nav">
        <div className="tv-nav-in">
          <Logo />
          <div className="tv-nav-act" style={{ marginLeft: "auto" }}>
            <ThemeToggle />
          </div>
        </div>
      </header>
      <main className="tv-legal-in">
        <h1 className="tv-h2">Maxfiylik siyosati</h1>
        <p>
          Bu sahifa Tasvir.uz foydalanuvchilar haqida qanday ma’lumotlarni saqlashi, ulardan nima uchun foydalanishi va ularni kim ko‘ra olishini tushuntiradi. Hisob ochishdan oldin
          o‘qib chiqing.
        </p>

        <h2 className="tv-h3">Qanday ma’lumotlar saqlanadi</h2>
        <ul>
          <li>Hisob: ism va familiya, email, parolning qaytarib bo‘lmaydigan xeshi (parolning o‘zi saqlanmaydi).</li>
          <li>O‘qish: darslardagi progress, test va mashqlarga javoblar (xato urinishlar ham), amaliy topshiriq natijalari.</li>
          <li>Studiya: siz yaratgan materiallar.</li>
          <li>Guruh: qo‘shilgan guruhlaringiz, topshirgan ishlaringiz, o‘qituvchi baholari va so‘rovnoma javoblaringiz.</li>
          <li>Texnik: kirish sessiyasini saqlovchi cookie fayllar va so‘rovlarni cheklash uchun vaqtincha ishlatiladigan IP manzil.</li>
        </ul>

        <h2 className="tv-h3">Nima uchun kerak</h2>
        <ul>
          <li>Hisobingizga kirish, progressingiz va materiallaringizni saqlash.</li>
          <li>Parolni tiklash va emailni tasdiqlash xatlarini yuborish.</li>
          <li>Guruh o‘qituvchisiga natijalaringizni ko‘rsatish (faqat guruhga qo‘shilsangiz).</li>
          <li>Pedagogika fanlari bo‘yicha dissertatsiya tadqiqoti: tajriba-sinov natijalari umumlashtirilgan holda, ism-sharifsiz tahlil qilinadi.</li>
        </ul>
        <p>Reklama uchun ishlatilmaydi va sotilmaydi.</p>

        <h2 className="tv-h3">Kim ko‘radi</h2>
        <ul>
          <li>Kod bilan guruhga qo‘shilsangiz, shu guruh o‘qituvchisi ismingizni, progressingizni, test natijalaringizni va topshirgan ishlaringizni ko‘radi. Buning uchun alohida rozilik so‘raladi.</li>
          <li>So‘rovnoma javoblarini o‘qituvchi faqat umumlashtirilgan holda, izohlarni esa ismsiz ko‘radi.</li>
          <li>Boshqa foydalanuvchilar sizning ma’lumotlaringizni ko‘rmaydi.</li>
          <li>Matematika kursidagi videolar YouTube orqali ko‘rsatiladi: videoni ochganingizda YouTube o‘z shartlari asosida ma’lumot olishi mumkin.</li>
        </ul>

        <h2 className="tv-h3">Sizning huquqlaringiz</h2>
        <ul>
          <li>Guruhdan istalgan vaqtda chiqishingiz mumkin: shu guruhdagi ishlaringiz, baholaringiz va so‘rovnoma javoblaringiz o‘chadi.</li>
          <li>Ma’lumotlaringizni ko‘rish, tuzatish yoki hisobingizni o‘chirishni so‘rash uchun quyidagi manzilga yozing.</li>
        </ul>
        <p>
          Ma’lumotlar O‘zbekiston Respublikasining “Shaxsga doir ma’lumotlar to‘g‘risida”gi Qonuni talablari asosida qayta ishlanadi.
        </p>

        <h2 className="tv-h3">Bog‘lanish</h2>
        <p>[Mas’ul shaxs yoki tashkilot nomi, aloqa emaili va manzili shu yerga yoziladi]</p>
        <p className="tv-muted">Tahrir sanasi: 2026-yil 30-oktabr.</p>
      </main>
    </div>
  );
}
