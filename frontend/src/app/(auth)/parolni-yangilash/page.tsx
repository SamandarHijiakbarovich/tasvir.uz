import type { Metadata } from "next";
import Link from "next/link";
import { AuthShell } from "@/features/auth/components/AuthShell";
import { ResetPasswordForm } from "@/features/auth/components/forms";

export const metadata: Metadata = { title: "Yangi parol — Tasvir.uz" };

type Props = { searchParams: Promise<{ token?: string }> };

export default async function ResetPasswordPage({ searchParams }: Props) {
  const { token } = await searchParams;

  if (!token) {
    return (
      <AuthShell title="Havola yaroqsiz" lead="Parolni tiklaş uçun yangi havola sörang." footer={<Link href="/kirish">Kiriş sahifasiga qaytiş</Link>}>
        <div className="tv-form">
          <Link className="tv-btn tv-btn-primary tv-btn-block" href="/parolni-tiklash">
            Yangi havola söraş
          </Link>
        </div>
      </AuthShell>
    );
  }

  return (
    <AuthShell
      title="Yangi parol"
      lead="Yangi parol örnating. Saqlangaç, barça qurilmalardan çiqib ketasiz."
      footer={<Link href="/parolni-tiklash">Havola işlamayaptimi? Yangisini sörang</Link>}
    >
      <ResetPasswordForm token={token} />
    </AuthShell>
  );
}
