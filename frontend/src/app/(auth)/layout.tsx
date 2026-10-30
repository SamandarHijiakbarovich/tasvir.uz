import type { Metadata } from "next";
import "@/styles/auth.css";

// Kirish sahifalari qidiruv tizimlariga kerak emas; maxfiy havolali sahifalar esa indekslanmasligi shart
export const metadata: Metadata = { robots: { index: false, follow: false } };

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return children;
}
