import type { Metadata } from "next";
import Link from "next/link";
import { getToolCatalog } from "@/features/tools/api";
import { ToolCatalogView } from "@/features/tools/components/ToolCatalogView";
import { Logo } from "@/shared/ui/Logo";
import { ThemeToggle } from "@/shared/ui/ThemeToggle";
import "@/styles/auth.css";
import "@/styles/classroom.css";
import "@/styles/studio.css";
import "@/styles/tools.css";

export const metadata: Metadata = {
  title: "Vizuallashtirish dasturlari — Tasvir.uz",
  description: "Vizualizatsiya nima, qaysi mazmunga qaysi shakl mos va Canva, GeoGebra, Power BI, Tableau kabi dasturlar nima qiladi.",
};

type Props = { searchParams: Promise<{ fan?: string }> };

export default async function ToolsPage({ searchParams }: Props) {
  const { fan } = await searchParams;
  const catalog = await getToolCatalog();
  const subject = catalog?.subjects.find((s) => s.key === fan);

  return (
    <div className="tv-legal">
      <header className="tv-nav">
        <div className="tv-nav-in">
          <Logo />
          <div className="tv-nav-act" style={{ marginLeft: "auto" }}>
            <Link className="tv-btn tv-btn-text" href="/studiya">
              Studiya
            </Link>
            <ThemeToggle />
          </div>
        </div>
      </header>
      <main className="tv-tools-in">
        <div className="tv-tools-section">
          <h1 className="tv-h2">Vizuallashtirish dasturlari</h1>
          <p className="tv-lead">
            O‘quv materialini ko‘rgazmali qilishning ko‘p yo‘li bor. Bu sahifa vizualizatsiya nima ekanini, qaysi mazmunga qaysi shakl mosligini va mashhur dasturlar nima
            qilishini tushuntiradi.
          </p>
        </div>
        {catalog ? (
          <ToolCatalogView catalog={catalog} subject={subject} />
        ) : (
          <p className="tv-form-msg err">Katalogni yuklab bo‘lmadi. Birozdan keyin sahifani yangilang.</p>
        )}
        <section className="tv-card tv-tools-cta">
          <div>
            <h2 className="tv-h3">O‘z materialingizni yarating</h2>
            <p className="tv-muted" style={{ margin: "var(--space-2) 0 0" }}>
              Blok-sxema, vaqt chizig‘i, aqliy xarita, qiyosiy jadval va infografikani Tasvir.uz Studiyasida shablondan tayyorlang.
            </p>
          </div>
          <Link className="tv-btn tv-btn-primary" href="/studiya/yangi">
            Studiyani ochish
          </Link>
        </section>
      </main>
    </div>
  );
}
