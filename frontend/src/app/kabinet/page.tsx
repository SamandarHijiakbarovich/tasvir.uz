import type { Metadata } from "next";
import { redirect } from "next/navigation";
import Link from "next/link";
import { getCourses } from "@/features/academy/api/academy-api";
import { CourseCard } from "@/features/academy/components/CourseCard";
import { acceptPrivacyAction, logoutAction } from "@/features/auth/actions";
import { ChangePasswordForm, ResendConfirmationForm } from "@/features/auth/components/forms";
import { getCurrentUser } from "@/features/auth/lib/identity-api";
import { Logo } from "@/shared/ui/Logo";
import { ThemeToggle } from "@/shared/ui/ThemeToggle";
import "@/styles/auth.css";
import "@/styles/academy.css";

export const metadata: Metadata = { title: "Kabinet — Tasvir.uz", robots: { index: false, follow: false } };

const dateFormat = new Intl.DateTimeFormat("uz-UZ", { dateStyle: "long", timeZone: "Asia/Tashkent" });

export default async function CabinetPage() {
  const user = await getCurrentUser();
  if (!user) redirect("/kirish?keyin=/kabinet");

  const courses = await getCourses();

  // Backend vaqtni UTC da yuboradi
  const createdAt = new Date(/Z$/.test(user.createdAt) ? user.createdAt : `${user.createdAt}Z`);

  return (
    <div className="tv-cabinet">
      <header className="tv-nav">
        <div className="tv-nav-in">
          <Logo />
          <div className="tv-nav-act" style={{ marginLeft: "auto" }}>
            <ThemeToggle />
            <form action={logoutAction}>
              <button className="tv-btn tv-btn-ghost tv-btn-sm" type="submit" style={{ display: "inline-flex" }}>
                Chiqish
              </button>
            </form>
          </div>
        </div>
      </header>

      <main className="tv-cabinet-in">
        <h1 className="tv-h2">Salom, {user.fullName}!</h1>

        {!user.emailConfirmed && (
          <div className="tv-banner">
            <p>
              <strong>Emailingizni tasdiqlang.</strong> {user.email} manziliga havola yubordik.
            </p>
            <ResendConfirmationForm />
          </div>
        )}

        {!user.privacyAcceptedAt && (
          <div className="tv-banner">
            <p>
              <strong>Maxfiylik siyosati qo‘shildi.</strong> <Link href="/maxfiylik">O‘qib chiqing</Link> va rozilik bildiring.
            </p>
            <form action={acceptPrivacyAction}>
              <button className="tv-btn tv-btn-primary tv-btn-sm" type="submit">
                Tanishdim, roziman
              </button>
            </form>
          </div>
        )}

        <section className="tv-card tv-card-outline tv-course-card" aria-labelledby="studiya-t">
          <div>
            <h2 className="tv-h3" id="studiya-t">
              Studiya
            </h2>
            <p className="tv-muted">Ertangi darsingiz uchun sxema, jadval yoki infografika yarating va uni darsda ko‘rsating.</p>
          </div>
          <div className="tv-lesson-actions">
            <Link className="tv-btn tv-btn-primary" href="/studiya/yangi">
              Yangi material +
            </Link>
            <Link className="tv-btn tv-btn-ghost" href="/studiya">
              Materiallarim
            </Link>
          </div>
        </section>

        <section className="tv-card tv-card-outline tv-course-card" aria-labelledby="guruhlar-t">
          <div>
            <h2 className="tv-h3" id="guruhlar-t">
              Guruhlar
            </h2>
            <p className="tv-muted">Talabalaringiz uchun guruh yarating, ishlarini baholang va natijalarini kuzating. Kodingiz bo‘lsa, guruhga qo‘shiling.</p>
          </div>
          <div className="tv-lesson-actions">
            <Link className="tv-btn tv-btn-primary" href="/guruhlar">
              Guruhlarga o‘tish
            </Link>
          </div>
        </section>

        {courses.ok && courses.data.map((course) => <CourseCard key={course.slug} course={course} />)}

        <p style={{ margin: 0 }}>
          <Link href="/akademiya/ishlarim">Darslardagi ishlarim →</Link>
        </p>

        <div className="tv-cabinet-grid">
          <section className="tv-card" aria-labelledby="profil-t">
            <h2 className="tv-h3" id="profil-t">
              Profil
            </h2>
            <dl className="tv-profile">
              <div>
                <dt>Ism va familiya</dt>
                <dd>{user.fullName}</dd>
              </div>
              <div>
                <dt>Email</dt>
                <dd>{user.email}</dd>
              </div>
              <div>
                <dt>Ro‘yxatdan o‘tgan sana</dt>
                <dd>{dateFormat.format(createdAt)}</dd>
              </div>
            </dl>
          </section>

          <section className="tv-card" aria-labelledby="keyingi-t">
            <h2 className="tv-h3" id="keyingi-t">
              Tez orada
            </h2>
            <ul className="tv-soon">
              <li>
                Sertifikat <span className="tv-chip-soon">Ishlab chiqilmoqda</span>
              </li>
            </ul>
          </section>

          <section className="tv-card tv-cabinet-wide" aria-labelledby="parol-t">
            <h2 className="tv-h3" id="parol-t">
              Parolni o‘zgartirish
            </h2>
            <ChangePasswordForm />
          </section>
        </div>
      </main>
    </div>
  );
}
