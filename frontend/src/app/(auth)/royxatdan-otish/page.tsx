import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { AuthShell } from "@/features/auth/components/AuthShell";
import { RegisterForm } from "@/features/auth/components/forms";
import { getCurrentUser } from "@/features/auth/lib/identity-api";
import { safeNextPath } from "@/features/auth/lib/session-cookies";

export const metadata: Metadata = { title: "Ro‘yxatdan o‘tish — Tasvir.uz" };

type Props = { searchParams: Promise<{ keyin?: string }> };

export default async function RegisterPage({ searchParams }: Props) {
  const { keyin } = await searchParams;
  const next = safeNextPath(keyin);

  if (await getCurrentUser()) redirect(next);

  return (
    <AuthShell
      title="Hisob yarating"
      lead="Birinchi vizual darsingizga bir qadam qoldi. Karta talab qilinmaydi."
      footer={
        <>
          Hisobingiz bormi? <Link href={`/kirish${keyin ? `?keyin=${encodeURIComponent(next)}` : ""}`}>Kirish</Link>
        </>
      }
    >
      <RegisterForm next={next} />
    </AuthShell>
  );
}
