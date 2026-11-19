import type { LibraryItem } from "./types";

export function formatSize(bytes: number) {
  if (bytes < 1024 * 1024) return `${Math.max(1, Math.round(bytes / 1024))} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1).replace(".", ",")} MB`;
}

export function kindLabel(item: LibraryItem) {
  if (item.kind === "link") return "Havola";
  const type = item.contentType ?? "";
  if (type === "application/pdf") return "PDF";
  if (type.includes("presentation") || type.includes("powerpoint")) return "PowerPoint";
  if (type.startsWith("image/")) return "Rasm";
  return "Fayl";
}

/** Ulashish sahifasi manzili (sayt manzili brauzerda aniqlanadi). */
export const sharePath = (token: string) => `/ulashish/${token}`;

/** Fayl mazmuni: egasi uchun va ochiq havola uchun sayt serveri orqali. */
export const ownerFileUrl = (id: string) => `/fayl/${id}`;
export const sharedFileUrl = (token: string) => `/fayl/ulashilgan/${token}`;

/**
 * Havolani sahifaga joylash (iframe) manzili. Ma’lum xizmatlarning "ko‘rish" manzili joylashga moslanadi;
 * boshqa saytlar o‘zini joylashni taqiqlashi mumkin, shuning uchun "Yangi oynada ochish" tugmasi har doim bo‘ladi.
 */
export function embedUrl(raw: string): string | null {
  let url: URL;
  try {
    url = new URL(raw);
  } catch {
    return null;
  }
  if (url.protocol !== "https:") return null;
  const host = url.hostname.replace(/^www\./, "");

  if (host === "docs.google.com") {
    const slides = url.pathname.match(/^\/presentation\/d\/([^/]+)/);
    if (slides) return `https://docs.google.com/presentation/d/${slides[1]}/embed`;
    const doc = url.pathname.match(/^\/(document|spreadsheets)\/d\/([^/]+)/);
    if (doc) return `https://docs.google.com/${doc[1]}/d/${doc[2]}/preview`;
  }
  if (host === "drive.google.com") {
    const file = url.pathname.match(/^\/file\/d\/([^/]+)/);
    if (file) return `https://drive.google.com/file/d/${file[1]}/preview`;
  }
  if (host === "canva.com" && url.pathname.startsWith("/design/")) {
    const path = url.pathname.replace(/\/(edit|view)\/?$/, "/view");
    return `https://www.canva.com${path}?embed`;
  }
  if (host === "youtube.com" && url.searchParams.get("v")) return `https://www.youtube-nocookie.com/embed/${url.searchParams.get("v")}`;
  if (host === "youtu.be") return `https://www.youtube-nocookie.com/embed${url.pathname}`;
  return url.toString();
}

/** PowerPoint faylni brauzerda ko‘rsatish: Microsoft ning bepul ko‘ruvchisi ochiq havoladagi faylni o‘qiydi. */
export function officeViewerUrl(publicFileUrl: string) {
  return `https://view.officeapps.live.com/op/embed.aspx?src=${encodeURIComponent(publicFileUrl)}`;
}
