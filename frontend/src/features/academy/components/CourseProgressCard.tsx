import Link from "next/link";
import { startLessonAction } from "../actions";
import type { Course } from "../api/types";

/** Kabinetdagi asosiy chaqiruv: birinchi darsni boshlash yoki to‘xtagan joydan davom ettirish. */
export function CourseProgressCard({ course }: { course: Course }) {
  const done = !course.nextLessonSlug;
  const percent = Math.round((course.completedLessons / course.totalLessons) * 100);

  return (
    <section className="tv-card tv-card-outline tv-course-card" aria-labelledby="kurs-t">
      <div>
        <span className="tv-label">Akademiya</span>
        <h2 className="tv-h3" id="kurs-t">
          {course.title}
        </h2>
        <p className="tv-muted">{course.description}</p>
      </div>
      <div className="tv-lesson-progress">
        <span>
          {course.completedLessons} / {course.totalLessons} dars
        </span>
        <div className="tv-progress" role="progressbar" aria-label="Kurs bo‘yicha natija" aria-valuenow={percent} aria-valuemin={0} aria-valuemax={100}>
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
        <Link className={`tv-btn ${done ? "tv-btn-primary" : "tv-btn-ghost"}`} href="/akademiya">
          Barcha darslar
        </Link>
      </div>
    </section>
  );
}
