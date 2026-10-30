import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { logoutAction } from "@/features/auth/actions";
import { ChangePasswordForm, ResendConfirmationForm } from "@/features/auth/components/forms";
import { getCurrentUser } from "@/features/auth/lib/identity-api";
import { Logo } from "@/shared/ui/Logo";
import { ThemeToggle } from "@/shared/ui/ThemeToggle";
import "@/styles/auth.css";

export const metadata: Metadata = { title: "Kabinet — Tasvir.uz", robots: { index: false, follow: false } };

const dateFormat = new Intl.DateTimeFormat("uz-UZ", { dateStyle: "long", timeZone: "Asia/Tashkent" });

export default async function CabinetPage() {
  const user = await getCurrentUser();
  if (!user) redirect("/kirish?keyin=/kabinet");

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
                Studiya <span className="tv-chip-soon">Ishlab chiqilmoqda</span>
              </li>
              <li>
                Akademiya <span className="tv-chip-soon">Ishlab chiqilmoqda</span>
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
