import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { getShared } from "@/features/library/api";
import { ResourceViewer } from "@/features/library/components/ResourceViewer";
import { sharedFileUrl } from "@/features/library/format";
import { PresentView } from "@/features/studio/components/PresentView";
import "@/styles/auth.css";
import "@/styles/studio.css";
import "@/styles/library.css";

// Ulashilgan material qidiruv tizimlariga chiqmaydi: havolani faqat egasi bergan odamlar ko‘radi
export const metadata: Metadata = { title: "Ulaşilgan material — Tasvir.uz", robots: { index: false, follow: false } };

const TOKEN = /^[A-Za-z0-9_-]{22}$/;

type Props = { params: Promise<{ token: string }> };

export default async function SharedPage({ params }: Props) {
  const { token } = await params;
  if (!TOKEN.test(token)) notFound();

  const result = await getShared(token);
  if (!result.ok) {
    if (result.status === 404) notFound();
    return (
      <main className="tv-auth-main" style={{ padding: "var(--space-8) var(--space-4)" }}>
        <p className="tv-form-msg err" role="alert">
          {result.message}
        </p>
        <p>
          <Link href="/">Bosh sahifa</Link>
        </p>
      </main>
    );
  }

  const { material, item } = result.data;
  if (material) return <PresentView material={material} closeHref={null} />;
  if (item) return <ResourceViewer item={item} fileUrl={item.kind === "file" ? sharedFileUrl(token) : null} shareToken={token} />;
  notFound();
}
