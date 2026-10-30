import Link from "next/link";

export function Logo() {
  return (
    <Link className="tv-logo" href="/" aria-label="Tasvir.uz bosh sahifa">
      <svg viewBox="0 0 64 64" aria-hidden="true">
        <path
          className="s"
          d="M6 20v-8a6 6 0 0 1 6-6h8M44 6h8a6 6 0 0 1 6 6v8M58 44v8a6 6 0 0 1-6 6h-8M20 58h-8a6 6 0 0 1-6-6v-8"
          fill="none"
          strokeWidth="5"
          strokeLinecap="round"
        />
        <path
          className="s"
          d="M12 32c5.5-9 12.5-13.5 20-13.5S46.5 23 52 32c-5.5 9-12.5 13.5-20 13.5S17.5 41 12 32z"
          fill="none"
          strokeWidth="4"
          strokeLinejoin="round"
        />
        <circle cx="32" cy="32" r="8" fill="#ff6b57" />
        <path d="M32 32v-8a8 8 0 0 1 8 8z" fill="#ffc93c" />
      </svg>
      <span>
        Tasvir<span className="uz">.uz</span>
      </span>
    </Link>
  );
}
