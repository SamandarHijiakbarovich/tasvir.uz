"use server";

import { backendRequest } from "@/shared/lib/backend";

export type AssistantAnswer = {
  answer: string;
  isMatched: boolean;
  link?: { label: string; href: string } | null;
  suggestions: string[];
};

export type AskResponse = { ok: true; data: AssistantAnswer } | { ok: false; message: string };

export async function askAssistantAction(question: string): Promise<AskResponse> {
  const result = await backendRequest<AssistantAnswer>("/api/assistant/ask", { body: { question } });
  return result.ok ? { ok: true, data: result.data } : { ok: false, message: result.message };
}
