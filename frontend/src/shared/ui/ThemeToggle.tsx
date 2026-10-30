"use client";

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

export function ThemeToggle() {
  return (
    <button className="tv-iconbtn theme-ic" type="button" onClick={toggleTheme} aria-label="Yorug‘ / qorong‘i rejimni almashtirish">
      <svg className="moon" viewBox="0 0 24 24">
        <path d="M20 14.5A8 8 0 1 1 9.5 4a6.5 6.5 0 0 0 10.5 10.5z" />
      </svg>
      <svg className="sun" viewBox="0 0 24 24">
        <circle cx="12" cy="12" r="4" />
        <path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" />
      </svg>
    </button>
  );
}
