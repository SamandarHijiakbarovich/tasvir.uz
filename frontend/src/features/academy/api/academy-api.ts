import "server-only";
import { authedRequest } from "@/shared/lib/backend";
import type { Course, Lesson, MyWorkItem } from "./types";

export function getCourses() {
  return authedRequest<Course[]>("/api/academy/courses", { method: "GET" });
}

export function getCourse(slug: string) {
  return authedRequest<Course>(`/api/academy/courses/${encodeURIComponent(slug)}`, { method: "GET" });
}

export function getMyWork() {
  return authedRequest<MyWorkItem[]>("/api/academy/my-work", { method: "GET" });
}

export function getLesson(slug: string) {
  return authedRequest<Lesson>(`/api/academy/lessons/${encodeURIComponent(slug)}`, { method: "GET" });
}
