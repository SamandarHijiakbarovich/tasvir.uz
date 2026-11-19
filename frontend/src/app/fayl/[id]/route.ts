import { proxyFile } from "@/features/library/file-proxy";

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/** Kabinetdagi faylni egasiga ko‘rsatish. */
export async function GET(_: Request, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  if (!GUID.test(id)) return new Response("Fayl topilmadi.", { status: 404 });
  return proxyFile(`/api/studio/library/${id}/file`, { authed: true });
}
