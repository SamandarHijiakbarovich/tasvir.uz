"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import type { FormState } from "@/features/auth/actions";
import { authedRequest, type ApiFailure } from "@/shared/lib/backend";
import type { GroupSummary } from "./types";

const BASE = "/api/classroom";
const LOGIN = "/kirish?keyin=/guruhlar";
const groupPath = (id: string) => `/guruhlar/${encodeURIComponent(id)}`;

const text = (form: FormData, name: string) => {
  const value = form.get(name);
  return typeof value === "string" ? value : "";
};

function failed(failure: ApiFailure): FormState {
  if (failure.status === 401) redirect(LOGIN);
  return {
    message: failure.fields && Object.keys(failure.fields).length > 0 ? undefined : failure.message,
    fields: failure.fields,
  };
}

export async function createGroupAction(_: FormState, form: FormData): Promise<FormState> {
  const result = await authedRequest<GroupSummary>(`${BASE}/groups`, { body: { name: text(form, "name") } });
  if (!result.ok) return failed(result);

  revalidatePath("/guruhlar");
  redirect(groupPath(result.data.id));
}

export async function joinGroupAction(_: FormState, form: FormData): Promise<FormState> {
  const result = await authedRequest<GroupSummary>(`${BASE}/groups/join`, {
    body: { code: text(form, "code"), acceptSharing: form.get("acceptSharing") === "on" },
  });
  if (!result.ok) return failed(result);

  revalidatePath("/guruhlar");
  redirect(groupPath(result.data.id));
}

export async function deleteGroupAction(groupId: string): Promise<void> {
  const result = await authedRequest<void>(`${BASE}/groups/${encodeURIComponent(groupId)}`, { method: "DELETE" });
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    redirect(`${groupPath(groupId)}?xato=${encodeURIComponent(result.message)}`);
  }

  revalidatePath("/guruhlar");
  redirect("/guruhlar");
}

/** Talabani chiqarish (o‘qituvchi). Shu guruhdagi ishlari va baholari ham o‘chadi. */
export async function removeMemberAction(groupId: string, userId: string): Promise<void> {
  const result = await authedRequest<void>(`${BASE}/groups/${encodeURIComponent(groupId)}/members/${encodeURIComponent(userId)}`, { method: "DELETE" });
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    redirect(`${groupPath(groupId)}?xato=${encodeURIComponent(result.message)}`);
  }

  revalidatePath(groupPath(groupId));
  redirect(groupPath(groupId));
}

/** Talaba guruhdan o‘zi chiqadi. */
export async function leaveGroupAction(groupId: string, userId: string): Promise<void> {
  const result = await authedRequest<void>(`${BASE}/groups/${encodeURIComponent(groupId)}/members/${encodeURIComponent(userId)}`, { method: "DELETE" });
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    redirect(`${groupPath(groupId)}?xato=${encodeURIComponent(result.message)}`);
  }

  revalidatePath("/guruhlar");
  redirect("/guruhlar");
}

export async function submitWorkAction(groupId: string, _: FormState, form: FormData): Promise<FormState> {
  const materialId = text(form, "materialId");
  if (!materialId) return { fields: { materialId: "Topshiriladigan materialni tanlang." } };

  const result = await authedRequest(`${BASE}/groups/${encodeURIComponent(groupId)}/submissions`, { body: { materialId } });
  if (!result.ok) return failed(result);

  revalidatePath(groupPath(groupId));
  return { success: true, message: "Ish o‘qituvchiga topshirildi." };
}

export async function saveReviewAction(submissionId: string, _: FormState, form: FormData): Promise<FormState> {
  const scores: Record<string, number> = {};
  for (const [name, value] of form.entries()) {
    if (name.startsWith("score:") && typeof value === "string") scores[name.slice(6)] = Number(value);
  }

  const result = await authedRequest(`${BASE}/submissions/${encodeURIComponent(submissionId)}/review`, {
    method: "PUT",
    body: { scores, comment: text(form, "comment") },
  });
  if (!result.ok) return failed(result);

  revalidatePath(`/guruhlar/ish/${encodeURIComponent(submissionId)}`);
  return { success: true, message: "Baho saqlandi. Talaba uni o‘z sahifasida ko‘radi." };
}

export async function saveSurveyAction(groupId: string, _: FormState, form: FormData): Promise<FormState> {
  const answers: Record<string, number> = {};
  for (const [name, value] of form.entries()) {
    if (name.startsWith("answer:") && typeof value === "string") answers[name.slice(7)] = Number(value);
  }

  const result = await authedRequest(`${BASE}/groups/${encodeURIComponent(groupId)}/survey`, {
    method: "PUT",
    body: { answers, comment: text(form, "comment") },
  });
  if (!result.ok) return failed(result);

  revalidatePath(groupPath(groupId));
  return { success: true, message: "Rahmat! Javobingiz saqlandi." };
}
