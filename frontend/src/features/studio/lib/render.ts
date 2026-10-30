import { ACCENTS } from "./templates";
import type { MaterialDraft, MaterialItem } from "./types";

/**
 * Materialni mustaqil SVG ga chizadi. Ko‘rish, darsda ko‘rsatish va eksport uchun bitta manba.
 * Ranglar doimiy (saytning yorug‘/qorong‘i mavzusiga bog‘liq emas): proyektor va chop etishda ham o‘qiladi.
 */
const WIDTH = 1200;
const MARGIN = 60;
const INK = "#17162b";
const MUTED = "#55546b";
const LINE = "#17162b";
const SOFT = "#f3f1ea";
const ARROW = "#b93a28";
const FONT = "Arial, Helvetica, sans-serif";

const escapeXml = (value: string) =>
  value.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");

// Matn eni brauzersiz ham hisoblanishi uchun taxminiy; zaxira bilan olingan
const charWidth = (size: number, bold: boolean) => size * (bold ? 0.6 : 0.54);

function wrap(text: string, size: number, maxWidth: number, bold = false): string[] {
  const perLine = Math.max(4, Math.floor(maxWidth / charWidth(size, bold)));
  const lines: string[] = [];
  let line = "";
  for (const word of text.trim().split(/\s+/)) {
    for (let rest = word; rest.length > 0; rest = rest.slice(perLine)) {
      const part = rest.slice(0, perLine);
      if (line && line.length + 1 + part.length > perLine) {
        lines.push(line);
        line = part;
      } else {
        line = line ? `${line} ${part}` : part;
      }
    }
  }
  if (line) lines.push(line);
  return lines.length ? lines : [""];
}

type TextStyle = { size: number; bold?: boolean; fill?: string; anchor?: "start" | "middle" | "end"; lineHeight: number };

function textLines(lines: string[], x: number, top: number, style: TextStyle): string {
  const { size, bold, fill = INK, anchor = "start", lineHeight } = style;
  return lines
    .map((line, i) => {
      const y = top + lineHeight * i + size;
      return `<text x="${x}" y="${y}" text-anchor="${anchor}" font-family="${FONT}" font-size="${size}" font-weight="${bold ? 700 : 400}" fill="${fill}">${escapeXml(line)}</text>`;
    })
    .join("");
}

const HEADING: TextStyle = { size: 26, bold: true, lineHeight: 34 };
const BODY: TextStyle = { size: 22, fill: MUTED, lineHeight: 30 };

type CardText = { heading: string[]; body: string[]; height: number };

function measure(item: MaterialItem, width: number, padding = 20): CardText {
  const heading = wrap(item.heading, HEADING.size, width - padding * 2, true);
  const body = item.text?.trim() ? wrap(item.text, BODY.size, width - padding * 2) : [];
  const height = padding * 2 + heading.length * HEADING.lineHeight + (body.length ? 6 + body.length * BODY.lineHeight : 0);
  return { heading, body, height };
}

function drawCard(card: CardText, x: number, y: number, width: number, fill: string, ink: string, padding = 20): string {
  const bodyFill = ink === INK ? (fill === "#ffffff" ? MUTED : INK) : ink;
  return (
    `<rect x="${x}" y="${y}" width="${width}" height="${card.height}" rx="16" fill="${fill}" stroke="${LINE}" stroke-width="2"/>` +
    textLines(card.heading, x + padding, y + padding, { ...HEADING, fill: ink }) +
    textLines(card.body, x + padding, y + padding + card.heading.length * HEADING.lineHeight + 6, { ...BODY, fill: bodyFill })
  );
}

const arrowDown = (x: number, y: number, length: number) =>
  `<path d="M${x} ${y + 6}V${y + length - 8}M${x - 8} ${y + length - 16}L${x} ${y + length - 8}L${x + 8} ${y + length - 16}" fill="none" stroke="${ARROW}" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/>`;

type Layout = { body: string; height: number };

function titleBlock(title: string, top: number): { svg: string; bottom: number } {
  const lines = wrap(title, 44, WIDTH - MARGIN * 2, true);
  return {
    svg: textLines(lines, WIDTH / 2, top, { size: 44, bold: true, anchor: "middle", lineHeight: 54 }),
    bottom: top + lines.length * 54,
  };
}

function process(doc: MaterialDraft, visible: number, accent: Accent): Layout {
  const { svg, bottom } = titleBlock(doc.title, 48);
  const width = WIDTH - MARGIN * 2;
  const gap = 44;
  let y = bottom + 32;
  let body = svg;
  doc.content.items.forEach((item, i, all) => {
    const card = measure(item, width);
    if (i < visible) {
      body += drawCard(card, MARGIN, y, width, i === 0 ? accent.fill : "#ffffff", i === 0 ? accent.ink : INK);
    }
    y += card.height;
    if (i < all.length - 1) {
      if (i + 1 < visible) body += arrowDown(WIDTH / 2, y, gap);
      y += gap;
    }
  });
  return { body, height: y };
}

function timeline(doc: MaterialDraft, visible: number, accent: Accent): Layout {
  const { svg, bottom } = titleBlock(doc.title, 48);
  const dateWidth = 220;
  const axisX = MARGIN + dateWidth + 30;
  const textX = axisX + 40;
  const textWidth = WIDTH - MARGIN - textX;
  const start = bottom + 32;
  let y = start;
  let body = "";
  const rows = doc.content.items.map((item) => {
    const date = wrap(item.heading, HEADING.size, dateWidth, true);
    const text = wrap(item.text?.trim() || " ", 24, textWidth);
    return { date, text, height: Math.max(date.length * HEADING.lineHeight, text.length * 32) + 36 };
  });
  rows.forEach((row, i) => {
    if (i < visible) {
      body += textLines(row.date, MARGIN + dateWidth, y, { ...HEADING, anchor: "end", fill: INK });
      body += `<circle cx="${axisX}" cy="${y + 18}" r="12" fill="${accent.fill}" stroke="${LINE}" stroke-width="2"/>`;
      body += textLines(row.text, textX, y, { size: 24, lineHeight: 32, fill: INK });
    }
    y += row.height;
  });
  const axis = `<path d="M${axisX} ${start + 18}V${y - rows[rows.length - 1].height + 18}" stroke="${LINE}" stroke-width="3"/>`;
  return { body: svg + axis + body, height: y };
}

function mindmap(doc: MaterialDraft, visible: number, accent: Accent): Layout {
  const cardWidth = 350;
  const leftX = MARGIN - 20;
  const rightX = WIDTH - leftX - cardWidth;
  const gap = 28;
  const cards = doc.content.items.map((item) => measure(item, cardWidth));
  // Tarmoqlar navbat bilan o‘ng va chap ustunga joylashadi
  const columnHeight = (side: number) => cards.reduce((sum, c, i) => (i % 2 === side ? sum + c.height + gap : sum), -gap);
  const titleLines = wrap(doc.title, 34, 250, true);
  const centerHeight = Math.max(120, titleLines.length * 42 + 60);
  const height = Math.max(columnHeight(0), columnHeight(1), centerHeight) + 100;
  const cx = WIDTH / 2;
  const cy = height / 2;

  const offsets = [(height - Math.max(columnHeight(0), 0)) / 2, (height - Math.max(columnHeight(1), 0)) / 2];
  let body = "";
  cards.forEach((card, i) => {
    const side = i % 2;
    const x = side === 0 ? rightX : leftX;
    const y = offsets[side];
    offsets[side] += card.height + gap;
    if (i >= visible) return;
    const edgeX = side === 0 ? rightX : leftX + cardWidth;
    const midY = y + card.height / 2;
    const controlX = (cx + edgeX) / 2;
    body += `<path d="M${cx} ${cy}C${controlX} ${cy} ${controlX} ${midY} ${edgeX} ${midY}" fill="none" stroke="${LINE}" stroke-width="3"/>`;
    body += drawCard(card, x, y, cardWidth, "#ffffff", INK);
  });

  const centerWidth = 300;
  body +=
    `<rect x="${cx - centerWidth / 2}" y="${cy - centerHeight / 2}" width="${centerWidth}" height="${centerHeight}" rx="${Math.min(centerHeight / 2, 40)}" fill="${accent.fill}" stroke="${LINE}" stroke-width="2"/>` +
    textLines(titleLines, cx, cy - (titleLines.length * 42) / 2 - 2, { size: 34, bold: true, anchor: "middle", lineHeight: 42, fill: accent.ink });

  return { body, height };
}

function comparison(doc: MaterialDraft, visible: number, accent: Accent): Layout {
  const { svg, bottom } = titleBlock(doc.title, 48);
  const columns = doc.content.columns ?? [];
  const labelWidth = 260;
  const tableWidth = WIDTH - MARGIN * 2;
  const cellWidth = (tableWidth - labelWidth) / Math.max(columns.length, 1);
  const padding = 16;
  const start = bottom + 32;

  const headerLines = columns.map((c) => wrap(c, HEADING.size, cellWidth - padding * 2, true));
  const headerHeight = Math.max(...headerLines.map((l) => l.length), 1) * HEADING.lineHeight + padding * 2;

  let body = `<rect x="${MARGIN}" y="${start}" width="${tableWidth}" height="${headerHeight}" fill="${accent.fill}" stroke="${LINE}" stroke-width="2"/>`;
  headerLines.forEach((lines, c) => {
    const x = MARGIN + labelWidth + cellWidth * c;
    body += `<path d="M${x} ${start}V${start + headerHeight}" stroke="${LINE}" stroke-width="2"/>`;
    body += textLines(lines, x + cellWidth / 2, start + padding, { ...HEADING, anchor: "middle", fill: accent.ink });
  });

  let y = start + headerHeight;
  doc.content.items.forEach((item, i) => {
    const label = wrap(item.heading, 24, labelWidth - padding * 2, true);
    const cells = columns.map((_, c) => wrap(item.cells?.[c]?.trim() || " ", 24, cellWidth - padding * 2));
    const rowHeight = Math.max(label.length, ...cells.map((l) => l.length), 1) * 32 + padding * 2;
    if (i < visible) {
      body += `<rect x="${MARGIN}" y="${y}" width="${tableWidth}" height="${rowHeight}" fill="${i % 2 === 0 ? "#ffffff" : SOFT}" stroke="${LINE}" stroke-width="2"/>`;
      body += textLines(label, MARGIN + padding, y + padding, { size: 24, bold: true, lineHeight: 32 });
      cells.forEach((lines, c) => {
        const x = MARGIN + labelWidth + cellWidth * c;
        body += `<path d="M${x} ${y}V${y + rowHeight}" stroke="${LINE}" stroke-width="2"/>`;
        body += textLines(lines, x + cellWidth / 2, y + padding, { size: 24, lineHeight: 32, anchor: "middle" });
      });
    }
    y += rowHeight;
  });

  return { body: svg + body, height: y };
}

function infographic(doc: MaterialDraft, visible: number, accent: Accent): Layout {
  const titleLines = wrap(doc.title, 46, WIDTH - MARGIN * 2, true);
  const bannerHeight = titleLines.length * 56 + 64;
  let body =
    `<rect x="0" y="0" width="${WIDTH}" height="${bannerHeight}" fill="${accent.fill}"/>` +
    textLines(titleLines, WIDTH / 2, 28, { size: 46, bold: true, anchor: "middle", lineHeight: 56, fill: accent.ink });

  const gap = 28;
  const cardWidth = (WIDTH - MARGIN * 2 - gap) / 2;
  let y = bannerHeight + 36;
  const items = doc.content.items;
  for (let i = 0; i < items.length; i += 2) {
    const pair = [items[i], items[i + 1]].filter(Boolean);
    const cards = pair.map((item) => measure({ ...item, heading: `${items.indexOf(item) + 1}. ${item.heading}` }, cardWidth));
    const rowHeight = Math.max(...cards.map((c) => c.height));
    cards.forEach((card, c) => {
      if (i + c < visible) {
        body += drawCard({ ...card, height: rowHeight }, MARGIN + (cardWidth + gap) * c, y, cardWidth, "#ffffff", INK);
      }
    });
    y += rowHeight + gap;
  }
  return { body, height: y - gap };
}

type Accent = (typeof ACCENTS)[number];

const LAYOUTS = { process, timeline, mindmap, comparison, infographic };

export type RenderedMaterial = { svg: string; width: number; height: number };

/** @param visible Ko‘rinadigan elementlar soni (darsda bosqichma-bosqich ochish). Joylashuv o‘zgarmaydi. */
export function renderMaterial(doc: MaterialDraft, visible = Number.POSITIVE_INFINITY): RenderedMaterial {
  const accent = ACCENTS.find((a) => a.key === doc.content.accent) ?? ACCENTS[0];
  const layout = LAYOUTS[doc.templateKey](doc, visible, accent);

  let height = layout.height + 32;
  let note = "";
  const noteText = doc.content.note?.trim();
  if (noteText) {
    const lines = wrap(noteText, 22, WIDTH - MARGIN * 2 - 40);
    const boxHeight = lines.length * 30 + 32;
    note =
      `<rect x="${MARGIN}" y="${height}" width="${WIDTH - MARGIN * 2}" height="${boxHeight}" rx="12" fill="${SOFT}"/>` +
      textLines(lines, MARGIN + 20, height + 14, { size: 22, lineHeight: 30, fill: INK });
    height += boxHeight + 32;
  }

  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${WIDTH}" height="${height}" viewBox="0 0 ${WIDTH} ${height}"><rect width="${WIDTH}" height="${height}" fill="#ffffff"/>${layout.body}${note}</svg>`;
  return { svg, width: WIDTH, height };
}

export const svgDataUri = (svg: string) => `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`;

/** Rasm uchun matnli tavsif: ekran o‘quvchilari sxema mazmunini shu orqali eshitadi. */
export function describeMaterial(doc: MaterialDraft): string {
  const items = doc.content.items.map((item) => {
    const cells = item.cells?.length && doc.content.columns ? item.cells.map((c, i) => `${doc.content.columns![i]}: ${c}`).join(", ") : item.text;
    return cells ? `${item.heading} (${cells})` : item.heading;
  });
  return `${doc.title}. ${items.join("; ")}.`;
}
