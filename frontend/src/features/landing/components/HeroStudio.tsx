// Hero o‘ng tomonidagi "Studiya" animatsiyasi: oddiy matn 9 soniyalik halqada infografikaga aylanadi (faqat CSS).

const tb = { font: "700 15px var(--font-sans)", fill: "var(--on-brand)" };

export function HeroStudio() {
  return (
    <div className="studio-wrap">
      <span className="tv-sticker tv-sticker-sun studio-sticker" aria-hidden="true">
        Studiya ✦
      </span>
      <figure
        className="studio"
        role="img"
        aria-label="Animatsiya: ‘Fotosintez’ mavzusidagi oddiy matn Tasvir.uz Studiyasida quyosh, barg, karbonat angidrid, suv, kislorod va glyukozani ko‘rsatuvchi infografikaga aylanadi."
        style={{ margin: 0 }}
      >
        <div className="studio-bar" aria-hidden="true">
          <i />
          <i />
          <i />
          <span className="fname" style={{ marginLeft: 8 }}>
            fotosintez-dars.tasvir
          </span>
          <span className="studio-mode">
            <span className="m1">Matn</span>
            <span className="m2">Infografika</span>
          </span>
        </div>
        <div className="stage" aria-hidden="true">
          <div className="txt">
            <span className="scan" />
            <h4>Fotosintez</h4>
            <p>
              Fotosintez — yashil o‘simliklarning quyosh nuri energiyasi yordamida karbonat angidrid va suvdan organik
              modda (glyukoza) hosil qilish jarayoni.
            </p>
            <p>Jarayon bargdagi xloroplastlarda kechadi. Natijada atmosferaga kislorod ajralib chiqadi.</p>
            <p>Ya’ni: quyosh nuri + CO₂ + H₂O → glyukoza + O₂.</p>
          </div>
          <div className="viz">
            <svg viewBox="0 0 500 400">
              <rect className="v-paper pop d1" x="150" y="140" width="200" height="130" rx="26" />
              {/* quyosh */}
              <g className="pop d1">
                <circle className="v-sun" cx="88" cy="78" r="40" />
                <g className="v-arrow" style={{ stroke: "var(--ink)", strokeWidth: 3 }}>
                  <path d="M88 22v-10M88 144v-10M32 78H22M154 78h-10M48 38l-7-7M135 125l-7-7M128 38l7-7" />
                </g>
                <text className="v-tb" x="88" y="83" textAnchor="middle">
                  Quyosh
                </text>
              </g>
              <path className="v-arrow draw d2" d="M130 110c28 20 50 30 72 40" />
              <path className="v-arrow draw d2" d="M190 140l14 10-16 4" />
              {/* barg */}
              <g className="pop d2">
                <path className="v-mint" d="M190 230c0-60 50-90 120-86-4 70-40 110-104 106-6 0-12-8-16-20z" />
                <path d="M200 246c30-30 60-58 100-92" fill="none" stroke="#17162b" strokeWidth="3" strokeLinecap="round" />
                <text className="v-t" x="306" y="254" textAnchor="middle">
                  Barg
                </text>
              </g>
              {/* kirish */}
              <g className="pop d3">
                <rect className="v-card v-line" x="30" y="250" width="104" height="44" rx="22" style={{ strokeWidth: 2 }} />
                <text className="v-t" x="82" y="277" textAnchor="middle">
                  CO₂
                </text>
              </g>
              <g className="pop d3">
                <rect className="v-card v-line" x="30" y="310" width="104" height="44" rx="22" style={{ strokeWidth: 2 }} />
                <text className="v-t" x="82" y="337" textAnchor="middle">
                  H₂O
                </text>
              </g>
              <path className="v-arrow draw d4" d="M138 272c20-4 36-8 52-14M138 330c26-14 44-36 58-62" />
              {/* chiqish */}
              <path className="v-arrow draw d5" d="M330 190c30-18 50-40 64-70M386 128l8-10 6 13M320 240c32 10 52 30 66 58M374 290l12 10 2-15" />
              <g className="pop d5">
                <circle className="v-coral" cx="420" cy="86" r="44" />
                <text className="v-tb" x="420" y="92" textAnchor="middle">
                  O₂
                </text>
              </g>
              <g className="pop d6">
                <rect className="v-brand" x="356" y="300" width="128" height="52" rx="16" />
                <text x="420" y="331" textAnchor="middle" style={tb}>
                  Glyukoza
                </text>
              </g>
              <text className="v-ts pop d6" x="250" y="388" textAnchor="middle">
                Xloroplastda: quyosh nuri + CO₂ + H₂O → glyukoza + O₂
              </text>
            </svg>
          </div>
        </div>
      </figure>
    </div>
  );
}
