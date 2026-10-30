import type { Metadata, Viewport } from "next";
import { Bricolage_Grotesque, Caveat, Onest } from "next/font/google";
import "@/styles/tokens.css";
import "@/styles/components.css";
import "./globals.css";

const display = Bricolage_Grotesque({ subsets: ["latin"], weight: ["600", "700", "800"], variable: "--ff-display" });
const sans = Onest({ subsets: ["latin"], weight: ["400", "500", "600", "700"], variable: "--ff-sans" });
const hand = Caveat({ subsets: ["latin"], weight: ["600", "700"], variable: "--ff-hand" });

export const metadata: Metadata = {
  title: "Tasvir.uz — murakkabni oddiy, zerikarlini qiziqarli qiling",
  description:
    "Tasvir.uz o‘qituvchilarga o‘quv materialini infografika, sxema, diagramma va aqliy xaritaga aylantirishni o‘rgatadi va buni oson qiladi.",
  icons: { icon: "/brand/tasvir-mark.svg" },
};

export const viewport: Viewport = {
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#fbfaf7" },
    { media: "(prefers-color-scheme: dark)", color: "#12121c" },
  ],
};

// Sahifa chizilishidan oldin mavzuni o‘rnatadi (yorug‘/qorong‘i "miltillash"ining oldini oladi).
const themeInit = `try{var t=localStorage.getItem("tasvir-theme");if(t!=="light"&&t!=="dark"){t=matchMedia("(prefers-color-scheme: dark)").matches?"dark":"light"}document.documentElement.setAttribute("data-theme",t)}catch(e){}`;

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="uz" className={`${display.variable} ${sans.variable} ${hand.variable}`} suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: themeInit }} />
      </head>
      <body>{children}</body>
    </html>
  );
}
