import "server-only";
import { cookies } from "next/headers";
import { serverEnv } from "@/shared/config/env";
import { ACCESS_COOKIE } from "@/shared/config/session";

/**
 * Backenddagi faylni sayt manzili orqali uzatadi: brauzer API manzilini bilmaydi, kirish tokeni esa serverda qoladi.
 * Faqat kerakli sarlavhalar o‘tkaziladi (tur, nom, hajm), qolganlari sayt sozlamalaridan olinadi.
 */
export async function proxyFile(path: string, { authed }: { authed: boolean }): Promise<Response> {
  const headers: Record<string, string> = {};
  if (authed) {
    const token = (await cookies()).get(ACCESS_COOKIE)?.value;
    if (!token) return new Response("Kirish talab qilinadi.", { status: 401 });
    headers.Authorization = `Bearer ${token}`;
  }

  let response: Response;
  try {
    response = await fetch(new URL(path, serverEnv.apiUrl), { headers, cache: "no-store" });
  } catch {
    return new Response("Xizmat vaqtinça işlamayapti.", { status: 503 });
  }

  if (!response.ok || !response.body) {
    return new Response(response.status === 404 ? "Fayl topilmadi." : "Faylni oçib bölmadi.", { status: response.status === 404 ? 404 : 502 });
  }

  const out = new Headers({ "X-Content-Type-Options": "nosniff", "Cache-Control": "private, no-store" });
  for (const name of ["content-type", "content-length", "content-disposition"]) {
    const value = response.headers.get(name);
    if (value) out.set(name, value);
  }
  return new Response(response.body, { status: 200, headers: out });
}
