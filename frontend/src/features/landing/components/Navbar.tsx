"use client";

import { useState } from "react";
import { Logo } from "@/shared/ui/Logo";

const links = [
  { href: "#imkoniyatlar", label: "Imkoniyatlar" },
  { href: "#qanday", label: "Qanday ishlaydi" },
  { href: "#narxlar", label: "Narxlar" },
  { href: "#tadqiqot", label: "Tadqiqot" },
];

function toggleTheme() {
  const root = document.documentElement;
  const next = root.getAttribute("data-theme") === "dark" ? "light" : "dark";
  root.setAttribute("data-theme", next);
  try {
    localStorage.setItem("tasvir-theme", next);
  } catch {
    // brauzer xotirasi yopiq bo‘lsa — faqat shu sessiya uchun
  }
}

export function Navbar() {
  const [open, setOpen] = useState(false);

  return (
    <header className={`tv-nav${open ? " open" : ""}`}>
      <nav className="tv-nav-in" aria-label="Asosiy menyu">
        <Logo />
        <ul className="tv-nav-links" id="menu">
          {links.map((l) => (
            <li key={l.href}>
              <a href={l.href} onClick={() => setOpen(false)}>
                {l.label}
              </a>
            </li>
          ))}
        </ul>
        <div className="tv-nav-act">
          <button className="tv-iconbtn theme-ic" type="button" onClick={toggleTheme} aria-label="Yorug‘ / qorong‘i rejimni almashtirish">
            <svg className="moon" viewBox="0 0 24 24">
              <path d="M20 14.5A8 8 0 1 1 9.5 4a6.5 6.5 0 0 0 10.5 10.5z" />
            </svg>
            <svg className="sun" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="4" />
              <path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" />
            </svg>
          </button>
          <a className="tv-btn tv-btn-text tv-login" href="#kirish">
            Kirish
          </a>
          <a className="tv-btn tv-btn-primary tv-btn-sm" href="#boshlash">
            Bepul boshlash
          </a>
          <button
            className="tv-iconbtn tv-burger"
            type="button"
            aria-label={open ? "Menyuni yopish" : "Menyuni ochish"}
            aria-expanded={open}
            aria-controls="menu"
            onClick={() => setOpen((o) => !o)}
          >
            <svg viewBox="0 0 24 24">
              <path d="M4 7h16M4 12h16M4 17h16" />
            </svg>
          </button>
        </div>
      </nav>
    </header>
  );
}
