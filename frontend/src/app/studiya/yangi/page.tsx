import type { Metadata } from "next";
import Link from "next/link";
import { createFromTemplateAction } from "@/features/studio/actions";
import { StudioHeader } from "@/features/studio/components/StudioHeader";
import { describeMaterial, renderMaterial, svgDataUri } from "@/features/studio/lib/render";
import { TEMPLATES } from "@/features/studio/lib/templates";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/studio.css";

export const metadata: Metadata = { title: "Yangi material — Tasvir.uz", robots: { index: false, follow: false } };

export default function NewMaterialPage() {
  return (
    <div className="tv-studio">
      <StudioHeader />
      <main className="tv-studio-in">
        <div>
          <p className="tv-breadcrumb">
            <Link href="/studiya">Materiallarim</Link>
          </p>
          <h1 className="tv-h2" style={{ marginTop: 8 }}>
            Mazmunga mos shablonni tanlang
          </h1>
          <p className="tv-lead" style={{ marginTop: 8 }}>
            Avval o‘zingizga savol bering: mavzuda tushunchalar bir-biri bilan qanday bog‘langan?
          </p>
        </div>

        <ul className="tv-material-grid">
          {TEMPLATES.map((template) => {
            const sample = { templateKey: template.key, ...template.starter };
            return (
              <li key={template.key} className="tv-card tv-material-card">
                <span className="tv-material-thumb">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img src={svgDataUri(renderMaterial(sample).svg)} alt={`Namuna: ${describeMaterial(sample)}`} />
                </span>
                <span className="tv-template-relation">{template.relation}</span>
                <h2 className="tv-h3">{template.name}</h2>
                <p className="tv-template-why">{template.whenToUse}</p>
                <form className="tv-material-actions" action={createFromTemplateAction.bind(null, template.key)}>
                  <button className="tv-btn tv-btn-primary tv-btn-sm" type="submit">
                    Shu shablondan boshlash
                  </button>
                </form>
              </li>
            );
          })}
        </ul>
      </main>
    </div>
  );
}
