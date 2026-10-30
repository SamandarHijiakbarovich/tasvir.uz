import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { duplicateMaterialAction } from "@/features/studio/actions";
import { listMaterials } from "@/features/studio/api/studio-api";
import { DeleteMaterialButton } from "@/features/studio/components/DeleteMaterialButton";
import { StudioHeader } from "@/features/studio/components/StudioHeader";
import { describeMaterial, renderMaterial, svgDataUri } from "@/features/studio/lib/render";
import { getTemplate } from "@/features/studio/lib/templates";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/studio.css";

export const metadata: Metadata = { title: "Materiallarim — Tasvir.uz", robots: { index: false, follow: false } };

const dateFormat = new Intl.DateTimeFormat("uz-UZ", { dateStyle: "medium", timeZone: "Asia/Tashkent" });

type Props = { searchParams: Promise<{ fan?: string; xato?: string }> };

export default async function MaterialsPage({ searchParams }: Props) {
  const { fan, xato } = await searchParams;
  const result = await listMaterials();

  if (!result.ok && result.status === 401) redirect("/kirish?keyin=/studiya");

  const all = result.ok ? result.data : [];
  const subjects = [...new Set(all.map((m) => m.subject).filter((s): s is string => Boolean(s)))].sort();
  const materials = fan ? all.filter((m) => m.subject === fan) : all;

  return (
    <div className="tv-studio">
      <StudioHeader />
      <main className="tv-studio-in">
        <div className="tv-studio-head">
          <div>
            <h1 className="tv-h2">Materiallarim</h1>
            <p className="tv-muted">Darslaringiz uchun yaratgan vizual materiallar shu yerda saqlanadi.</p>
          </div>
          <Link className="tv-btn tv-btn-primary" href="/studiya/yangi">
            Yangi material +
          </Link>
        </div>

        {(xato || !result.ok) && (
          <p className="tv-form-msg err" role="alert">
            {xato ?? (!result.ok ? result.message : "")}
          </p>
        )}

        {subjects.length > 0 && (
          <nav className="tv-filter" aria-label="Fan bo‘yicha saralash">
            <Link href="/studiya" aria-current={!fan}>
              Hammasi
            </Link>
            {subjects.map((subject) => (
              <Link key={subject} href={`/studiya?fan=${encodeURIComponent(subject)}`} aria-current={fan === subject}>
                {subject}
              </Link>
            ))}
          </nav>
        )}

        {result.ok && all.length === 0 && (
          <section className="tv-card tv-empty">
            <h2 className="tv-h3">Hali material yo‘q</h2>
            <p className="tv-muted">Shablon tanlang, namunani o‘z mavzuingizga moslang va ertangi darsingizda ishlating.</p>
            <Link className="tv-btn tv-btn-primary" href="/studiya/yangi">
              Birinchi materialni yaratish
            </Link>
          </section>
        )}

        {materials.length > 0 && (
          <ul className="tv-material-grid">
            {materials.map((material) => (
              <li key={material.id} className="tv-card tv-material-card">
                <Link className="tv-material-thumb" href={`/studiya/${material.id}`} tabIndex={-1} aria-hidden="true">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img src={svgDataUri(renderMaterial(material).svg)} alt={describeMaterial(material)} loading="lazy" />
                </Link>
                <h2 className="tv-h3">
                  <Link href={`/studiya/${material.id}`}>{material.title}</Link>
                </h2>
                <p className="tv-material-meta">
                  {[getTemplate(material.templateKey)?.name, material.subject, material.grade ? `${material.grade}-sinf` : null].filter(Boolean).join(" · ")}
                  <br />
                  O‘zgartirilgan: {dateFormat.format(new Date(/Z$/.test(material.updatedAt) ? material.updatedAt : `${material.updatedAt}Z`))}
                </p>
                <div className="tv-material-actions">
                  <Link className="tv-btn tv-btn-primary tv-btn-sm" href={`/studiya/${material.id}/korsatish`}>
                    Ko‘rsatish
                  </Link>
                  <form action={duplicateMaterialAction.bind(null, material.id)}>
                    <button className="tv-btn tv-btn-ghost tv-btn-sm" type="submit">
                      Nusxa olish
                    </button>
                  </form>
                  <DeleteMaterialButton id={material.id} title={material.title} />
                </div>
              </li>
            ))}
          </ul>
        )}
      </main>
    </div>
  );
}
