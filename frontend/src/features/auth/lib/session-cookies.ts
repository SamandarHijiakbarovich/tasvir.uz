import { ACCESS_COOKIE, SESSION_FLAG_COOKIE } from "@/shared/config/session";

export { ACCESS_COOKIE };

/** Sessiya cookie’lari. Fayl proxy va server kodida ishlatiladi, shuning uchun next/headers’ga bog‘lanmagan. */
export const REFRESH_COOKIE = "tasvir_rt";

export type AuthUser = {
  id: string;
  fullName: string;
  email: string;
  emailConfirmed: boolean;
  createdAt: string;
  privacyAcceptedAt?: string | null;
};

export type AuthSession = {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  isPersistent: boolean;
  user: AuthUser;
};

type CookieJar = {
  set(name: string, value: string, options: Record<string, unknown>): unknown;
  delete(name: string): unknown;
};

const base = {
  path: "/",
  sameSite: "lax",
  secure: process.env.NODE_ENV === "production",
} as const;

// Vaqt UTC; backend DateTime’ni "Z" siz qaytarishi mumkin
const toUtc = (value: string) => new Date(/(Z|[+-]\d\d:\d\d)$/.test(value) ? value : `${value}Z`);

const secondsUntil = (value: string) => Math.max(0, Math.floor((toUtc(value).getTime() - Date.now()) / 1000));

export function applySession(jar: CookieJar, session: AuthSession) {
  jar.set(ACCESS_COOKIE, session.accessToken, {
    ...base,
    httpOnly: true,
    maxAge: secondsUntil(session.accessTokenExpiresAt),
  });

  // "Meni eslab qolish" belgilanmagan bo‘lsa, cookie brauzer yopilguncha yashaydi
  const lifetime = session.isPersistent ? { maxAge: secondsUntil(session.refreshTokenExpiresAt) } : {};
  jar.set(REFRESH_COOKIE, session.refreshToken, { ...base, httpOnly: true, ...lifetime });
  jar.set(SESSION_FLAG_COOKIE, "1", { ...base, httpOnly: false, ...lifetime });
}

export function clearSession(jar: CookieJar) {
  jar.delete(ACCESS_COOKIE);
  jar.delete(REFRESH_COOKIE);
  jar.delete(SESSION_FLAG_COOKIE);
}

/** Tashqi saytga yo‘naltirishning oldini oladi: faqat shu saytning nisbiy yo‘llari. */
export function safeNextPath(value: unknown, fallback = "/kabinet"): string {
  if (typeof value !== "string" || !value.startsWith("/") || value.startsWith("//") || value.includes("\\")) {
    return fallback;
  }
  return value;
}
