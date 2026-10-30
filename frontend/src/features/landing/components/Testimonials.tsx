import type { Testimonial } from "../api/types";
import { SectionHead } from "./SectionHead";

export function Testimonials({ items }: { items: Testimonial[] }) {
  return (
    <section className="tv-section" aria-labelledby="fikr-t">
      <div className="tv-wrap">
        <SectionHead id="fikr-t" label="Fikrlar" title="O‘qituvchilar va talabalar nima deydi" />
        <div className="tv-grid-3">
          {items.map((t, i) => (
            <figure key={i} className="tv-card tv-quote tv-reveal" style={{ margin: 0 }}>
              {/* Tasdiqlanmagan sharh — namuna joy sifatida ko‘rsatiladi */}
              <blockquote className={t.isVerified ? undefined : "tv-ph"}>“{t.quote}”</blockquote>
              <figcaption className="tv-person">
                {t.avatarUrl ? (
                  <img className="tv-avatar" src={t.avatarUrl} alt="" width={48} height={48} />
                ) : (
                  <span className="tv-avatar" role="img" aria-label="Rasm uchun joy">
                    Rasm
                  </span>
                )}
                <span>
                  <strong>{t.authorName}</strong>
                  <br />
                  <span className="tv-muted" style={{ fontSize: 14 }}>
                    {t.authorRole}
                  </span>
                </span>
              </figcaption>
            </figure>
          ))}
        </div>
      </div>
    </section>
  );
}
