import type { Metadata } from "next";
import { Logo } from "@/shared/ui/Logo";
import { ThemeToggle } from "@/shared/ui/ThemeToggle";
import "@/styles/classroom.css";

export const metadata: Metadata = {
  title: "Maxfiylik siyosati — Tasvir.uz",
  description: "Tasvir.uz qanday ma’lumotlarni yiğadi, nima uçun işlatadi va ularni kim köradi.",
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
          Bu sahifa Tasvir.uz foydalanuvçilar haqida qanday ma’lumotlarni saqlaşi, ulardan nima uçun foydalanişi va ularni kim köra olişini tuşuntiradi. Hisob oçişdan oldin
          öqib çiqing.
        </p>

        <h2 className="tv-h3">Qanday ma’lumotlar saqlanadi</h2>
        <ul>
          <li>Hisob: ism va familiya, email, parolning qaytarib bölmaydigan xeşi (parolning özi saqlanmaydi).</li>
          <li>Öqiş: darslardagi progress, test va maşqlarga javoblar (xato urinişlar ham), amaliy topşiriq natijalari.</li>
          <li>Studiya: siz yaratgan materiallar.</li>
          <li>Guruh: qöşilgan guruhlaringiz, topşirgan işlaringiz, öqituvçi baholari va sörovnoma javoblaringiz.</li>
          <li>Texnik: kiriş sessiyasini saqlovçi cookie fayllar va sörovlarni çeklaş uçun vaqtinça işlatiladigan IP manzil.</li>
        </ul>

        <h2 className="tv-h3">Nima uçun kerak</h2>
        <ul>
          <li>Hisobingizga kiriş, progressingiz va materiallaringizni saqlaş.</li>
          <li>Parolni tiklaş va emailni tasdiqlaş xatlarini yuboriş.</li>
          <li>Guruh öqituvçisiga natijalaringizni körsatiş (faqat guruhga qöşilsangiz).</li>
          <li>Pedagogika fanlari böyiça dissertatsiya tadqiqoti: tajriba-sinov natijalari umumlaştirilgan holda, ism-şarifsiz tahlil qilinadi.</li>
        </ul>
        <p>Reklama uçun işlatilmaydi va sotilmaydi.</p>

        <h2 className="tv-h3">Kim köradi</h2>
        <ul>
          <li>Kod bilan guruhga qöşilsangiz, şu guruh öqituvçisi ismingizni, progressingizni, test natijalaringizni va topşirgan işlaringizni köradi. Buning uçun alohida rozilik söraladi.</li>
          <li>Sörovnoma javoblarini öqituvçi faqat umumlaştirilgan holda, izohlarni esa ismsiz köradi.</li>
          <li>Boşqa foydalanuvçilar sizning ma’lumotlaringizni körmaydi.</li>
          <li>Matematika kursidagi videolar YouTube orqali körsatiladi: videoni oçganingizda YouTube öz şartlari asosida ma’lumot olişi mumkin.</li>
        </ul>

        <h2 className="tv-h3">Sizning huquqlaringiz</h2>
        <ul>
          <li>Guruhdan istalgan vaqtda çiqişingiz mumkin: şu guruhdagi işlaringiz, baholaringiz va sörovnoma javoblaringiz öçadi.</li>
          <li>Ma’lumotlaringizni köriş, tuzatiş yoki hisobingizni öçirişni söraş uçun quyidagi manzilga yozing.</li>
        </ul>
        <p>
          Ma’lumotlar Özbekiston Respublikasining “Şaxsga doir ma’lumotlar töğrisida”gi Qonuni talablari asosida qayta işlanadi.
        </p>

        <h2 className="tv-h3">Boğlaniş</h2>
        <p>[Mas’ul şaxs yoki taşkilot nomi, aloqa emaili va manzili şu yerga yoziladi]</p>
        <p className="tv-muted">Tahrir sanasi: 2026-yil 30-oktabr.</p>
      </main>
    </div>
  );
}
