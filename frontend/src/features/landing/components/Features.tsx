import { SectionHead } from "./SectionHead";

const onBrandLabel = { color: "var(--on-brand)", opacity: 0.85 };
const cardBg = { background: "var(--card)" };
const muted0 = { margin: 0 };

export function Features() {
  return (
    <section className="tv-section tv-alt" id="imkoniyatlar" aria-labelledby="imk-t">
      <div className="tv-wrap">
        <SectionHead id="imk-t" label="Imkoniyatlar" title="Vizual dars uchun kerak bo‘lgan hammasi — bir joyda" />
        <div className="tv-bento">
          <article className="tv-tile tv-tile-brand b-wide tv-reveal">
            <span className="tv-label" style={onBrandLabel}>Akademiya</span>
            <h3 className="tv-h3">Amaliy darslar</h3>
            <p className="tv-muted" style={muted0}>
              Infografika, sxema, diagramma va aqliy xarita yaratishni qisqa, amaliy darslarda o‘rganing.
            </p>
            <svg className="viz" viewBox="0 0 520 120" aria-hidden="true">
              <rect x="0" y="10" width="160" height="100" rx="14" fill="rgba(255,255,255,.14)" />
              <circle cx="80" cy="60" r="22" fill="#ffc93c" />
              <path d="M73 49v22l18-11z" fill="#17162b" />
              <rect x="180" y="24" width="300" height="12" rx="6" fill="rgba(255,255,255,.3)" />
              <rect x="180" y="50" width="220" height="12" rx="6" fill="rgba(255,255,255,.2)" />
              <rect x="180" y="86" width="320" height="6" rx="3" fill="rgba(255,255,255,.2)" />
              <rect x="180" y="86" width="120" height="6" rx="3" fill="#ff6b57" />
            </svg>
          </article>

          <article className="tv-tile tv-tile-sun b-tall tv-reveal">
            <span className="tv-icon tv-icon-sun" style={cardBg}>
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <rect x="3" y="3" width="7" height="7" rx="2" />
                <rect x="14" y="3" width="7" height="7" rx="2" />
                <rect x="3" y="14" width="7" height="7" rx="2" />
                <rect x="14" y="14" width="7" height="7" rx="2" />
              </svg>
            </span>
            <h3 className="tv-h3">Tayyor shablonlar</h3>
            <p className="tv-muted" style={muted0}>Fanlar bo‘yicha infografika va sxema shablonlari — faqat mazmunni qo‘ying.</p>
            <svg className="viz" viewBox="0 0 220 230" aria-hidden="true">
              <g transform="rotate(-6 110 120)">
                <rect x="30" y="20" width="160" height="110" rx="14" className="v-card v-line" style={{ strokeWidth: 2 }} />
                <circle cx="70" cy="62" r="18" className="v-coral" />
                <rect x="100" y="50" width="70" height="10" rx="5" className="v-paper" />
                <rect x="100" y="68" width="50" height="10" rx="5" className="v-paper" />
                <rect x="50" y="96" width="120" height="14" rx="7" className="v-tint" />
              </g>
              <g transform="rotate(5 110 160)">
                <rect x="40" y="110" width="160" height="110" rx="14" className="v-card v-line" style={{ strokeWidth: 2 }} />
                <path d="M70 190l30-40 26 22 30-34" className="v-arrow" style={{ stroke: "var(--brand)" }} />
                <circle cx="156" cy="138" r="6" className="v-mint" />
              </g>
            </svg>
          </article>

          <article className="tv-tile tv-reveal">
            <span className="tv-icon tv-icon-mint">
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M9 11l3 3 8-8" />
                <path d="M20 12v6a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h9" />
              </svg>
            </span>
            <h3 className="tv-h3">Amaliy topshiriqlar</h3>
            <p className="tv-muted" style={muted0}>Har bir mavzudan so‘ng o‘z darsingiz uchun material yaratib, mustahkamlang.</p>
          </article>

          <article className="tv-tile b-wide tv-reveal">
            <span className="tv-icon tv-icon-brand">
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M3 20h18M6 16V10M11 16V6M16 16v-4M21 16V8" />
              </svg>
            </span>
            <h3 className="tv-h3">Progress kuzatuvi</h3>
            <p className="tv-muted" style={muted0}>Qaysi modulni tugatdingiz, qaysi topshiriq qoldi — barchasi ko‘z oldingizda.</p>
            <svg className="viz" viewBox="0 0 520 70" aria-hidden="true">
              <text x="0" y="16" className="v-ts">1-modul</text>
              <rect x="80" y="6" width="440" height="12" rx="6" className="v-paper" />
              <rect x="80" y="6" width="440" height="12" rx="6" className="v-mint" />
              <text x="0" y="40" className="v-ts">2-modul</text>
              <rect x="80" y="30" width="440" height="12" rx="6" className="v-paper" />
              <rect x="80" y="30" width="260" height="12" rx="6" className="v-sun" />
              <text x="0" y="64" className="v-ts">3-modul</text>
              <rect x="80" y="54" width="440" height="12" rx="6" className="v-paper" />
              <rect x="80" y="54" width="90" height="12" rx="6" className="v-coral" />
            </svg>
          </article>

          <article className="tv-tile tv-tile-coral tv-reveal">
            <span className="tv-icon tv-icon-coral" style={cardBg}>
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <circle cx="12" cy="9" r="6" />
                <path d="M8.5 14L7 22l5-3 5 3-1.5-8" />
              </svg>
            </span>
            <h3 className="tv-h3">Sertifikat</h3>
            <p className="tv-muted" style={muted0}>Tez orada: kursni yakunlaganingizni tasdiqlovchi sertifikat.</p>
          </article>

          <article className="tv-tile tv-tile-mint tv-reveal" style={{ gridColumn: "1/-1" }}>
            <span className="tv-icon tv-icon-mint" style={cardBg}>
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <circle cx="9" cy="8" r="3.5" />
                <path d="M2.5 20a6.5 6.5 0 0 1 13 0" />
                <circle cx="17" cy="9" r="2.5" />
                <path d="M16 14a5 5 0 0 1 6 5" />
              </svg>
            </span>
            <h3 className="tv-h3">O‘qituvchilar hamjamiyati</h3>
            <p className="tv-muted" style={muted0}>Hamkasblaringiz bilan ishlaringizni ulashing, fikr oling va g‘oyalar almashing.</p>
            <div className="viz" style={{ display: "flex" }} aria-hidden="true">
              <span className="tv-avatar" style={{ background: "var(--sun)" }} />
              <span className="tv-avatar" style={{ background: "var(--coral)", marginLeft: -12 }} />
              <span className="tv-avatar" style={{ background: "var(--brand-tint)", marginLeft: -12 }} />
              <span className="tv-avatar" style={{ background: "var(--card)", marginLeft: -12 }}>+</span>
            </div>
          </article>
        </div>
      </div>
    </section>
  );
}
