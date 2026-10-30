"use client";

import { useState } from "react";
import { downloadBlob, downloadSvgAsPng } from "@/shared/lib/download";

const WIDTH = 640;
const BOX_X = 40;
const BOX_WIDTH = WIDTH - BOX_X * 2;
const LINE_HEIGHT = 26;
const PADDING_Y = 16;
const GAP = 40;
const MARGIN = 32;
const MAX_CHARS_PER_LINE = 38;

const escapeXml = (value: string) =>
  value.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");

function wrap(text: string): string[] {
  const lines: string[] = [];
  let line = "";
  for (const word of text.split(/\s+/)) {
    // Bo‘shliqsiz uzun so‘z ham qutidan chiqib ketmasin
    for (let rest = word; rest.length > 0; rest = rest.slice(MAX_CHARS_PER_LINE)) {
      const part = rest.slice(0, MAX_CHARS_PER_LINE);
      if (line && line.length + 1 + part.length > MAX_CHARS_PER_LINE) {
        lines.push(line);
        line = part;
      } else {
        line = line ? `${line} ${part}` : part;
      }
    }
  }
  if (line) lines.push(line);
  return lines;
}

/** Sxemani mustaqil SVG faylga aylantiradi. Ranglar doimiy: mavzuga bog‘liq emas, chop etishda ham o‘qiladi. */
export function buildDiagramSvg(items: string[]): { svg: string; width: number; height: number } {
  let y = MARGIN;
  const parts: string[] = [];

  items.forEach((item, index) => {
    const lines = wrap(item);
    const boxHeight = lines.length * LINE_HEIGHT + PADDING_Y * 2;
    const fill = index === 0 ? "#ffc93c" : "#ffffff";

    parts.push(`<rect x="${BOX_X}" y="${y}" width="${BOX_WIDTH}" height="${boxHeight}" rx="14" fill="${fill}" stroke="#17162b" stroke-width="2"/>`);
    lines.forEach((line, i) => {
      const textY = y + PADDING_Y + LINE_HEIGHT * i + 19;
      parts.push(`<text x="${WIDTH / 2}" y="${textY}" text-anchor="middle" font-family="Arial, Helvetica, sans-serif" font-size="20" font-weight="600" fill="#17162b">${escapeXml(line)}</text>`);
    });
    y += boxHeight;

    if (index < items.length - 1) {
      const x = WIDTH / 2;
      parts.push(`<path d="M${x} ${y + 6}V${y + GAP - 8}M${x - 7} ${y + GAP - 15}L${x} ${y + GAP - 8}L${x + 7} ${y + GAP - 15}" fill="none" stroke="#b93a28" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/>`);
      y += GAP;
    }
  });

  const height = y + MARGIN;
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${WIDTH}" height="${height}" viewBox="0 0 ${WIDTH} ${height}"><rect width="${WIDTH}" height="${height}" fill="#ffffff"/>${parts.join("")}</svg>`;
  return { svg, width: WIDTH, height };
}

export function DiagramDownload({ items, name = "sxema" }: { items: string[]; name?: string }) {
  const [error, setError] = useState(false);
  const filled = items.map((i) => i.trim()).filter(Boolean);
  if (filled.length === 0) return null;

  const { svg, width, height } = buildDiagramSvg(filled);

  return (
    <div className="tv-download">
      <button
        className="tv-btn tv-btn-ghost tv-btn-sm"
        type="button"
        onClick={() => {
          setError(false);
          downloadSvgAsPng(svg, width, height, `${name}.png`).catch(() => setError(true));
        }}
      >
        PNG yuklab olish
      </button>
      <button
        className="tv-btn tv-btn-ghost tv-btn-sm"
        type="button"
        onClick={() => downloadBlob(new Blob([svg], { type: "image/svg+xml" }), `${name}.svg`)}
      >
        SVG yuklab olish
      </button>
      {error && (
        <p className="tv-field-error" role="alert">
          PNG yaratib bo‘lmadi. SVG faylni yuklab oling.
        </p>
      )}
    </div>
  );
}
