import "server-only";
import { serverEnv } from "@/shared/config/env";

type GetOptions = {
  /** Necha soniyada keshni yangilash (ISR). */
  revalidate?: number;
};

/**
 * API Gateway ga GET so‘rov. Xatoda null qaytaradi — sahifa backend ishlamasa ham ochilishi kerak,
 * tegishli bo‘lim esa yashiriladi.
 */
export async function apiGet<T>(path: string, { revalidate = 60 }: GetOptions = {}): Promise<T | null> {
  const url = new URL(path, serverEnv.apiUrl);
  try {
    const res = await fetch(url, { next: { revalidate }, headers: { Accept: "application/json" } });
    if (!res.ok) {
      console.error(`[api] ${url} → ${res.status}`);
      return null;
    }
    return (await res.json()) as T;
  } catch (error) {
    console.error(`[api] ${url} ga ulanib bo‘lmadi`, error);
    return null;
  }
}
