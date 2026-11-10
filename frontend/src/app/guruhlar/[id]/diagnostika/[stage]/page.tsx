import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { getDiagnosticTest } from "@/features/classroom/api";
import { ClassroomHeader } from "@/features/classroom/components/ClassroomHeader";
import { DiagnosticTestForm } from "@/features/classroom/components/forms";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/studio.css";
import "@/styles/classroom.css";

export const metadata: Metadata = { title: "Diagnostika testi — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { params: Promise<{ id: string; stage: string }> };

export default async function DiagnosticTestPage({ params }: Props) {
  const { id, stage } = await params;
  const groupPage = `/guruhlar/${encodeURIComponent(id)}`;
  const test = await getDiagnosticTest(id, stage);

  if (!test.ok && test.status === 401) redirect(`/kirish?keyin=${encodeURIComponent(`${groupPage}/diagnostika/${stage}`)}`);

  return (
    <div className="tv-studio">
      <ClassroomHeader />
      <main className="tv-studio-in">
        <p className="tv-breadcrumb" style={{ margin: 0 }}>
          <Link href={groupPage}>{test.ok ? test.data.groupName : "Guruh"}</Link>
        </p>

        {test.ok ? (
          <>
            <div>
              <h1 className="tv-h2">{test.data.title}</h1>
              <p className="tv-muted" style={{ margin: "var(--space-2) 0 0" }}>
                {test.data.questions.length} ta savol. Har bir savolga bitta javob tanlang. Natija baho uçun emas: kurs boşida va oxirida qança özgarganingizni
                körsatadi, şuning uçun taxmin qilmasdan özingiz bilganini belgilang.
              </p>
            </div>
            <section className="tv-card tv-panel">
              <DiagnosticTestForm groupId={id} test={test.data} />
            </section>
          </>
        ) : (
          <section className="tv-card tv-panel">
            <p className="tv-form-msg err" role="alert">
              {test.message}
            </p>
            <Link className="tv-btn tv-btn-ghost" href={groupPage}>
              Guruhga qaytiş
            </Link>
          </section>
        )}
      </main>
    </div>
  );
}
