import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  reactStrictMode: true,
  poweredByHeader: false,
  // Faqat "next dev" uchun: localhost va 127.0.0.1 alohida cookie ishlatadi, ikki hisobni yonma-yon sinash mumkin
  allowedDevOrigins: ["127.0.0.1"],
  experimental: {
    // Kabinetga fayl yuklash (4 MB gacha, Vercel so‘rov chegarasi 4,5 MB) server action orqali o‘tadi
    serverActions: { bodySizeLimit: "5mb" },
  },
  async headers() {
    return [
      {
        // /fayl/* — kabinetdagi PDF va rasmlar: ular shu saytning o‘zida (iframe) ko‘rsatiladi
        source: "/:path((?!fayl/).*)",
        headers: [{ key: "X-Frame-Options", value: "DENY" }],
      },
      {
        source: "/fayl/:path*",
        // Bu yerda faqat PDF, PowerPoint va rasm beriladi (backend turini mazmunidan tekshiradi), nosniff pastda
        headers: [{ key: "X-Frame-Options", value: "SAMEORIGIN" }],
      },
      {
        source: "/:path*",
        headers: [
          { key: "X-Content-Type-Options", value: "nosniff" },
          { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
          { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=()" },
        ],
      },
      {
        // Havolada maxfiy token bor — boshqa saytlarga Referer orqali chiqmasin
        source: "/(parolni-yangilash|email-tasdiqlash)",
        headers: [{ key: "Referrer-Policy", value: "no-referrer" }],
      },
    ];
  },
};

export default nextConfig;
