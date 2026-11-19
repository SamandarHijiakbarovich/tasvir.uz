import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import { getLibrary } from "@/features/library/api";
import { ResourceViewer } from "@/features/library/components/ResourceViewer";
import { ownerFileUrl } from "@/features/library/format";
import "@/styles/studio.css";
import "@/styles/library.css";

export const metadata: Metadata = { title: "Darsda körsatiş — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { params: Promise<{ id: string }> };

export default async function LibraryPresentPage({ params }: Props) {
  const { id } = await params;
  const library = await getLibrary();
  if (!library.ok) {
    if (library.status === 401) redirect(`/kirish?keyin=${encodeURIComponent(`/kabinet/materiallar/${id}`)}`);
    redirect(`/kabinet/materiallar?xato=${encodeURIComponent(library.message)}`);
  }

  const item = library.data.items.find((i) => i.id === id);
  if (!item) notFound();

  return (
    <ResourceViewer
      item={item}
      fileUrl={item.kind === "file" ? ownerFileUrl(item.id) : null}
      shareToken={item.shareToken}
      closeHref="/kabinet/materiallar"
    />
  );
}
