import { SectionHead } from "./SectionHead";

const groups = [
  {
    tone: "brand",
    title: "Maktab va kollej öqituvçilari",
    sub: null,
    points: ["Dars materialini tez va çiroyli tayyorlaş", "Öquvçilar diqqatini uşlab turiş", "Öz işlaringiz saqlanadigan portfolio"],
    cta: { href: "/royxatdan-otish", text: "Bepul boşlaş →" },
    icon: (
      <>
        <rect x="3" y="4" width="18" height="12" rx="2" />
        <path d="M8 20h8M12 16v4" />
      </>
    ),
  },
  {
    tone: "coral",
    title: "Pedagogika OTM talabalari",
    sub: "Bölajak öqituvçilar uçun",
    points: ["Amaliyot va bitiruv işlari uçun vizual materiallar", "Kasbiy könikmani öqiş davridayoq egallaş", "Portfolio uçun işlar töplami"],
    cta: { href: "/royxatdan-otish", text: "Talaba sifatida boşlaş →" },
    icon: (
      <>
        <path d="M2 9l10-5 10 5-10 5z" />
        <path d="M6 11v5c3 2.5 9 2.5 12 0v-5" />
      </>
    ),
  },
  {
    tone: "mint",
    title: "Ta’lim muassasalari",
    sub: "Maktab, kollej va OTM",
    points: ["Butun jamoa uçun yagona platforma", "Öqituvçilar ösişini kuzatiş", "Taşkilot tarifi va hamkorlik"],
    cta: { href: "#boglanish", text: "Boğlaniş →" },
    icon: <path d="M3 21h18M5 21V10l7-5 7 5v11M9 21v-6h6v6" />,
  },
];

export function Audience() {
  return (
    <section className="tv-section aud" aria-labelledby="aud-t">
      <div className="tv-wrap">
        <SectionHead id="aud-t" label="Kimlar uçun" title="Har bir öqituvçi vizual dars yarata oladi" />
        <div className="tv-grid-3">
          {groups.map((g) => (
            <article key={g.title} className="tv-card tv-card-hover tv-reveal">
              <span className={`tv-icon tv-icon-${g.tone}`}>
                <svg viewBox="0 0 24 24" aria-hidden="true">
                  {g.icon}
                </svg>
              </span>
              <h3 className="tv-h3">{g.title}</h3>
              {g.sub && (
                <p className="tv-muted" style={{ margin: "-6px 0 0" }}>
                  {g.sub}
                </p>
              )}
              <ul>
                {g.points.map((p) => (
                  <li key={p}>{p}</li>
                ))}
              </ul>
              <a className="tv-btn tv-btn-text" href={g.cta.href}>
                {g.cta.text}
              </a>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
