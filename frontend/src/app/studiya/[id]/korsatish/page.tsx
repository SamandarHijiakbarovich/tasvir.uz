import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import { getMaterial } from "@/features/studio/api/studio-api";
import { PresentView } from "@/features/studio/components/PresentView";
import "@/styles/studio.css";

export const metadata: Metadata = { title: "Darsda ko‘rsatish — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { params: Promise<{ id: string }> };

export default async function PresentPage({ params }: Props) {
  const { id } = await params;
  const result = await getMaterial(id);

  if (!result.ok) {
    if (result.status === 401) redirect(`/kirish?keyin=${encodeURIComponent(`/studiya/${id}/korsatish`)}`);
    if (result.status === 404) notFound();
    redirect("/studiya");
  }

  return <PresentView material={result.data} />;
}
