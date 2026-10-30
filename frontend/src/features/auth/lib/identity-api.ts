import "server-only";
import { cache } from "react";
import { cookies, headers } from "next/headers";
import { serverEnv } from "@/shared/config/env";
import { ACCESS_COOKIE, type AuthUser } from "./session-cookies";

export type ApiFailure = {
  ok: false;
  status: number;
  code?: string;
  message: string;
  fields?: Record<string, string>;
};

export type ApiResult<T> = { ok: true; data: T } | ApiFailure;

const UNAVAILABLE = "Xizmat vaqtincha ishlamayapti. Birozdan so‘ng qayta urining.";

type RequestOptions = {
  method?: "GET" | "POST";
  body?: unknown;
  accessToken?: string;
};

/** Identity moduliga API Gateway orqali so‘rov. Tokenlar faqat serverda qoladi, brauzerga chiqmaydi. */
export async function identityRequest<T>(path: string, { method = "POST", body, accessToken }: RequestOptions = {}): Promise<ApiResult<T>> {
  const requestHeaders: Record<string, string> = { Accept: "application/json" };
  if (body !== undefined) requestHeaders["Content-Type"] = "application/json";
  if (accessToken) requestHeaders.Authorization = `Bearer ${accessToken}`;

  // So‘rovlar cheklovi Next.js serverini emas, foydalanuvchi IP manzilini hisoblashi uchun
  const clientIp = (await headers()).get("x-forwarded-for")?.split(",").pop()?.trim();
  if (clientIp) requestHeaders["X-Forwarded-For"] = clientIp;

  let response: Response;
  try {
    response = await fetch(new URL(`/api/identity/${path}`, serverEnv.apiUrl), {
      method,
      headers: requestHeaders,
      body: body === undefined ? undefined : JSON.stringify(body),
      cache: "no-store",
    });
  } catch (error) {
    console.error(`[identity] ${path} ga ulanib bo‘lmadi`, error);
    return { ok: false, status: 503, message: UNAVAILABLE };
  }

  if (response.ok) {
    const text = await response.text();
    return { ok: true, data: (text ? JSON.parse(text) : undefined) as T };
  }

  const problem = (await response.json().catch(() => null)) as { title?: string; code?: string; errors?: Record<string, string> | null } | null;
  // Kutilmagan xato matni foydalanuvchiga ko‘rsatilmaydi: biznes xatolarida har doim "code" bo‘ladi
  return {
    ok: false,
    status: response.status,
    code: problem?.code,
    message: problem?.code && problem.title ? problem.title : UNAVAILABLE,
    fields: problem?.errors ?? undefined,
  };
}

/** Joriy foydalanuvchi; bitta so‘rov davomida backend’ga bir marta murojaat qiladi. */
export const getCurrentUser = cache(async (): Promise<AuthUser | null> => {
  const accessToken = (await cookies()).get(ACCESS_COOKIE)?.value;
  if (!accessToken) return null;

  const result = await identityRequest<AuthUser>("me", { method: "GET", accessToken });
  return result.ok ? result.data : null;
});
