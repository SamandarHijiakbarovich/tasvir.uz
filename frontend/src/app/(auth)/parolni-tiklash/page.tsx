import type { Metadata } from "next";
import Link from "next/link";
import { AuthShell } from "@/features/auth/components/AuthShell";
import { ForgotPasswordForm } from "@/features/auth/components/forms";

export const metadata: Metadata = { title: "Parolni tiklash — Tasvir.uz" };

export default function ForgotPasswordPage() {
  return (
    <AuthShell
      title="Parolni tiklash"
      lead="Emailingizni kiriting, yangi parol o‘rnatish havolasini yuboramiz."
      footer={<Link href="/kirish">Kirish sahifasiga qaytish</Link>}
    >
      <ForgotPasswordForm />
    </AuthShell>
  );
}
