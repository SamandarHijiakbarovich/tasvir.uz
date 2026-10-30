"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useRef, useState, useTransition } from "react";
import { downloadBlob, downloadSvgAsPng, toSafeFileName } from "@/shared/lib/download";
import { saveMaterialAction } from "../actions";
import { describeMaterial, renderMaterial, svgDataUri } from "../lib/render";
import { ACCENTS, SUBJECTS, getTemplate } from "../lib/templates";
import type { Accent, Material, MaterialDraft, MaterialItem } from "../lib/types";

// Tartib o‘zgarganda inputlar fokusni yo‘qotmasligi uchun barqaror kalit
type EditorItem = MaterialItem & { key: number };

const MAX_COLUMNS = 4;
const MIN_COLUMNS = 2;

export function MaterialEditor({ material }: { material: Material }) {
  const template = getTemplate(material.templateKey)!;
  const nextKey = useRef(material.content.items.length);
  const router = useRouter();

  const [title, setTitle] = useState(material.title);
  const [subject, setSubject] = useState(material.subject ?? "");
  const [grade, setGrade] = useState(material.grade?.toString() ?? "");
  const [accent, setAccent] = useState<Accent>(material.content.accent ?? "sun");
  const [note, setNote] = useState(material.content.note ?? "");
  const [columns, setColumns] = useState(material.content.columns ?? []);
  const [items, setItems] = useState<EditorItem[]>(() => material.content.items.map((item, key) => ({ ...item, key })));
  const [dirty, setDirty] = useState(false);
  const [status, setStatus] = useState<{ ok: boolean; text: string } | null>(null);
  const [pending, startTransition] = useTransition();

  const isTable = material.templateKey === "comparison";

  const draft: MaterialDraft = useMemo(
    () => ({
      templateKey: material.templateKey,
      title,
      subject: subject.trim() || null,
      grade: grade ? Number(grade) : null,
      content: {
        accent,
        note: note.trim() || null,
        columns: isTable ? columns : null,
        items: items.map(({ key: _key, ...item }) => item),
      },
    }),
    [material.templateKey, title, subject, grade, accent, note, columns, items, isTable],
  );

  // Yozish paytida preview bo‘sh joyda ham buzilmasin: bo‘sh nom o‘rniga belgi ko‘rsatiladi
  const preview = useMemo(() => {
    const safe: MaterialDraft = {
      ...draft,
      title: draft.title.trim() || "Sarlavha",
      content: { ...draft.content, items: draft.content.items.map((item) => ({ ...item, heading: item.heading.trim() || "…" })) },
    };
    return { ...renderMaterial(safe), alt: describeMaterial(safe) };
  }, [draft]);

  // Saqlanmagan o‘zgarish bilan sahifa yopilsa, brauzer ogohlantiradi
  useEffect(() => {
    if (!dirty) return;
    const warn = (event: BeforeUnloadEvent) => event.preventDefault();
    window.addEventListener("beforeunload", warn);
    return () => window.removeEventListener("beforeunload", warn);
  }, [dirty]);

  const touch = () => {
    setDirty(true);
    setStatus(null);
  };

  const updateItem = (key: number, patch: Partial<MaterialItem>) => {
    setItems((all) => all.map((item) => (item.key === key ? { ...item, ...patch } : item)));
    touch();
  };

  const moveItem = (index: number, delta: number) => {
    setItems((all) => {
      const next = [...all];
      const [item] = next.splice(index, 1);
      next.splice(index + delta, 0, item);
      return next;
    });
    touch();
  };

  const removeItem = (key: number) => {
    setItems((all) => all.filter((item) => item.key !== key));
    touch();
  };

  const addItem = () => {
    setItems((all) => [...all, { key: nextKey.current++, heading: "", text: "", cells: isTable ? columns.map(() => "") : null }]);
    touch();
  };

  const updateCell = (key: number, column: number, value: string) => {
    setItems((all) => all.map((item) => (item.key === key ? { ...item, cells: (item.cells ?? []).map((c, i) => (i === column ? value : c)) } : item)));
    touch();
  };

  const addColumn = () => {
    setColumns((all) => [...all, `${all.length + 1}-ustun`]);
    setItems((all) => all.map((item) => ({ ...item, cells: [...(item.cells ?? []), ""] })));
    touch();
  };

  const removeColumn = (column: number) => {
    setColumns((all) => all.filter((_, i) => i !== column));
    setItems((all) => all.map((item) => ({ ...item, cells: (item.cells ?? []).filter((_, i) => i !== column) })));
    touch();
  };

  const save = (then?: () => void) => {
    setStatus(null);
    startTransition(async () => {
      const response = await saveMaterialAction(material.id, draft);
      if (!response.ok) {
        if (response.sessionExpired) {
          setStatus({ ok: false, text: "Sessiya tugagan. O‘zgarishlaringizni nusxalab oling va qayta kiring." });
        } else {
          setStatus({ ok: false, text: response.message });
        }
        return;
      }
      setDirty(false);
      setStatus({ ok: true, text: "Saqlandi." });
      then?.();
    });
  };

  const fileName = toSafeFileName(title);

  return (
    <div className="tv-editor">
      <div className="tv-editor-bar">
        <p className="tv-breadcrumb">
          <Link href="/studiya">Materiallarim</Link> · {template.name}
        </p>
        <div className="tv-editor-actions">
          <span className="tv-editor-status" role="status">
            {status ? status.text : dirty ? "Saqlanmagan o‘zgarishlar bor" : ""}
          </span>
          <button className="tv-btn tv-btn-primary tv-btn-sm" type="button" disabled={pending || !dirty} onClick={() => save()}>
            {pending ? "Saqlanmoqda…" : "Saqlash"}
          </button>
          <button
            className="tv-btn tv-btn-ghost tv-btn-sm"
            type="button"
            disabled={pending}
            onClick={() => {
              const open = () => router.push(`/studiya/${material.id}/korsatish`);
              if (dirty) save(open);
              else open();
            }}
          >
            Darsda ko‘rsatish
          </button>
        </div>
      </div>

      {status && !status.ok && (
        <p className="tv-form-msg err" role="alert">
          {status.text}
        </p>
      )}

      <div className="tv-editor-grid">
        <div className="tv-editor-form">
          <div className="tv-field">
            <label htmlFor="material-title">Sarlavha</label>
            <input
              id="material-title"
              value={title}
              maxLength={120}
              onChange={(e) => {
                setTitle(e.target.value);
                touch();
              }}
            />
          </div>

          <div className="tv-editor-row">
            <div className="tv-field">
              <label htmlFor="material-subject">Fan</label>
              <input
                id="material-subject"
                list="material-subjects"
                value={subject}
                maxLength={40}
                onChange={(e) => {
                  setSubject(e.target.value);
                  touch();
                }}
              />
              <datalist id="material-subjects">
                {SUBJECTS.map((s) => (
                  <option key={s} value={s} />
                ))}
              </datalist>
            </div>
            <div className="tv-field">
              <label htmlFor="material-grade">Sinf</label>
              <select
                id="material-grade"
                value={grade}
                onChange={(e) => {
                  setGrade(e.target.value);
                  touch();
                }}
              >
                <option value="">Tanlanmagan</option>
                {Array.from({ length: 11 }, (_, i) => i + 1).map((g) => (
                  <option key={g} value={g}>
                    {g}-sinf
                  </option>
                ))}
              </select>
            </div>
          </div>

          <fieldset className="tv-accents">
            <legend>Asosiy rang</legend>
            {ACCENTS.map((option) => (
              <label key={option.key} className={accent === option.key ? "on" : undefined}>
                <input
                  type="radio"
                  name="material-accent"
                  checked={accent === option.key}
                  onChange={() => {
                    setAccent(option.key);
                    touch();
                  }}
                />
                <i style={{ background: option.fill }} aria-hidden="true" />
                {option.label}
              </label>
            ))}
          </fieldset>

          {isTable && (
            <fieldset className="tv-editor-items">
              <legend>Ustunlar</legend>
              {columns.map((column, c) => (
                <div className="tv-editor-line" key={c}>
                  <input
                    aria-label={`${c + 1}-ustun nomi`}
                    value={column}
                    maxLength={80}
                    onChange={(e) => {
                      setColumns((all) => all.map((v, i) => (i === c ? e.target.value : v)));
                      touch();
                    }}
                  />
                  {columns.length > MIN_COLUMNS && (
                    <button className="tv-iconbtn" type="button" aria-label={`${c + 1}-ustunni o‘chirish`} onClick={() => removeColumn(c)}>
                      <svg viewBox="0 0 24 24" aria-hidden="true">
                        <path d="M6 6l12 12M18 6L6 18" />
                      </svg>
                    </button>
                  )}
                </div>
              ))}
              {columns.length < MAX_COLUMNS && (
                <button className="tv-btn tv-btn-text" type="button" onClick={addColumn}>
                  + Ustun qo‘shish
                </button>
              )}
            </fieldset>
          )}

          <fieldset className="tv-editor-items">
            <legend>
              {template.itemName}lar ({items.length} / {template.maxItems})
            </legend>
            {items.map((item, index) => (
              <div className="tv-editor-item" key={item.key}>
                <div className="tv-editor-item-head">
                  <strong>
                    {index + 1}. {template.itemName.toLowerCase()}
                  </strong>
                  <div>
                    <button className="tv-iconbtn" type="button" disabled={index === 0} aria-label={`${index + 1}-elementni yuqoriga surish`} onClick={() => moveItem(index, -1)}>
                      <svg viewBox="0 0 24 24" aria-hidden="true">
                        <path d="M12 19V5M6 11l6-6 6 6" />
                      </svg>
                    </button>
                    <button
                      className="tv-iconbtn"
                      type="button"
                      disabled={index === items.length - 1}
                      aria-label={`${index + 1}-elementni pastga surish`}
                      onClick={() => moveItem(index, 1)}
                    >
                      <svg viewBox="0 0 24 24" aria-hidden="true">
                        <path d="M12 5v14M6 13l6 6 6-6" />
                      </svg>
                    </button>
                    <button
                      className="tv-iconbtn"
                      type="button"
                      disabled={items.length <= template.minItems}
                      aria-label={`${index + 1}-elementni o‘chirish`}
                      onClick={() => removeItem(item.key)}
                    >
                      <svg viewBox="0 0 24 24" aria-hidden="true">
                        <path d="M6 6l12 12M18 6L6 18" />
                      </svg>
                    </button>
                  </div>
                </div>
                <div className="tv-field">
                  <label htmlFor={`item-heading-${item.key}`}>{template.headingLabel}</label>
                  <input id={`item-heading-${item.key}`} value={item.heading} maxLength={80} onChange={(e) => updateItem(item.key, { heading: e.target.value })} />
                </div>
                {template.textLabel && (
                  <div className="tv-field">
                    <label htmlFor={`item-text-${item.key}`}>{template.textLabel}</label>
                    <textarea id={`item-text-${item.key}`} rows={2} value={item.text ?? ""} maxLength={240} onChange={(e) => updateItem(item.key, { text: e.target.value })} />
                  </div>
                )}
                {isTable &&
                  columns.map((column, c) => (
                    <div className="tv-field" key={c}>
                      <label htmlFor={`item-cell-${item.key}-${c}`}>{column || `${c + 1}-ustun`}</label>
                      <input id={`item-cell-${item.key}-${c}`} value={item.cells?.[c] ?? ""} maxLength={80} onChange={(e) => updateCell(item.key, c, e.target.value)} />
                    </div>
                  ))}
              </div>
            ))}
            {items.length < template.maxItems && (
              <button className="tv-btn tv-btn-text" type="button" onClick={addItem}>
                + {template.itemName} qo‘shish
              </button>
            )}
          </fieldset>

          <div className="tv-field">
            <label htmlFor="material-note">{template.noteLabel}</label>
            <textarea
              id="material-note"
              rows={2}
              value={note}
              maxLength={240}
              onChange={(e) => {
                setNote(e.target.value);
                touch();
              }}
            />
          </div>

          <p className="tv-callout tv-editor-tip">
            <strong>Tekshiring:</strong> yozuvlar qisqami, ortiqcha element yo‘qmi, ilmiy jihatdan to‘g‘rimi? Har bir element aniq vazifani bajarsin.
          </p>
        </div>

        <aside className="tv-editor-preview" aria-label="Material ko‘rinishi">
          <div className="tv-print-area">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={svgDataUri(preview.svg)} alt={preview.alt} />
          </div>
          <div className="tv-download">
            <button
              className="tv-btn tv-btn-ghost tv-btn-sm"
              type="button"
              onClick={() => {
                downloadSvgAsPng(preview.svg, preview.width, preview.height, `${fileName}.png`).catch(() =>
                  setStatus({ ok: false, text: "PNG yaratib bo‘lmadi. SVG faylni yuklab oling." }),
                );
              }}
            >
              PNG yuklab olish
            </button>
            <button className="tv-btn tv-btn-ghost tv-btn-sm" type="button" onClick={() => downloadBlob(new Blob([preview.svg], { type: "image/svg+xml" }), `${fileName}.svg`)}>
              SVG yuklab olish
            </button>
            <button className="tv-btn tv-btn-ghost tv-btn-sm" type="button" onClick={() => window.print()}>
              Chop etish / PDF
            </button>
          </div>
        </aside>
      </div>
    </div>
  );
}
