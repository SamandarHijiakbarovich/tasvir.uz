import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { getMyWork } from "@/features/academy/api/academy-api";
import { AcademyHeader } from "@/features/academy/components/AcademyHeader";
import { DiagramDownload } from "@/features/academy/components/DiagramDownload";
import { ProcessDiagram } from "@/features/academy/components/ProcessDiagram";
import "@/styles/auth.css";
import "@/styles/academy.css";

export const metadata: Metadata = { title: "Mening ishlarim — Tasvir.uz", robots: { index: false, follow: false } };

const dateFormat = new Intl.DateTimeFormat("uz-UZ", { dateStyle: "medium", timeZone: "Asia/Tashkent" });

export default async function MyWorkPage() {
  const result = await getMyWork();

  if (!result.ok && result.status === 401) redirect("/kirish?keyin=/akademiya/ishlarim");

  return (
    <div className="tv-academy">
      <AcademyHeader />
      <main className="tv-academy-in">
        <div>
          <h1 className="tv-h2">Mening ishlarim</h1>
          <p className="tv-lead" style={{ marginTop: 8 }}>
            Darslarda tayyorlagan javoblaringiz va sxemalaringiz shu yerda saqlanadi.
          </p>
        </div>

        {!result.ok && (
          <p className="tv-form-msg err" role="alert">
            {result.message}
          </p>
        )}

        {result.ok && result.data.length === 0 && (
          <section className="tv-card tv-work-empty">
            <p>Hali ishlaringiz yo‘q. Birinchi amaliy topshiriq birinchi darsning oxirida.</p>
            <Link className="tv-btn tv-btn-primary" href="/akademiya">
              Darslarga o‘tish
            </Link>
          </section>
        )}

        {result.ok &&
          result.data.map((work) => {
            // Backend vaqtni UTC da yuboradi
            const completedAt = new Date(/Z$/.test(work.completedAt) ? work.completedAt : `${work.completedAt}Z`);
            return (
              <article key={work.stepId} className="tv-card tv-work">
                <header>
                  <h2 className="tv-h3">{work.lessonTitle}</h2>
                  <p className="tv-lesson-meta">
                    {work.stepTitle} · {dateFormat.format(completedAt)}
                  </p>
                </header>
                {work.mode === "process" && work.items ? (
                  <>
                    <ProcessDiagram items={work.items} />
                    <DiagramDownload items={work.items} name={`sxema-${work.lessonSlug}`} />
                  </>
                ) : (
                  <p className="tv-work-text">{work.text}</p>
                )}
              </article>
            );
          })}
      </main>
    </div>
  );
}
