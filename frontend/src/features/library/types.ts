/** Backend DTO’lari bilan mos (Tasvir.Modules.Studio.Application/LibraryService.cs). */
import type { Material } from "@/features/studio/lib/types";

export type LibraryItem = {
  id: string;
  kind: "file" | "link";
  title: string;
  subject?: string | null;
  topic?: string | null;
  url?: string | null;
  fileName?: string | null;
  contentType?: string | null;
  sizeBytes: number;
  shareToken?: string | null;
  createdAt: string;
  updatedAt: string;
};

export type Library = { items: LibraryItem[]; usedBytes: number; quotaBytes: number; maxFileBytes: number };

export type ShareKind = "material" | "item";

export type Share = { kind: ShareKind; id: string; token: string };

export type Shared = { kind: ShareKind; material?: Material | null; item?: LibraryItem | null };

/** Yuklashga ruxsat etilgan fayllar (backend ham tekshiradi). */
export const ACCEPT = ".pdf,.pptx,.ppt,.png,.jpg,.jpeg,.webp";
