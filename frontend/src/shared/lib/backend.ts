import "server-only";
import { cookies, headers } from "next/headers";
import { serverEnv } from "@/shared/config/env";
import { ACCESS_COOKIE } from "@/shared/config/session";

export type ApiFailure = {
  ok: false;
  status: number;
  code?: string;
  message: string;
  fields?: Record<string, string>;
};

export type ApiResult<T> = { ok: true; data: T } | ApiFailure;

const UNAVAILABLE = "Xizmat vaqtincha ishlamayapti. Birozdan so‘ng qayta urining.";
const SESSION_EXPIRED = "Sessiya tugagan. Qayta kiring.";

type RequestOptions = {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
  accessToken?: string;
};

/** Backend’ga API Gateway orqali so‘rov. Tokenlar faqat serverda qoladi, brauzerga chiqmaydi. */
export async function backendRequest<T>(path: string, { method = "POST", body, accessToken }: RequestOptions = {}): Promise<ApiResult<T>> {
  const requestHeaders: Record<string, string> = { Accept: "application/json" };
  if (body !== undefined) requestHeaders["Content-Type"] = "application/json";
  if (accessToken) requestHeaders.Authorization = `Bearer ${accessToken}`;

  // So‘rovlar cheklovi Next.js serverini emas, foydalanuvchi IP manzilini hisoblashi uchun
  const clientIp = (await headers()).get("x-forwarded-for")?.split(",").pop()?.trim();
  if (clientIp) requestHeaders["X-Forwarded-For"] = clientIp;

  let response: Response;
  try {
    response = await fetch(new URL(path, serverEnv.apiUrl), {
      method,
      headers: requestHeaders,
      body: body === undefined ? undefined : JSON.stringify(body),
      cache: "no-store",
    });
  } catch (error) {
    console.error(`[backend] ${path} ga ulanib bo‘lmadi`, error);
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
    message: problem?.code && problem.title ? problem.title : response.status === 401 ? SESSION_EXPIRED : UNAVAILABLE,
    fields: problem?.errors ?? undefined,
  };
}

/** Kirgan foydalanuvchi nomidan so‘rov. */
export async function authedRequest<T>(path: string, options: Omit<RequestOptions, "accessToken"> = {}): Promise<ApiResult<T>> {
  const accessToken = (await cookies()).get(ACCESS_COOKIE)?.value;
  if (!accessToken) return { ok: false, status: 401, message: SESSION_EXPIRED };
  return backendRequest<T>(path, { ...options, accessToken });
}
