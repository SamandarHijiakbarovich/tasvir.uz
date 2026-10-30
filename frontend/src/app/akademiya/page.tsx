import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { getCourses } from "@/features/academy/api/academy-api";
import { AcademyHeader } from "@/features/academy/components/AcademyHeader";
import { CourseCard } from "@/features/academy/components/CourseCard";
import "@/styles/auth.css";
import "@/styles/academy.css";

export const metadata: Metadata = { title: "Akademiya — Tasvir.uz", robots: { index: false, follow: false } };

export default async function AcademyPage() {
  const result = await getCourses();

  if (!result.ok && result.status === 401) redirect("/kirish?keyin=/akademiya");

  return (
    <div className="tv-academy">
      <AcademyHeader />
      <main className="tv-academy-in">
        <div>
          <h1 className="tv-h2">Akademiya</h1>
          <p className="tv-lead" style={{ marginTop: 8 }}>
            Kursni tanlang va o‘zingizga qulay sur’atda o‘rganing.
          </p>
        </div>

        {result.ok ? (
          result.data.map((course) => <CourseCard key={course.slug} course={course} />)
        ) : (
          <p className="tv-form-msg err" role="alert">
            {result.message}
          </p>
        )}

        <p style={{ margin: 0 }}>
          <Link href="/akademiya/ishlarim">Mening ishlarim →</Link>
        </p>
      </main>
    </div>
  );
}
