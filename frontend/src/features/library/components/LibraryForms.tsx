"use client";

import { useActionState, useState, useSyncExternalStore } from "react";
import { SUBJECTS } from "@/features/studio/lib/templates";
import { addLinkAction, deleteItemAction, shareAction, unshareAction, updateItemAction, uploadFileAction, type LibraryFormState } from "../actions";
import { sharePath } from "../format";
import { ACCEPT, type LibraryItem, type ShareKind } from "../types";

const initial: LibraryFormState = {};

function Message({ state }: { state: LibraryFormState }) {
  if (!state.message) return null;
  return (
    <p className={`tv-form-msg ${state.ok ? "ok" : "err"}`} role={state.ok ? "status" : "alert"}>
      {state.message}
    </p>
  );
}

function MetaFields({ item, idPrefix }: { item?: LibraryItem; idPrefix: string }) {
  return (
    <>
      <div className="tv-field">
        <label htmlFor={`${idPrefix}-title`}>Nomi</label>
        <input id={`${idPrefix}-title`} name="title" defaultValue={item?.title} maxLength={120} placeholder="Masalan: Fotosintez — 7-sinf taqdimoti" />
      </div>
      <div className="tv-lib-row">
        <div className="tv-field">
          <label htmlFor={`${idPrefix}-subject`}>Fan</label>
          <input id={`${idPrefix}-subject`} name="subject" list="tv-lib-subjects" defaultValue={item?.subject ?? ""} maxLength={40} placeholder="Biologiya" />
        </div>
        <div className="tv-field">
          <label htmlFor={`${idPrefix}-topic`}>Mavzu</label>
          <input id={`${idPrefix}-topic`} name="topic" defaultValue={item?.topic ?? ""} maxLength={80} placeholder="Fotosintez" />
        </div>
      </div>
    </>
  );
}

export function SubjectOptions() {
  return (
    <datalist id="tv-lib-subjects">
      {SUBJECTS.map((s) => (
        <option key={s} value={s} />
      ))}
    </datalist>
  );
}

/** Kabinetga fayl (PDF, PowerPoint, rasm) yoki havola qo‘shish. */
export function AddMaterialForm({ maxFileMb }: { maxFileMb: number }) {
  const [mode, setMode] = useState<"file" | "link">("file");
  const [fileState, fileAction, filePending] = useActionState(uploadFileAction, initial);
  const [linkState, linkAction, linkPending] = useActionState(addLinkAction, initial);
  const state = mode === "file" ? fileState : linkState;
  const pending = mode === "file" ? filePending : linkPending;

  return (
    <section className="tv-card tv-lib-add" aria-labelledby="qoshish-t">
      <h2 className="tv-h3" id="qoshish-t">
        Tayyor material qöşiş
      </h2>
      <div className="tv-filter" role="group" aria-label="Material turi">
        <button type="button" className="tv-lib-tab" aria-pressed={mode === "file"} onClick={() => setMode("file")}>
          Fayl yuklaş
        </button>
        <button type="button" className="tv-lib-tab" aria-pressed={mode === "link"} onClick={() => setMode("link")}>
          Havola qöşiş
        </button>
      </div>
      <form key={`${mode}-${state.key ?? 0}`} action={mode === "file" ? fileAction : linkAction} className="tv-form">
        {mode === "file" ? (
          <div className="tv-field">
            <label htmlFor="lib-file">Fayl</label>
            <input id="lib-file" name="file" type="file" accept={ACCEPT} required aria-describedby="lib-file-hint" />
            <p className="tv-field-hint" id="lib-file-hint">
              PDF, PowerPoint (PPTX, PPT) yoki rasm (PNG, JPG, WEBP), {maxFileMb} MB gaça. Nomini yozmasangiz, fayl nomi olinadi.
            </p>
          </div>
        ) : (
          <div className="tv-field">
            <label htmlFor="lib-url">Havola</label>
            <input id="lib-url" name="url" type="url" inputMode="url" required placeholder="https://docs.google.com/presentation/d/…" aria-describedby="lib-url-hint" />
            <p className="tv-field-hint" id="lib-url-hint">
              Google Slides, Canva, Genially, YouTube yoki boşqa sayt. Material öşa saytda ham “havolasi bor hamma köra oladi” deb oçilgan bölsin.
            </p>
          </div>
        )}
        <MetaFields idPrefix={`lib-new-${mode}`} />
        <Message state={state} />
        <div>
          <button className="tv-btn tv-btn-primary" type="submit" disabled={pending}>
            {pending ? "Saqlanmoqda…" : mode === "file" ? "Yuklaş" : "Saqlaş"}
          </button>
        </div>
      </form>
    </section>
  );
}

export function EditItemForm({ item }: { item: LibraryItem }) {
  const [state, action, pending] = useActionState(updateItemAction.bind(null, item.id), initial);
  return (
    <details className="tv-lib-edit">
      <summary>Tahrirlaş</summary>
      <form action={action} className="tv-form">
        <MetaFields item={item} idPrefix={`lib-${item.id}`} />
        {item.kind === "link" && (
          <div className="tv-field">
            <label htmlFor={`lib-${item.id}-url`}>Havola</label>
            <input id={`lib-${item.id}-url`} name="url" type="url" defaultValue={item.url ?? ""} required />
          </div>
        )}
        <Message state={state} />
        <div>
          <button className="tv-btn tv-btn-primary tv-btn-sm" type="submit" disabled={pending}>
            {pending ? "Saqlanmoqda…" : "Saqlaş"}
          </button>
        </div>
      </form>
    </details>
  );
}

export function DeleteItemButton({ id, title }: { id: string; title: string }) {
  return (
    <form
      action={deleteItemAction.bind(null, id)}
      onSubmit={(event) => {
        if (!window.confirm(`“${title}” öçirilsinmi? Buni qaytarib bölmaydi.`)) event.preventDefault();
      }}
    >
      <button className="tv-btn tv-btn-text tv-btn-danger" type="submit">
        Öçiriş
      </button>
    </form>
  );
}

const noSubscribe = () => () => {};

/** Ochiq havola: tokenni bilgan har kim kirmasdan ko‘radi. To‘xtatilsa, eski havola ishlamay qoladi. */
export function ShareControl({ kind, id, token: initialToken }: { kind: ShareKind; id: string; token?: string | null }) {
  const [token, setToken] = useState(initialToken ?? null);
  const [message, setMessage] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const origin = useSyncExternalStore(noSubscribe, () => window.location.origin, () => "");
  const url = token ? `${origin}${sharePath(token)}` : null;

  const run = async (action: typeof shareAction) => {
    setBusy(true);
    setMessage(null);
    const result = await action(kind, id);
    setBusy(false);
    if (result.ok) setToken(result.token);
    else setMessage(result.message);
  };

  const copy = async () => {
    if (!url) return;
    try {
      await navigator.clipboard.writeText(url);
      setMessage("Havola nusxalandi.");
    } catch {
      setMessage("Nusxalab bölmadi: havolani belgilab, özingiz nusxalang.");
    }
  };

  return (
    <div className="tv-lib-share">
      {url ? (
        <>
          <label className="tv-sr" htmlFor={`share-${id}`}>
            Ulaşiş havolasi
          </label>
          <input id={`share-${id}`} className="tv-lib-share-url" readOnly value={url} onFocus={(e) => e.currentTarget.select()} />
          <div className="tv-material-actions">
            <button className="tv-btn tv-btn-ghost tv-btn-sm" type="button" onClick={copy}>
              Nusxalaş
            </button>
            <button className="tv-btn tv-btn-text tv-btn-sm" type="button" onClick={() => run(unshareAction)} disabled={busy}>
              Ulaşişni töxtatiş
            </button>
          </div>
        </>
      ) : (
        <button className="tv-btn tv-btn-ghost tv-btn-sm" type="button" onClick={() => run(shareAction)} disabled={busy}>
          {busy ? "…" : "Ulaşiş havolasi"}
        </button>
      )}
      {message && (
        <p className="tv-field-hint" role="status">
          {message}
        </p>
      )}
    </div>
  );
}
