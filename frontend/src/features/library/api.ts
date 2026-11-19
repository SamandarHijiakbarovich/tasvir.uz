import "server-only";
import { authedRequest, backendRequest } from "@/shared/lib/backend";
import type { Library, Share, Shared } from "./types";

export function getLibrary() {
  return authedRequest<Library>("/api/studio/library", { method: "GET" });
}

export function getMyShares() {
  return authedRequest<Share[]>("/api/studio/shares", { method: "GET" });
}

/** Ochiq havola: kirish talab qilinmaydi. */
export function getShared(token: string) {
  return backendRequest<Shared>(`/api/studio/shared/${encodeURIComponent(token)}`, { method: "GET" });
}
