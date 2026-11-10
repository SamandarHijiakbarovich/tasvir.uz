import type { Metadata } from "next";
import Link from "next/link";
import { AuthShell } from "@/features/auth/components/AuthShell";
import { ConfirmEmailForm } from "@/features/auth/components/forms";

export const metadata: Metadata = { title: "Emailni tasdiqlaş — Tasvir.uz" };

type Props = { searchParams: Promise<{ token?: string }> };

export default async function ConfirmEmailPage({ searchParams }: Props) {
  const { token } = await searchParams;

  if (!token) {
    return (
      <AuthShell title="Havola yaroqsiz" lead="Kabinetingizdan tasdiqlaş xatini qayta söraşingiz mumkin." footer={<Link href="/kabinet">Kabinetga ötiş</Link>}>
        <span />
      </AuthShell>
    );
  }

  return (
    <AuthShell title="Emailni tasdiqlaş" lead="Bu email sizniki ekanini tasdiqlaş uçun tugmani bosing.">
      <ConfirmEmailForm token={token} />
    </AuthShell>
  );
}
