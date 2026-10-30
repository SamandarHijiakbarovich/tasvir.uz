"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { authedRequest } from "@/shared/lib/backend";
import type { StepAnswer, StepResult } from "./api/types";

const lessonPath = (slug: string) => `/akademiya/darslar/${encodeURIComponent(slug)}`;

/** "Darsni boshlash" tugmasi: holatni saqlab, dars sahifasiga o‘tkazadi. */
export async function startLessonAction(slug: string): Promise<void> {
  const result = await authedRequest<unknown>(`/api/academy/lessons/${encodeURIComponent(slug)}/start`);

  if (!result.ok) {
    if (result.status === 401) redirect(`/kirish?keyin=${encodeURIComponent(lessonPath(slug))}`);
    // Dars hali ochilmagan yoki topilmagan: kurs sahifasi holatni tushuntiradi
    redirect("/akademiya");
  }

  redirect(lessonPath(slug));
}

export type CompleteStepResponse =
  | { ok: true; result: StepResult }
  | { ok: false; message: string; sessionExpired?: boolean };

export async function completeStepAction(slug: string, stepId: string, answer: StepAnswer): Promise<CompleteStepResponse> {
  const result = await authedRequest<StepResult>(
    `/api/academy/lessons/${encodeURIComponent(slug)}/steps/${encodeURIComponent(stepId)}/complete`,
    { body: answer },
  );

  if (!result.ok) {
    return { ok: false, message: result.message, sessionExpired: result.status === 401 };
  }

  if (result.data.lessonCompleted) {
    revalidatePath("/akademiya");
    revalidatePath("/kabinet");
  }

  return { ok: true, result: result.data };
}
