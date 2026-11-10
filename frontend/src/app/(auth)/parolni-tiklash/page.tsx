import type { Metadata } from "next";
import Link from "next/link";
import { AuthShell } from "@/features/auth/components/AuthShell";
import { ForgotPasswordForm } from "@/features/auth/components/forms";

export const metadata: Metadata = { title: "Parolni tiklaş — Tasvir.uz" };

export default function ForgotPasswordPage() {
  return (
    <AuthShell
      title="Parolni tiklaş"
      lead="Emailingizni kiriting, yangi parol örnatiş havolasini yuboramiz."
      footer={<Link href="/kirish">Kiriş sahifasiga qaytiş</Link>}
    >
      <ForgotPasswordForm />
    </AuthShell>
  );
}
