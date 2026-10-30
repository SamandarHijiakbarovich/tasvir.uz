"use client";

import Link from "next/link";
import { useCallback, useEffect, useRef, useState } from "react";
import { describeMaterial, renderMaterial, svgDataUri } from "../lib/render";
import type { Material } from "../lib/types";

/** Darsda ko‘rsatish: elementlar birin-ketin ochiladi, o‘qituvchi tushuntirish sur’atini o‘zi boshqaradi. */
export function PresentView({ material }: { material: Material }) {
  const total = material.content.items.length;
  const [visible, setVisible] = useState(0);
  const rootRef = useRef<HTMLDivElement>(null);

  const next = useCallback(() => setVisible((v) => Math.min(total, v + 1)), [total]);
  const previous = useCallback(() => setVisible((v) => Math.max(0, v - 1)), []);
  const toggleFullscreen = useCallback(() => {
    if (document.fullscreenElement) void document.exitFullscreen();
    else void rootRef.current?.requestFullscreen?.();
  }, []);

  useEffect(() => {
    const onKey = (event: KeyboardEvent) => {
      if ((event.target as HTMLElement).closest("a, button")) {
        // Tugma ustida Enter yoki bo‘sh joy o‘sha tugmaning o‘zini bosadi
        if (event.key === " " || event.key === "Enter") return;
      }
      if (["ArrowRight", "ArrowDown", "PageDown", " "].includes(event.key)) {
        event.preventDefault();
        next();
      } else if (["ArrowLeft", "ArrowUp", "PageUp"].includes(event.key)) {
        event.preventDefault();
        previous();
      } else if (event.key === "f" || event.key === "F") {
        toggleFullscreen();
      }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [next, previous, toggleFullscreen]);

  const { svg } = renderMaterial(material, visible);

  return (
    <div className="tv-present" ref={rootRef}>
      <div className="tv-present-stage">
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={svgDataUri(svg)} alt={describeMaterial(material)} onClick={next} />
      </div>
      <div className="tv-present-bar">
        <Link className="tv-present-btn" href={`/studiya/${material.id}`}>
          Yopish
        </Link>
        <div className="tv-present-nav">
          <button className="tv-present-btn" type="button" onClick={previous} disabled={visible === 0}>
            ← Orqaga
          </button>
          <span aria-live="polite">
            {visible} / {total}
          </span>
          <button className="tv-present-btn primary" type="button" onClick={next} disabled={visible === total}>
            Keyingisi →
          </button>
        </div>
        <div className="tv-present-nav">
          <button className="tv-present-btn" type="button" onClick={() => setVisible(total)} disabled={visible === total}>
            Hammasi
          </button>
          <button className="tv-present-btn" type="button" onClick={toggleFullscreen}>
            To‘liq ekran
          </button>
        </div>
      </div>
    </div>
  );
}
