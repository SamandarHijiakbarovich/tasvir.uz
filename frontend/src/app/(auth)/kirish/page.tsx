import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { AuthShell } from "@/features/auth/components/AuthShell";
import { LoginForm } from "@/features/auth/components/forms";
import { getCurrentUser } from "@/features/auth/lib/identity-api";
import { safeNextPath } from "@/features/auth/lib/session-cookies";

export const metadata: Metadata = { title: "Kiriş — Tasvir.uz" };

type Props = { searchParams: Promise<{ keyin?: string; parol?: string }> };

export default async function LoginPage({ searchParams }: Props) {
  const { keyin, parol } = await searchParams;
  const next = safeNextPath(keyin);

  if (await getCurrentUser()) redirect(next);

  return (
    <AuthShell
      title="Xuş kelibsiz"
      lead="Hisobingizga kiring va vizual darslaringizni davom ettiring."
      footer={
        <>
          Hisobingiz yöqmi? <Link href={`/royxatdan-otish${keyin ? `?keyin=${encodeURIComponent(next)}` : ""}`}>Röyxatdan öting</Link>
        </>
      }
    >
      {parol === "yangilandi" && (
        <p className="tv-form-msg ok" role="status" style={{ marginTop: 24, marginBottom: -8 }}>
          Parol yangilandi. Yangi parol bilan kiring.
        </p>
      )}
      <LoginForm next={next} />
    </AuthShell>
  );
}
