"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { authedRequest } from "@/shared/lib/backend";
import { getTemplate } from "./lib/templates";
import type { Material, MaterialDraft } from "./lib/types";

const BASE = "/api/studio/materials";
const LOGIN = "/kirish?keyin=/studiya";
const materialPath = (id: string) => `/studiya/${encodeURIComponent(id)}`;

/** Tanlangan shablon namunasidan yangi material yaratib, muharrirni ochadi. */
export async function createFromTemplateAction(templateKey: string): Promise<void> {
  const template = getTemplate(templateKey);
  if (!template) redirect("/studiya/yangi");

  const result = await authedRequest<Material>(BASE, { body: { templateKey, ...template.starter } });
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    redirect(`/studiya?xato=${encodeURIComponent(result.message)}`);
  }

  revalidatePath("/studiya");
  redirect(materialPath(result.data.id));
}

export type SaveResponse = { ok: true; material: Material } | { ok: false; message: string; sessionExpired?: boolean };

export async function saveMaterialAction(id: string, draft: MaterialDraft): Promise<SaveResponse> {
  const result = await authedRequest<Material>(`${BASE}/${encodeURIComponent(id)}`, { method: "PUT", body: draft });
  if (!result.ok) {
    return { ok: false, message: result.message, sessionExpired: result.status === 401 };
  }

  revalidatePath("/studiya");
  return { ok: true, material: result.data };
}

export async function duplicateMaterialAction(id: string): Promise<void> {
  const result = await authedRequest<Material>(`${BASE}/${encodeURIComponent(id)}/duplicate`);
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    redirect(`/studiya?xato=${encodeURIComponent(result.message)}`);
  }

  revalidatePath("/studiya");
  redirect(materialPath(result.data.id));
}

export async function deleteMaterialAction(id: string): Promise<void> {
  const result = await authedRequest<void>(`${BASE}/${encodeURIComponent(id)}`, { method: "DELETE" });
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    redirect(`/studiya?xato=${encodeURIComponent(result.message)}`);
  }

  revalidatePath("/studiya");
  redirect("/studiya");
}
