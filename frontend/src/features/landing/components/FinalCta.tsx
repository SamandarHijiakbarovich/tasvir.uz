import type { CSSProperties } from "react";

const ctaButton = { "--line-strong": "#000", background: "#ffc93c", color: "#17162b" } as CSSProperties;

export function FinalCta() {
  return (
    <section className="tv-section" style={{ paddingTop: 0 }} id="boshlash" aria-labelledby="cta-t">
      <div className="tv-wrap">
        <div className="tv-cta tv-reveal">
          <svg className="cta-doodle a" viewBox="0 0 90 90" aria-hidden="true">
            <path d="M45 10l8 24 26 2-20 16 7 26-21-15-21 15 7-26-20-16 26-2z" style={{ stroke: "#ffc93c" }} />
          </svg>
          <svg className="cta-doodle b" viewBox="0 0 110 70" aria-hidden="true">
            <path d="M6 50c20-40 60-46 90-20M84 18l12 12-16 4" style={{ stroke: "#ff7d6b" }} />
          </svg>
          <div style={{ display: "grid", gap: 20, justifyItems: "center", maxWidth: 680, margin: "0 auto", position: "relative" }}>
            <span className="tv-hand" style={{ color: "#ffd25e" }}>
              bugun boshlang!
            </span>
            <h2 className="tv-h2" id="cta-t">
              Birinchi vizual darsingizni bugun yarating
            </h2>
            <p className="tv-lead">Ro‘yxatdan o‘ting va Akademiyaning birinchi darsini bepul boshlang.</p>
            <a className="tv-btn tv-btn-primary" href="/royxatdan-otish" style={ctaButton}>
              Bepul boshlash →
            </a>
          </div>
        </div>
      </div>
    </section>
  );
}
