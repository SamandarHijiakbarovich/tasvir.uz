import Link from "next/link";
import { startLessonAction } from "../actions";
import type { Course } from "../api/types";

/** Kurs kartasi: progress, keyingi darsni boshlash yoki davom ettirish. Kabinet va Akademiya sahifalarida ishlatiladi. */
export function CourseCard({ course }: { course: Course }) {
  const done = !course.nextLessonSlug;
  const percent = course.totalLessons === 0 ? 0 : Math.round((course.completedLessons / course.totalLessons) * 100);
  const coursePath = `/akademiya/kurslar/${course.slug}`;

  return (
    <section className="tv-card tv-card-outline tv-course-card" aria-labelledby={`kurs-${course.slug}`}>
      <div>
        <h2 className="tv-h3" id={`kurs-${course.slug}`}>
          {course.title}
        </h2>
        <p className="tv-muted">{course.description}</p>
      </div>
      <div className="tv-lesson-progress">
        <span>
          {course.completedLessons} / {course.totalLessons} dars
        </span>
        <div className="tv-progress" role="progressbar" aria-label={`${course.title}: natija`} aria-valuenow={percent} aria-valuemin={0} aria-valuemax={100}>
          <i style={{ width: `${percent}%` }} />
        </div>
      </div>
      <div className="tv-lesson-actions">
        {course.nextLessonSlug && (
          <form action={startLessonAction.bind(null, course.nextLessonSlug)}>
            <button className="tv-btn tv-btn-primary" type="submit">
              {course.hasStarted ? "Davom ettirish →" : "Birinchi darsni boshlash →"}
            </button>
          </form>
        )}
        <Link className={`tv-btn ${done ? "tv-btn-primary" : "tv-btn-ghost"}`} href={coursePath}>
          Barcha darslar
        </Link>
      </div>
    </section>
  );
}
