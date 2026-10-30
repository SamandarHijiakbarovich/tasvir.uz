import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import { getMaterial } from "@/features/studio/api/studio-api";
import { MaterialEditor } from "@/features/studio/components/MaterialEditor";
import { StudioHeader } from "@/features/studio/components/StudioHeader";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/studio.css";

export const metadata: Metadata = { title: "Material — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { params: Promise<{ id: string }> };

export default async function MaterialPage({ params }: Props) {
  const { id } = await params;
  const result = await getMaterial(id);

  if (!result.ok) {
    if (result.status === 401) redirect(`/kirish?keyin=${encodeURIComponent(`/studiya/${id}`)}`);
    if (result.status === 404) notFound();

    return (
      <div className="tv-studio">
        <StudioHeader />
        <main className="tv-studio-in">
          <p className="tv-form-msg err" role="alert">
            {result.message}
          </p>
        </main>
      </div>
    );
  }

  return (
    <div className="tv-studio">
      <StudioHeader />
      <main className="tv-studio-in">
        <h1 className="tv-sr">{result.data.title}: tahrirlash</h1>
        {/* Kalit: nusxa ochilganda muharrir holati noldan boshlanadi */}
        <MaterialEditor key={result.data.id} material={result.data} />
      </main>
    </div>
  );
}
