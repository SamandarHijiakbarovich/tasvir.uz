import { SectionHead } from "./SectionHead";

const steps = [
  {
    badge: "Akademiya",
    badgeClass: "tv-badge",
    title: "O‘rgan",
    text: "Vizual o‘quv material yaratishni bosqichma-bosqich o‘rgatuvchi video kurslar va amaliy topshiriqlar.",
    viz: (
      <>
        <rect x="20" y="10" width="120" height="74" rx="12" className="v-card v-line" style={{ strokeWidth: 2 }} />
        <path d="M72 34v26l22-13z" className="v-brand" />
        <rect x="32" y="74" width="96" height="4" rx="2" className="v-paper" />
        <rect x="32" y="74" width="40" height="4" rx="2" className="v-coral" />
      </>
    ),
  },
  {
    badge: "Studiya",
    badgeClass: "tv-badge tv-badge-coral",
    title: "Yarat",
    text: "Tayyor shablonni tanlang, mavzuingizni kiriting — infografika yoki sxema tayyor.",
    viz: (
      <>
        <rect x="14" y="14" width="56" height="68" rx="10" className="v-card v-line" style={{ strokeWidth: 2 }} />
        <rect x="24" y="26" width="36" height="5" rx="2.5" className="v-paper" />
        <rect x="24" y="38" width="28" height="5" rx="2.5" className="v-paper" />
        <rect x="24" y="50" width="34" height="5" rx="2.5" className="v-paper" />
        <path d="M78 48h14M86 42l6 6-6 6" className="v-arrow" />
        <circle cx="122" cy="30" r="14" className="v-sun" />
        <rect x="100" y="54" width="20" height="22" rx="6" className="v-mint" />
        <rect x="126" y="54" width="20" height="22" rx="6" className="v-coral" />
      </>
    ),
  },
  {
    badge: "Sinfda",
    badgeClass: "tv-badge tv-badge-mint",
    title: "Darsda qo‘lla",
    text: "Materialni doskada ko‘rsating, chop eting yoki o‘quvchilarga ulashing — mavzu ko‘z oldida jonlanadi.",
    viz: (
      <>
        <rect x="18" y="10" width="124" height="60" rx="8" className="v-ink" />
        <circle cx="56" cy="40" r="14" className="v-sun" />
        <rect x="80" y="28" width="46" height="8" rx="4" className="v-mint" />
        <rect x="80" y="44" width="34" height="8" rx="4" className="v-coral" />
        <path d="M80 70l-10 18M80 70l10 18" className="v-arrow" />
      </>
    ),
  },
];

export function HowItWorks() {
  return (
    <section className="tv-section tv-alt" id="qanday" aria-labelledby="qanday-t">
      <div className="tv-wrap">
        <SectionHead
          id="qanday-t"
          label="Qanday ishlaydi"
          title="O‘rgan. Yarat. Darsda qo‘lla."
          lead="Akademiya sizni o‘rgatadi, Studiya ishni tezlashtiradi — natija esa sinfda ko‘rinadi."
        />
        <div className="tv-grid-3 steps">
          {steps.map((s, i) => (
            <div key={s.title} className="tv-step tv-reveal">
              <article className="tv-card">
                <div className="tv-step-n">{i + 1}</div>
                <span className={s.badgeClass}>{s.badge}</span>
                <h3 className="tv-h3" style={{ marginTop: 12 }}>
                  {s.title}
                </h3>
                <p className="tv-muted" style={{ margin: "8px 0 0" }}>
                  {s.text}
                </p>
                <div className="step-viz" aria-hidden="true">
                  <svg viewBox="0 0 160 96">{s.viz}</svg>
                </div>
              </article>
              {i < steps.length - 1 && (
                <svg className="tv-arrow" viewBox="0 0 56 40" aria-hidden="true">
                  <path d="M4 28C16 6 36 4 50 18M42 10l8 8-10 3" />
                </svg>
              )}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
