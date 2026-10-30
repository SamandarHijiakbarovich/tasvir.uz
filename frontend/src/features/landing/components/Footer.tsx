import { Logo } from "@/shared/ui/Logo";

const columns = [
  {
    title: "Platforma",
    links: [
      { href: "#", label: "Akademiya" },
      { href: "#", label: "Studiya" },
      { href: "#", label: "Shablonlar" },
      { href: "#narxlar", label: "Narxlar" },
    ],
  },
  {
    title: "Kompaniya",
    links: [
      { href: "#tadqiqot", label: "Tadqiqot" },
      { href: "#", label: "Biz haqimizda" },
      { href: "#", label: "Maxfiylik siyosati" },
      { href: "#", label: "Foydalanish shartlari" },
    ],
  },
];

export function Footer() {
  return (
    <footer className="tv-footer">
      <div className="ft">
        <div style={{ display: "grid", gap: 18, alignContent: "start" }}>
          <Logo />
          <p style={{ margin: 0, opacity: 0.8, maxWidth: 320 }}>Murakkabni oddiy, zerikarlini qiziqarli qiling.</p>
          <div className="tv-social">
            <a href="#telegram" aria-label="Telegram">
              <svg viewBox="0 0 24 24">
                <path d="M21 4L3 11l6 2 2 6 3-4 5 4z" />
                <path d="M9 13l12-9" />
              </svg>
            </a>
            <a href="#instagram" aria-label="Instagram">
              <svg viewBox="0 0 24 24">
                <rect x="3" y="3" width="18" height="18" rx="5" />
                <circle cx="12" cy="12" r="4" />
                <circle cx="17.5" cy="6.5" r=".5" />
              </svg>
            </a>
            <a href="#youtube" aria-label="YouTube">
              <svg viewBox="0 0 24 24">
                <rect x="2" y="5" width="20" height="14" rx="4" />
                <path d="M10 9v6l5-3z" />
              </svg>
            </a>
          </div>
        </div>
        {columns.map((c) => (
          <div key={c.title}>
            <h3>{c.title}</h3>
            <ul>
              {c.links.map((l) => (
                <li key={l.label}>
                  <a href={l.href}>{l.label}</a>
                </li>
              ))}
            </ul>
          </div>
        ))}
        <div id="boglanish">
          <h3>Bog‘lanish</h3>
          <ul>
            <li><a href="#">[email@tasvir.uz]</a></li>
            <li><a href="#">[+998 __ ___ __ __]</a></li>
            <li><span style={{ opacity: 0.8 }}>[Manzil]</span></li>
          </ul>
        </div>
      </div>
      <div className="ft-bottom">
        <span>© {new Date().getFullYear()} Tasvir.uz. Barcha huquqlar himoyalangan.</span>
        <span>O‘zbekistonda ishlab chiqilgan</span>
      </div>
    </footer>
  );
}
