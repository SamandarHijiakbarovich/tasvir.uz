"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { authedRequest } from "@/shared/lib/backend";
import type { LibraryItem, Share, ShareKind } from "./types";

const BASE = "/api/studio/library";
const PAGE = "/kabinet/materiallar";
const LOGIN = `/kirish?keyin=${PAGE}`;
/** Sayt Vercel da: so‘rov tanasi 4,5 MB dan oshmaydi. Backend ham shu chegarani tekshiradi. */
const MAX_FILE_BYTES = 4 * 1024 * 1024;

export type LibraryFormState = { ok?: boolean; message?: string; key?: number };

const text = (formData: FormData, name: string) => {
  const value = formData.get(name);
  return typeof value === "string" ? value.trim() : "";
};

const meta = (formData: FormData) => ({
  title: text(formData, "title"),
  subject: text(formData, "subject") || null,
  topic: text(formData, "topic") || null,
});

function done(message: string): LibraryFormState {
  revalidatePath(PAGE);
  revalidatePath("/kabinet");
  // key: forma muvaffaqiyatdan keyin tozalanishi uchun
  return { ok: true, message, key: Date.now() };
}

export async function addLinkAction(_: LibraryFormState, formData: FormData): Promise<LibraryFormState> {
  const result = await authedRequest<LibraryItem>(`${BASE}/links`, { body: { ...meta(formData), url: text(formData, "url") } });
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    return { message: result.message };
  }
  return done(`“${result.data.title}” saqlandi.`);
}

export async function uploadFileAction(_: LibraryFormState, formData: FormData): Promise<LibraryFormState> {
  const file = formData.get("file");
  if (!(file instanceof File) || file.size === 0) return { message: "Faylni tanlang." };
  if (file.size > MAX_FILE_BYTES) {
    return { message: "Fayl hajmi 4 MB dan oşmasin. Katta taqdimotni PDF qilib siqing yoki havola sifatida saqlang." };
  }

  const data = Buffer.from(await file.arrayBuffer()).toString("base64");
  const fields = meta(formData);
  const result = await authedRequest<LibraryItem>(`${BASE}/files`, {
    body: { ...fields, title: fields.title || file.name.replace(/\.[^.]+$/, ""), fileName: file.name, dataBase64: data },
  });
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    return { message: result.message };
  }
  return done(`“${result.data.title}” yuklandi.`);
}

export async function updateItemAction(id: string, _: LibraryFormState, formData: FormData): Promise<LibraryFormState> {
  const url = formData.has("url") ? text(formData, "url") : null;
  const result = await authedRequest<LibraryItem>(`${BASE}/${encodeURIComponent(id)}`, { method: "PUT", body: { ...meta(formData), url } });
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    return { message: result.message };
  }
  return done("Özgarişlar saqlandi.");
}

export async function deleteItemAction(id: string): Promise<void> {
  const result = await authedRequest<void>(`${BASE}/${encodeURIComponent(id)}`, { method: "DELETE" });
  if (!result.ok) {
    if (result.status === 401) redirect(LOGIN);
    redirect(`${PAGE}?xato=${encodeURIComponent(result.message)}`);
  }
  revalidatePath(PAGE);
  redirect(PAGE);
}

export type ShareResponse = { ok: true; token: string | null } | { ok: false; message: string };

export async function shareAction(kind: ShareKind, id: string): Promise<ShareResponse> {
  const result = await authedRequest<Share>("/api/studio/shares", { body: { kind, id } });
  if (!result.ok) return { ok: false, message: result.message };
  revalidatePath(PAGE);
  return { ok: true, token: result.data.token };
}

export async function unshareAction(kind: ShareKind, id: string): Promise<ShareResponse> {
  const result = await authedRequest<void>(`/api/studio/shares/${kind}/${encodeURIComponent(id)}`, { method: "DELETE" });
  if (!result.ok) return { ok: false, message: result.message };
  revalidatePath(PAGE);
  return { ok: true, token: null };
}
