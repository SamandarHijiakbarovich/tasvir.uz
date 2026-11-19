"use client";

import Link from "next/link";
import { useCallback, useEffect, useRef, useSyncExternalStore } from "react";
import { embedUrl, officeViewerUrl, sharedFileUrl } from "../format";
import type { LibraryItem } from "../types";

type Props = {
  item: LibraryItem;
  /** Fayl manzili shu sayt ichida (egasi uchun /fayl/{id}, ochiq havola uchun /fayl/ulashilgan/{token}). */
  fileUrl: string | null;
  /** Ochiq havola tokeni: PowerPoint faqat shu bilan Microsoft ko‘ruvchisida ochiladi. */
  shareToken?: string | null;
  closeHref?: string;
};

const noSubscribe = () => () => {};

/** Saqlangan fayl yoki havolani darsda to‘liq ekranda ko‘rsatish. */
export function ResourceViewer({ item, fileUrl, shareToken, closeHref }: Props) {
  const rootRef = useRef<HTMLDivElement>(null);
  const origin = useSyncExternalStore(noSubscribe, () => window.location.origin, () => "");

  const toggleFullscreen = useCallback(() => {
    if (document.fullscreenElement) void document.exitFullscreen();
    else void rootRef.current?.requestFullscreen?.();
  }, []);

  useEffect(() => {
    const onKey = (event: KeyboardEvent) => {
      if ((event.key === "f" || event.key === "F") && !(event.target as HTMLElement).closest("input, textarea")) toggleFullscreen();
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [toggleFullscreen]);

  const type = item.contentType ?? "";
  const isPowerPoint = type.includes("presentation") || type.includes("powerpoint");
  // Microsoft ko‘ruvchisi faylni internetdan o‘qiydi: lokal kompyuterdagi (localhost) faylga yeta olmaydi
  const officeReachable = Boolean(shareToken && origin && !/^https?:\/\/(localhost|127\.0\.0\.1)/.test(origin));

  let stage: React.ReactNode;
  let openHref: string | null = null;

  if (item.kind === "link" && item.url) {
    openHref = item.url;
    const src = embedUrl(item.url);
    stage = src ? (
      <iframe className="tv-viewer-frame" src={src} title={item.title} allow="fullscreen" referrerPolicy="no-referrer" />
    ) : (
      <ViewerNote>Bu havolani sahifa ichida körsatib bölmaydi. Uni yangi oynada oçing.</ViewerNote>
    );
  } else if (fileUrl && type === "application/pdf") {
    openHref = fileUrl;
    stage = <iframe className="tv-viewer-frame" src={fileUrl} title={item.title} />;
  } else if (fileUrl && type.startsWith("image/")) {
    openHref = fileUrl;
    // eslint-disable-next-line @next/next/no-img-element
    stage = <img className="tv-viewer-img" src={fileUrl} alt={item.title} />;
  } else if (fileUrl && isPowerPoint) {
    openHref = fileUrl;
    stage = officeReachable ? (
      <iframe className="tv-viewer-frame" src={officeViewerUrl(`${origin}${sharedFileUrl(shareToken!)}`)} title={item.title} allow="fullscreen" />
    ) : (
      <ViewerNote>
        PowerPoint faylni brauzerda körsatiş uçun avval uning <strong>ulaşiş havolasini</strong> yoqing (Microsoft körsatiş xizmati faylni şu havola orqali
        öqiydi). Yoki faylni yuklab olib, PowerPoint da oçing. Eng qulay yöli — taqdimotni PDF qilib saqlab yuklaş.
      </ViewerNote>
    );
  } else {
    stage = <ViewerNote>Bu turdagi faylni brauzerda körsatib bölmaydi.</ViewerNote>;
  }

  return (
    <div className="tv-present" ref={rootRef}>
      <div className="tv-present-stage tv-viewer-stage">{stage}</div>
      <div className="tv-present-bar">
        {closeHref ? (
          <Link className="tv-present-btn" href={closeHref}>
            Yopiş
          </Link>
        ) : (
          <span />
        )}
        <strong className="tv-viewer-title">{item.title}</strong>
        <div className="tv-present-nav">
          {openHref && (
            <a className="tv-present-btn" href={openHref} target="_blank" rel="noopener noreferrer">
              {item.kind === "link" ? "Yangi oynada oçiş ↗" : "Yuklab oliş"}
            </a>
          )}
          <button className="tv-present-btn primary" type="button" onClick={toggleFullscreen}>
            Töliq ekran
          </button>
        </div>
      </div>
    </div>
  );
}

function ViewerNote({ children }: { children: React.ReactNode }) {
  return <p className="tv-viewer-note">{children}</p>;
}
