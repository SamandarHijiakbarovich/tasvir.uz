"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { useFavoriteTools } from "../favorites";
import type { Subject, ToolCatalog, VisualTool } from "../types";

const ACCESS: Record<VisualTool["access"], string> = { free: "Bepul", freemium: "Qisman bepul", paid: "Pullik" };
const LEVEL: Record<VisualTool["level"], string> = { easy: "Oson", medium: "Örta", coding: "Dasturlaş kerak" };

type AccessFilter = "all" | "free" | "freemium";
const ACCESS_FILTERS: { key: AccessFilter; label: string }[] = [
  { key: "all", label: "Barçasi" },
  { key: "free", label: "Faqat bepul" },
  { key: "freemium", label: "Qisman bepul" },
];

/** Qidiruv uchun: katta-kichik harf va o‘zbek harflari farqi (ö/o, ş/sh) hisobga olinmaydi. */
function normalize(text: string) {
  return text
    .toLocaleLowerCase("uz")
    .replace(/[‘’`']/g, "")
    .replace(/ş/g, "sh")
    .replace(/ç/g, "ch")
    .replace(/ö/g, "o")
    .replace(/ğ/g, "g");
}

/**
 * Dasturlar katalogi: fan (URL orqali), nom bo‘yicha qidiruv, narx filtri va sevimlilar.
 * Sevimlilar shu brauzerda saqlanadi, hisobga kirish shart emas.
 */
export function ToolCatalogBrowser({ catalog, subject }: { catalog: ToolCatalog; subject?: Subject }) {
  const [query, setQuery] = useState("");
  const [access, setAccess] = useState<AccessFilter>("all");
  const [onlyFavorites, setOnlyFavorites] = useState(false);
  const favorites = useFavoriteTools();

  const subjectName = (key: string) => catalog.subjects.find((s) => s.key === key)?.name ?? key;
  const shapeName = (key: string) => catalog.shapes.find((s) => s.key === key)?.name ?? key;
  const studioShapes = useMemo(() => new Set(catalog.shapes.filter((s) => s.inStudio).map((s) => s.key)), [catalog.shapes]);

  const matches = (tool: VisualTool) => {
    if (access !== "all" && tool.access !== access) return false;
    if (onlyFavorites && !favorites.has(tool.key)) return false;
    const q = normalize(query.trim());
    if (!q) return true;
    const categoryTitle = catalog.categories.find((c) => c.key === tool.category)?.title ?? "";
    return [tool.name, tool.summary, categoryTitle, ...tool.shapes.map(shapeName)].some((text) => normalize(text).includes(q));
  };

  const special = (subject ? catalog.tools.filter((t) => t.subjects.includes(subject.key)) : catalog.tools).filter(matches);
  const universal = (subject ? catalog.tools.filter((t) => t.subjects.length === 0) : []).filter(matches);
  const shown = special.length + universal.length;

  const card = (tool: VisualTool) => {
    const studioShape = tool.shapes.find((s) => studioShapes.has(s));
    const saved = favorites.has(tool.key);
    return (
      <li key={tool.key} className="tv-card tv-tool">
        <div className="tv-tool-head">
          <h4 className="tv-tool-name">{tool.name}</h4>
          <span className={`tv-tool-access ${tool.access}`}>{ACCESS[tool.access]}</span>
        </div>
        <p>{tool.summary}</p>
        <p>
          <strong>Darsda:</strong> {tool.classroomUse}
        </p>
        <dl className="tv-tool-facts">
          <div>
            <dt>Şakllar</dt>
            <dd>{tool.shapes.map(shapeName).join(", ")}</dd>
          </div>
          <div>
            <dt>Fanlar</dt>
            <dd>{tool.subjects.length === 0 ? "Har qanday fan" : tool.subjects.map(subjectName).join(", ")}</dd>
          </div>
          <div>
            <dt>Örganiş</dt>
            <dd>{LEVEL[tool.level]}</dd>
          </div>
          <div>
            <dt>Narxi</dt>
            <dd>{tool.accessNote}</dd>
          </div>
        </dl>
        <div className="tv-tool-actions">
          <a className="tv-tool-link" href={tool.url} target="_blank" rel="noopener noreferrer">
            Rasmiy sayt<span className="tv-sr"> (yangi oynada oçiladi)</span> ↗
          </a>
          {studioShape && (
            <Link className="tv-tool-studio" href={`/studiya/yangi#${studioShape}`}>
              Studiyada yasaş →
            </Link>
          )}
          <button
            type="button"
            className={`tv-tool-fav${saved ? " on" : ""}`}
            aria-pressed={saved}
            onClick={() => favorites.toggle(tool.key)}
            title={saved ? "Sevimlilardan olib taşlaş" : "Sevimlilarga qöşiş"}
          >
            <span aria-hidden="true">{saved ? "★" : "☆"}</span>
            <span className="tv-sr">{tool.name}: </span>
            {saved ? "Saqlangan" : "Saqlaş"}
          </button>
        </div>
      </li>
    );
  };

  const grouped = (tools: VisualTool[]) =>
    catalog.categories
      .map((category) => ({ category, tools: tools.filter((t) => t.category === category.key) }))
      .filter((g) => g.tools.length > 0)
      .map(({ category, tools }) => (
        <div key={category.key} className="tv-tools-group">
          <h3 className="tv-tools-group-t">{category.title}</h3>
          <p className="tv-muted" style={{ margin: 0 }}>
            {category.description}
          </p>
          <ul className="tv-tools-grid">{tools.map(card)}</ul>
        </div>
      ));

  return (
    <section className="tv-tools-section" aria-labelledby="katalog-t" id="katalog">
      <h2 className="tv-h3" id="katalog-t">
        Dasturlar katalogi ({catalog.tools.length})
      </h2>

      <div className="tv-tools-search" role="search">
        <div className="tv-field">
          <label htmlFor="vosita-qidiruv">Dastur nomi böyiça qidiruv</label>
          <input
            id="vosita-qidiruv"
            type="search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Masalan: GeoGebra, infografika, xarita"
            autoComplete="off"
          />
        </div>
        <fieldset className="tv-tools-seg">
          <legend>Narxi</legend>
          {ACCESS_FILTERS.map((f) => (
            <label key={f.key} className={access === f.key ? "on" : undefined}>
              <input type="radio" name="narx" value={f.key} checked={access === f.key} onChange={() => setAccess(f.key)} />
              {f.label}
            </label>
          ))}
        </fieldset>
        <button type="button" className={`tv-tool-fav${onlyFavorites ? " on" : ""}`} aria-pressed={onlyFavorites} onClick={() => setOnlyFavorites((v) => !v)}>
          <span aria-hidden="true">★</span> Sevimlilar ({favorites.size})
        </button>
      </div>

      <nav className="tv-filter" aria-label="Fan böyiça saralaş">
        <Link href="/vositalar#katalog" aria-current={!subject}>
          Barça fanlar
        </Link>
        {catalog.subjects.map((s) => (
          <Link key={s.key} href={`/vositalar?fan=${s.key}#katalog`} aria-current={subject?.key === s.key}>
            {s.name}
          </Link>
        ))}
      </nav>

      <p className="tv-muted" aria-live="polite" style={{ margin: 0 }}>
        {shown === 0
          ? onlyFavorites && favorites.size === 0
            ? "Hali sevimli dastur yöq. Kartadagi “☆ Saqlaş” tugmasini bosing."
            : "Heç narsa topilmadi. Qidiruv sözini yoki filtrni özgartiring."
          : `Körsatilmoqda: ${shown} ta dastur`}
      </p>

      {subject && special.length > 0 && <h3 className="tv-h3">{subject.name} uçun</h3>}
      {grouped(special)}
      {universal.length > 0 && (
        <>
          <h3 className="tv-h3">Har qanday fanda işlatiladigan dasturlar</h3>
          {grouped(universal)}
        </>
      )}
    </section>
  );
}
