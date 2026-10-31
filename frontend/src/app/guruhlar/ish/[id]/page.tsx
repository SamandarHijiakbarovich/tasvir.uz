import type { Metadata } from "next";
import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { getSubmission } from "@/features/classroom/api";
import { ClassroomHeader } from "@/features/classroom/components/ClassroomHeader";
import { ReviewForm } from "@/features/classroom/components/forms";
import { formatDate } from "@/features/classroom/lib/format";
import { describeMaterial, renderMaterial, svgDataUri } from "@/features/studio/lib/render";
import { getTemplate } from "@/features/studio/lib/templates";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/studio.css";
import "@/styles/classroom.css";

export const metadata: Metadata = { title: "Topshirilgan ish — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { params: Promise<{ id: string }> };

export default async function SubmissionPage({ params }: Props) {
  const { id } = await params;
  const result = await getSubmission(id);

  if (!result.ok) {
    if (result.status === 401) redirect(`/kirish?keyin=${encodeURIComponent(`/guruhlar/ish/${id}`)}`);
    if (result.status === 404) notFound();

    return (
      <div className="tv-studio">
        <ClassroomHeader />
        <main className="tv-studio-in">
          <p className="tv-form-msg err" role="alert">
            {result.message}
          </p>
        </main>
      </div>
    );
  }

  const { summary, content, rubric, maxCriterionScore, canReview, groupId, groupName } = result.data;
  const doc = { templateKey: summary.templateKey, title: summary.title, subject: summary.subject, grade: summary.grade, content };
  const review = summary.review;
  const isCurrent = review?.version === summary.version;

  return (
    <div className="tv-studio">
      <ClassroomHeader />
      <main className="tv-studio-in">
        <p className="tv-breadcrumb" style={{ margin: 0 }}>
          <Link href={`/guruhlar/${groupId}${canReview ? "?b=ishlar" : ""}`}>{groupName}</Link>
        </p>
        <div>
          <h1 className="tv-h2">{summary.title}</h1>
          <p className="tv-muted" style={{ margin: "var(--space-2) 0 0" }}>
            {[summary.studentName, getTemplate(summary.templateKey)?.name, summary.subject, summary.grade ? `${summary.grade}-sinf` : null, `${summary.version}-versiya`, formatDate(summary.submittedAt)]
              .filter(Boolean)
              .join(" · ")}
          </p>
        </div>

        <div className="tv-review-grid">
          <div className="tv-print-area">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={svgDataUri(renderMaterial(doc).svg)} alt={describeMaterial(doc)} />
          </div>

          <section className="tv-card" aria-labelledby="baho-t">
            <h2 className="tv-h3" id="baho-t">
              Baholash
            </h2>
            {canReview ? (
              <ReviewForm key={summary.version} submissionId={id} rubric={rubric} maxScore={maxCriterionScore} review={review} isCurrent={isCurrent} />
            ) : review ? (
              <div className="tv-form">
                {!isCurrent && <p className="tv-form-msg ok">Bu baho oldingi versiyaga qo‘yilgan. Yangi versiya hali baholanmagan.</p>}
                <p className="tv-rubric-total">
                  Jami: <strong>{review.totalScore}</strong> / {review.maxScore}
                </p>
                <ul className="tv-score-list">
                  {rubric.map((criterion) => (
                    <li key={criterion.key}>
                      <span>{criterion.title}</span>
                      <strong>
                        {review.scores[criterion.key]} / {maxCriterionScore}
                      </strong>
                    </li>
                  ))}
                </ul>
                {review.comment && (
                  <blockquote className="tv-review-comment">
                    <strong>O‘qituvchi izohi:</strong> {review.comment}
                  </blockquote>
                )}
              </div>
            ) : (
              <p className="tv-muted">O‘qituvchi hali baholamagan. Baho qo‘yilgach, shu yerda ko‘rinadi.</p>
            )}
          </section>
        </div>
      </main>
    </div>
  );
}
