import { SectionHead } from "./SectionHead";

const pains = [
  {
    tone: "coral",
    title: "Vaqt yetishmaydi",
    text: "Dars rejasi, daftar tekshirish, hisobotlar… Chiroyli ko‘rgazmali material tayyorlashga esa soatlar kerak.",
    icon: (
      <>
        <circle cx="12" cy="13" r="8" />
        <path d="M12 9v4l2.5 2.5M9 2h6" />
      </>
    ),
  },
  {
    tone: "sun",
    title: "Dizayn ko‘nikmasi yo‘q",
    text: "Qaysi rang, qaysi shakl, ma’lumotni qanday joylash? Murakkab dasturlar o‘rganishga qo‘rqitadi.",
    icon: (
      <>
        <path d="M12 19l7-7 3 3-7 7zM18 13l-1.5-7.5L2 2l3.5 14.5L13 18zM2 2l7.6 7.6" />
        <circle cx="11" cy="11" r="2" />
      </>
    ),
  },
  {
    tone: "brand",
    title: "O‘quvchilar zerikadi",
    text: "Matnga to‘la slaydlar diqqatni ushlab turmaydi — mavzu esda qolmaydi, savollar kamayadi.",
    icon: (
      <>
        <circle cx="12" cy="12" r="9" />
        <path d="M8 15h8M9 9.5h.01M15 9.5h.01" />
      </>
    ),
  },
];

export function Problem() {
  return (
    <section className="tv-section" aria-labelledby="muammo-t">
      <div className="tv-wrap">
        <SectionHead id="muammo-t" label="Tanish holat?" title="Yaxshi dars bor, lekin uni ko‘rinadigan qilish qiyin" />
        <div className="tv-grid-3">
          {pains.map((p) => (
            <article key={p.title} className="tv-card tv-card-hover pain tv-reveal">
              <span className={`tv-icon tv-icon-${p.tone}`}>
                <svg viewBox="0 0 24 24" aria-hidden="true">
                  {p.icon}
                </svg>
              </span>
              <h3 className="tv-h3">{p.title}</h3>
              <p className="tv-muted" style={{ margin: 0 }}>
                {p.text}
              </p>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
