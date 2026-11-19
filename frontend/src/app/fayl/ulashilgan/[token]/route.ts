import { proxyFile } from "@/features/library/file-proxy";

const TOKEN = /^[A-Za-z0-9_-]{22}$/;

/** Ochiq havola orqali ulashilgan fayl (kirish shart emas). */
export async function GET(_: Request, { params }: { params: Promise<{ token: string }> }) {
  const { token } = await params;
  if (!TOKEN.test(token)) return new Response("Fayl topilmadi.", { status: 404 });
  return proxyFile(`/api/studio/shared/${token}/file`, { authed: false });
}
