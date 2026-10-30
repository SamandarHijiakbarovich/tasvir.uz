import type { Metadata } from "next";
import Link from "next/link";
import { AuthShell } from "@/features/auth/components/AuthShell";
import { ConfirmEmailForm } from "@/features/auth/components/forms";

export const metadata: Metadata = { title: "Emailni tasdiqlash — Tasvir.uz" };

type Props = { searchParams: Promise<{ token?: string }> };

export default async function ConfirmEmailPage({ searchParams }: Props) {
  const { token } = await searchParams;

  if (!token) {
    return (
      <AuthShell title="Havola yaroqsiz" lead="Kabinetingizdan tasdiqlash xatini qayta so‘rashingiz mumkin." footer={<Link href="/kabinet">Kabinetga o‘tish</Link>}>
        <span />
      </AuthShell>
    );
  }

  return (
    <AuthShell title="Emailni tasdiqlash" lead="Bu email sizniki ekanini tasdiqlash uchun tugmani bosing.">
      <ConfirmEmailForm token={token} />
    </AuthShell>
  );
}
