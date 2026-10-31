import { cookies } from "next/headers";
import { NextResponse, type NextRequest } from "next/server";
import { serverEnv } from "@/shared/config/env";
import { ACCESS_COOKIE } from "@/shared/config/session";

type Context = { params: Promise<{ id: string; kind: string }> };

/** CSV faylni uzatadi: token brauzerga chiqmasligi uchun so‘rov server orqali o‘tadi. */
export async function GET(request: NextRequest, { params }: Context) {
  const { id, kind } = await params;
  const page = `/guruhlar/${encodeURIComponent(id)}`;

  const token = (await cookies()).get(ACCESS_COOKIE)?.value;
  if (!token) {
    return NextResponse.redirect(new URL(`/kirish?keyin=${encodeURIComponent(page)}`, request.url));
  }

  let response: Response;
  try {
    response = await fetch(new URL(`/api/classroom/groups/${encodeURIComponent(id)}/export/${encodeURIComponent(kind)}`, serverEnv.apiUrl), {
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
    });
  } catch {
    return NextResponse.redirect(new URL(`${page}?xato=${encodeURIComponent("Xizmat vaqtincha ishlamayapti.")}`, request.url));
  }

  if (!response.ok) {
    return new NextResponse(null, { status: response.status === 401 ? 401 : 404 });
  }

  return new NextResponse(await response.arrayBuffer(), {
    headers: {
      "Content-Type": "text/csv; charset=utf-8",
      "Content-Disposition": `attachment; filename="${encodeURIComponent(kind)}.csv"`,
      "Cache-Control": "no-store",
    },
  });
}
