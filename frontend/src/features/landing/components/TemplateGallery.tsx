"use client";

import { useState, type ReactNode } from "react";
import { SectionHead } from "./SectionHead";

type Subject = "mat" | "bio" | "tar" | "inf" | "ona";

const filters: { id: Subject | "all"; label: string }[] = [
  { id: "all", label: "Hammasi" },
  { id: "mat", label: "Matematika" },
  { id: "bio", label: "Biologiya" },
  { id: "tar", label: "Tarix" },
  { id: "inf", label: "Informatika" },
  { id: "ona", label: "Ona tili" },
];

const small = (fill: string) => ({ font: "700 11px var(--font-sans)", fill });

// TODO: Studiya moduli tayyor bo‘lgach, shablonlar API dan keladi.
const templates: { subject: Subject; title: string; meta: string; alt: string; art: ReactNode }[] = [
  {
    subject: "mat",
    title: "Kvadrat funksiya",
    meta: "Matematika · grafik",
    alt: "Matematika: kvadrat funksiya grafigi namunasi",
    art: (
      <>
        <path d="M20 130h170M30 140V12" className="v-arrow" style={{ strokeWidth: 2 }} />
        <path d="M40 30Q100 210 160 30" fill="none" style={{ stroke: "var(--brand)", strokeWidth: 4, strokeLinecap: "round" }} />
        <circle cx="100" cy="120" r="6" className="v-coral" />
        <circle cx="62" cy="76" r="5" className="v-sun" />
        <circle cx="138" cy="76" r="5" className="v-sun" />
        <rect x="118" y="108" width="62" height="22" rx="11" className="v-tint" />
        <text x="149" y="123" textAnchor="middle" style={small("var(--brand)")}>y = x²</text>
      </>
    ),
  },
  {
    subject: "bio",
    title: "Hujayra tuzilishi",
    meta: "Biologiya · sxema",
    alt: "Biologiya: hujayra tuzilishi sxemasi namunasi",
    art: (
      <>
        <ellipse cx="100" cy="75" rx="80" ry="58" className="v-mint" style={{ opacity: 0.35 }} />
        <ellipse cx="100" cy="75" rx="80" ry="58" fill="none" style={{ stroke: "var(--mint-ink)", strokeWidth: 3 }} />
        <circle cx="96" cy="72" r="22" className="v-brand" />
        <circle cx="96" cy="72" r="8" className="v-tint" />
        <ellipse cx="148" cy="58" rx="14" ry="7" className="v-coral" />
        <ellipse cx="56" cy="96" rx="12" ry="6" className="v-sun" />
        <circle cx="140" cy="100" r="5" className="v-ink" />
        <circle cx="60" cy="50" r="4" className="v-ink" />
      </>
    ),
  },
  {
    subject: "tar",
    title: "Vaqt chizig‘i",
    meta: "Tarix · xronologiya",
    alt: "Tarix: voqealar vaqt chizig‘i namunasi",
    art: (
      <>
        <path d="M14 78h172" style={{ stroke: "var(--ink)", strokeWidth: 3, strokeLinecap: "round" }} />
        {[
          { cx: 34, tone: "v-coral", x: 14, y: 26 },
          { cx: 80, tone: "v-sun", x: 60, y: 96 },
          { cx: 126, tone: "v-mint", x: 106, y: 26 },
          { cx: 170, tone: "v-brand", x: 148, y: 96 },
        ].map((e) => (
          <g key={e.cx}>
            <circle cx={e.cx} cy="78" r="9" className={e.tone} />
            <rect x={e.x} y={e.y} width="44" height="34" rx="8" className="v-card v-line" style={{ strokeWidth: 2 }} />
          </g>
        ))}
      </>
    ),
  },
  {
    subject: "inf",
    title: "Algoritm",
    meta: "Informatika · blok-sxema",
    alt: "Informatika: algoritm blok-sxemasi namunasi",
    art: (
      <>
        <rect x="70" y="6" width="60" height="22" rx="11" className="v-mint" />
        <path d="M100 28v12M100 92v12M64 66H40v52h24M136 66h24v52h-24" className="v-arrow" style={{ strokeWidth: 2.5 }} />
        <path d="M100 40l36 26-36 26-36-26z" className="v-sun" />
        <text x="100" y="70" textAnchor="middle" style={small("#17162b")}>shart?</text>
        <rect x="64" y="104" width="72" height="28" rx="6" className="v-brand" />
        <text x="100" y="122" textAnchor="middle" style={small("var(--on-brand)")}>amal</text>
      </>
    ),
  },
  {
    subject: "ona",
    title: "Gap bo‘laklari",
    meta: "Ona tili · aqliy xarita",
    alt: "Ona tili: gap bo‘laklari aqliy xaritasi namunasi",
    art: (
      <>
        <g className="v-arrow" style={{ strokeWidth: 2.5 }}>
          <path d="M100 75L40 30M100 75l60-45M100 75L36 120M100 75l64 45M100 75v-58" />
        </g>
        <circle cx="100" cy="75" r="26" className="v-brand" />
        <text x="100" y="79" textAnchor="middle" style={small("var(--on-brand)")}>Gap</text>
        <circle cx="40" cy="30" r="14" className="v-coral" />
        <circle cx="160" cy="30" r="14" className="v-sun" />
        <circle cx="36" cy="120" r="14" className="v-mint" />
        <circle cx="164" cy="120" r="14" className="v-coral" />
        <circle cx="100" cy="16" r="11" className="v-mint" />
      </>
    ),
  },
];

export function TemplateGallery() {
  const [active, setActive] = useState<Subject | "all">("all");

  return (
    <section className="tv-section" aria-labelledby="tpl-t">
      <div className="tv-wrap">
        <SectionHead id="tpl-t" label="Shablonlar galereyasi" title="Har bir fan uchun tayyor vizual namunalar" />
        <div className="tv-chips" role="group" aria-label="Fan bo‘yicha saralash">
          {filters.map((f) => (
            <button key={f.id} className="tv-chip" type="button" aria-pressed={active === f.id} onClick={() => setActive(f.id)}>
              {f.label}
            </button>
          ))}
        </div>
        <div className="tpl-grid">
          {templates.map((t) => (
            <article key={t.title} className="tv-tpl tv-reveal" hidden={active !== "all" && active !== t.subject}>
              <div className="tv-tpl-art">
                <svg viewBox="0 0 200 150" role="img" aria-label={t.alt}>
                  {t.art}
                </svg>
              </div>
              <div className="tv-tpl-body">
                <div>
                  <strong>{t.title}</strong>
                  <div className="tv-muted" style={{ fontSize: 14 }}>
                    {t.meta}
                  </div>
                </div>
              </div>
            </article>
          ))}
        </div>
        <div style={{ textAlign: "center", marginTop: 36 }}>
          <a className="tv-btn tv-btn-ghost" href="#shablonlar">
            Barcha shablonlarni ko‘rish
          </a>
        </div>
      </div>
    </section>
  );
}
