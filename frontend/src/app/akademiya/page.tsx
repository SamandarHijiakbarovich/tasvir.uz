import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { startLessonAction } from "@/features/academy/actions";
import { getCourse } from "@/features/academy/api/academy-api";
import type { LessonSummary } from "@/features/academy/api/types";
import { AcademyHeader } from "@/features/academy/components/AcademyHeader";
import "@/styles/auth.css";
import "@/styles/academy.css";

export const metadata: Metadata = { title: "Akademiya — Tasvir.uz", robots: { index: false, follow: false } };

const statusText: Record<LessonSummary["status"], string> = {
  locked: "Oldingi darsdan so‘ng ochiladi",
  available: "Ochiq",
  inProgress: "Boshlangan",
  completed: "Yakunlangan",
};

const actionText: Partial<Record<LessonSummary["status"], string>> = {
  available: "Darsni boshlash",
  inProgress: "Davom ettirish",
  completed: "Qayta ko‘rish",
};

export default async function AcademyPage() {
  const result = await getCourse();

  if (!result.ok) {
    if (result.status === 401) redirect("/kirish?keyin=/akademiya");
    return (
      <div className="tv-academy">
        <AcademyHeader />
        <main className="tv-academy-in">
          <h1 className="tv-h2">Akademiya</h1>
          <p className="tv-form-msg err" role="alert">
            {result.message}
          </p>
        </main>
      </div>
    );
  }

  const course = result.data;
  const percent = Math.round((course.completedLessons / course.totalLessons) * 100);

  return (
    <div className="tv-academy">
      <AcademyHeader />
      <main className="tv-academy-in">
        <div>
          <h1 className="tv-h2">{course.title}</h1>
          <p className="tv-lead" style={{ marginTop: 8 }}>
            {course.description}
          </p>
        </div>

        <div className="tv-lesson-progress">
          <span>
            {course.completedLessons} / {course.totalLessons} dars
          </span>
          <div className="tv-progress" role="progressbar" aria-label="Kurs bo‘yicha natija" aria-valuenow={percent} aria-valuemin={0} aria-valuemax={100}>
            <i style={{ width: `${percent}%` }} />
          </div>
        </div>

        <p style={{ margin: 0 }}>
          <Link href="/akademiya/ishlarim">Mening ishlarim →</Link>
        </p>

        <ol className="tv-lesson-list">
          {course.lessons.map((lesson, index) => (
            <li key={lesson.slug} className={`tv-card tv-lesson-item ${lesson.status}`}>
              <span className="tv-lesson-num" aria-hidden="true">
                {lesson.status === "completed" ? (
                  <svg viewBox="0 0 24 24">
                    <path d="M5 13l4 4L19 7" />
                  </svg>
                ) : (
                  index + 1
                )}
              </span>
              <div>
                <h2 className="tv-h3">{lesson.title}</h2>
                <p className="tv-muted">{lesson.summary}</p>
                <p className="tv-lesson-meta">
                  {statusText[lesson.status]} · taxminan {lesson.estimatedMinutes} daqiqa
                </p>
              </div>
              {actionText[lesson.status] && (
                <form action={startLessonAction.bind(null, lesson.slug)}>
                  <button className={`tv-btn ${lesson.slug === course.nextLessonSlug ? "tv-btn-primary" : "tv-btn-ghost"}`} type="submit">
                    {actionText[lesson.status]}
                  </button>
                </form>
              )}
            </li>
          ))}
        </ol>
      </main>
    </div>
  );
}
