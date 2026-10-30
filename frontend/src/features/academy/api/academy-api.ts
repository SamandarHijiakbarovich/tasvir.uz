import "server-only";
import { authedRequest } from "@/shared/lib/backend";
import type { Course, Lesson, MyWorkItem } from "./types";

/** Hozircha bitta kurs bor; keyinroq kurslar ro‘yxati qo‘shiladi. */
export const FIRST_COURSE_SLUG = "vizuallashtirish-asoslari";

export function getCourse(slug = FIRST_COURSE_SLUG) {
  return authedRequest<Course>(`/api/academy/courses/${encodeURIComponent(slug)}`, { method: "GET" });
}

export function getMyWork() {
  return authedRequest<MyWorkItem[]>("/api/academy/my-work", { method: "GET" });
}

export function getLesson(slug: string) {
  return authedRequest<Lesson>(`/api/academy/lessons/${encodeURIComponent(slug)}`, { method: "GET" });
}
