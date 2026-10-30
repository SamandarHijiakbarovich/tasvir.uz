export function Research() {
  return (
    <section className="tv-section tv-alt" id="tadqiqot" aria-labelledby="tad-t">
      <div className="tv-wrap">
        <div className="research tv-reveal">
          <svg className="research-viz" viewBox="0 0 220 220" aria-hidden="true">
            <circle className="ring" cx="110" cy="110" r="90" style={{ stroke: "var(--brand-tint)" }} />
            <path className="ring" d="M110 20a90 90 0 0 1 90 90" style={{ stroke: "var(--brand)" }} strokeLinecap="round" />
            <circle className="ring" cx="110" cy="110" r="60" style={{ stroke: "var(--mint-tint)" }} />
            <path className="ring" d="M170 110a60 60 0 0 1-60 60" style={{ stroke: "var(--mint)" }} strokeLinecap="round" />
            <circle cx="110" cy="110" r="26" style={{ fill: "var(--sun)" }} />
            <path d="M100 110l7 7 14-15" fill="none" stroke="#17162b" strokeWidth="4" strokeLinecap="round" strokeLinejoin="round" />
          </svg>
          <div style={{ display: "grid", gap: 16, justifyItems: "start" }}>
            <span className="tv-label">Tadqiqot haqida</span>
            <h2 className="tv-h2" id="tad-t">
              Intuitsiya emas — isbotlangan metodika
            </h2>
            <p className="tv-lead">
              Tasvir.uz pedagogika fanlari bo‘yicha dissertatsiya tadqiqoti asosida ishlab chiqilgan. Vizual o‘quv
              materiallarini yaratish va darsda qo‘llash metodikasi tajriba-sinovdan o‘tkazilgan.
            </p>
            <a className="tv-btn tv-btn-ghost" href="#tadqiqot-sahifa">
              Tadqiqot bilan tanishish →
            </a>
          </div>
        </div>
      </div>
    </section>
  );
}
