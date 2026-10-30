import type { Metadata } from "next";
import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { getLesson } from "@/features/academy/api/academy-api";
import { AcademyHeader } from "@/features/academy/components/AcademyHeader";
import { LessonPlayer } from "@/features/academy/components/LessonPlayer";
import "@/styles/auth.css";
import "@/styles/academy.css";

export const metadata: Metadata = { title: "Dars — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { params: Promise<{ slug: string }> };

export default async function LessonPage({ params }: Props) {
  const { slug } = await params;
  const result = await getLesson(slug);

  if (!result.ok) {
    if (result.status === 401) redirect(`/kirish?keyin=${encodeURIComponent(`/akademiya/darslar/${slug}`)}`);
    if (result.status === 404) notFound();
    if (result.status === 409) redirect("/akademiya");

    return (
      <div className="tv-academy">
        <AcademyHeader />
        <main className="tv-academy-in">
          <p className="tv-form-msg err" role="alert">
            {result.message}
          </p>
        </main>
      </div>
    );
  }

  const lesson = result.data;

  return (
    <div className="tv-academy">
      <AcademyHeader />
      <main className="tv-academy-in">
        <div>
          <p className="tv-breadcrumb">
            <Link href="/akademiya">{lesson.courseTitle}</Link> · {lesson.position}-dars / {lesson.totalLessons}
          </p>
          <h1 className="tv-h2" style={{ marginTop: 8 }}>
            {lesson.title}
          </h1>
        </div>
        {/* Kalit: keyingi darsga o‘tilganda pleyer holati noldan boshlanadi */}
        <LessonPlayer key={lesson.slug} lesson={lesson} />
      </main>
    </div>
  );
}
