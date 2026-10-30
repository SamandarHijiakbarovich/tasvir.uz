import { NextResponse, type NextRequest } from "next/server";
import { serverEnv } from "@/shared/config/env";
import { ACCESS_COOKIE, REFRESH_COOKIE, applySession, clearSession, type AuthSession } from "@/features/auth/lib/session-cookies";

const PROTECTED_PREFIXES = ["/kabinet"];

function isProtected(pathname: string) {
  return PROTECTED_PREFIXES.some((prefix) => pathname === prefix || pathname.startsWith(`${prefix}/`));
}

function redirectToLogin(request: NextRequest) {
  const url = new URL("/kirish", request.url);
  url.searchParams.set("keyin", request.nextUrl.pathname + request.nextUrl.search);
  return NextResponse.redirect(url);
}

/** Access token muddati tugaganda uni foydalanuvchiga sezdirmasdan yangilaydi va himoyalangan sahifalarni qo‘riqlaydi. */
export async function proxy(request: NextRequest) {
  const accessToken = request.cookies.get(ACCESS_COOKIE)?.value;
  const refreshToken = request.cookies.get(REFRESH_COOKIE)?.value;

  if (accessToken) return NextResponse.next();

  if (!refreshToken) {
    return isProtected(request.nextUrl.pathname) ? redirectToLogin(request) : NextResponse.next();
  }

  let response: Response;
  try {
    response = await fetch(new URL("/api/identity/refresh", serverEnv.apiUrl), {
      method: "POST",
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      body: JSON.stringify({ refreshToken }),
      cache: "no-store",
    });
  } catch {
    // Backend vaqtincha ishlamayapti: sessiyani o‘chirmaymiz, keyingi so‘rovda yana urinamiz
    return NextResponse.next();
  }

  if (response.ok) {
    const session = (await response.json()) as AuthSession;

    // Yangi tokenlar shu so‘rovning o‘zida ham ko‘rinishi kerak (sahifa server tomonda chiziladi)
    request.cookies.set(ACCESS_COOKIE, session.accessToken);
    request.cookies.set(REFRESH_COOKIE, session.refreshToken);
    const headers = new Headers(request.headers);
    headers.set("cookie", request.cookies.toString());

    const next = NextResponse.next({ request: { headers } });
    applySession(next.cookies, session);
    return next;
  }

  if (response.status === 401) {
    const next = isProtected(request.nextUrl.pathname) ? redirectToLogin(request) : NextResponse.next();
    clearSession(next.cookies);
    return next;
  }

  return NextResponse.next();
}

export const config = {
  // Statik fayllar va brend rasmlari uchun ishlamaydi
  matcher: ["/((?!_next/static|_next/image|brand/|favicon.ico).*)"],
};
