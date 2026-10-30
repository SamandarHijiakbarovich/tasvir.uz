import type { Partner } from "../api/types";

export function Trust({ partners }: { partners: Partner[] }) {
  return (
    <section className="trust" aria-label="Ishonch">
      <div className="trust-in">
        <div className="seal">
          <span className="seal-ic">
            <svg viewBox="0 0 24 24" aria-hidden="true">
              <path d="M9 3h6M10 3v6L5 18a2 2 0 0 0 1.7 3h10.6A2 2 0 0 0 19 18l-5-9V3" />
              <path d="M7.5 14h9" />
            </svg>
          </span>
          <span>
            Ilmiy tadqiqot asosida ishlab chiqilgan
            <br />
            <span className="tv-muted" style={{ fontWeight: 400, fontSize: 14 }}>
              Pedagogika fanlari bo‘yicha dissertatsiya · tajriba-sinovdan o‘tgan
            </span>
          </span>
        </div>
        {partners.length > 0 && (
          <div className="logos" aria-label="Hamkor muassasalar">
            {partners.map((p, i) =>
              p.logoUrl ? (
                <img key={i} src={p.logoUrl} alt={p.name} height={48} />
              ) : (
                <span key={i} className="logo-ph">
                  {p.name}
                </span>
              ),
            )}
          </div>
        )}
      </div>
    </section>
  );
}
