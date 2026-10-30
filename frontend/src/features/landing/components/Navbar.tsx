"use client";

import Link from "next/link";
import { useState, useSyncExternalStore } from "react";
import { SESSION_FLAG_COOKIE } from "@/shared/config/session";
import { Logo } from "@/shared/ui/Logo";
import { ThemeToggle } from "@/shared/ui/ThemeToggle";

const links = [
  { href: "#imkoniyatlar", label: "Imkoniyatlar" },
  { href: "#qanday", label: "Qanday ishlaydi" },
  { href: "#narxlar", label: "Narxlar" },
  { href: "#tadqiqot", label: "Tadqiqot" },
];

// Sahifa keshlanadi, shuning uchun kirganlik holati brauzerda o‘qiladi
const subscribe = () => () => {};
const hasSession = () => document.cookie.split("; ").some((c) => c.startsWith(`${SESSION_FLAG_COOKIE}=`));
const noSessionOnServer = () => false;

export function Navbar() {
  const [open, setOpen] = useState(false);
  const signedIn = useSyncExternalStore(subscribe, hasSession, noSessionOnServer);

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
          {signedIn ? (
            <li className="tv-nav-auth">
              <Link href="/kabinet">Kabinet</Link>
            </li>
          ) : (
            <>
              <li className="tv-nav-auth">
                <Link href="/kirish">Kirish</Link>
              </li>
              <li className="tv-nav-auth">
                <Link href="/royxatdan-otish">Bepul boshlash</Link>
              </li>
            </>
          )}
        </ul>
        <div className="tv-nav-act">
          <ThemeToggle />
          {signedIn ? (
            <Link className="tv-btn tv-btn-primary tv-btn-sm" href="/kabinet">
              Kabinet
            </Link>
          ) : (
            <>
              <Link className="tv-btn tv-btn-text tv-login" href="/kirish">
                Kirish
              </Link>
              <Link className="tv-btn tv-btn-primary tv-btn-sm" href="/royxatdan-otish">
                Bepul boshlash
              </Link>
            </>
          )}
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
