import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { getLibrary, getMyShares } from "@/features/library/api";
import { AddMaterialForm, DeleteItemButton, EditItemForm, ShareControl, SubjectOptions } from "@/features/library/components/LibraryForms";
import { formatSize, kindLabel } from "@/features/library/format";
import type { LibraryItem } from "@/features/library/types";
import { listMaterials } from "@/features/studio/api/studio-api";
import { StudioHeader } from "@/features/studio/components/StudioHeader";
import { describeMaterial, renderMaterial, svgDataUri } from "@/features/studio/lib/render";
import { getTemplate } from "@/features/studio/lib/templates";
import type { Material } from "@/features/studio/lib/types";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/studio.css";
import "@/styles/library.css";

export const metadata: Metadata = { title: "Mening materiallarim — Tasvir.uz", robots: { index: false, follow: false } };

const dateFormat = new Intl.DateTimeFormat("uz-UZ", { dateStyle: "medium", timeZone: "Asia/Tashkent" });
const utc = (value: string) => new Date(/Z$/.test(value) ? value : `${value}Z`);

type Props = { searchParams: Promise<{ fan?: string; q?: string; xato?: string }> };

const matches = (q: string, ...values: (string | null | undefined)[]) => values.some((v) => v?.toLocaleLowerCase("uz").includes(q));

export default async function MyMaterialsPage({ searchParams }: Props) {
  const { fan, q: rawQuery, xato } = await searchParams;
  const [library, materials, shares] = await Promise.all([getLibrary(), listMaterials(), getMyShares()]);
  if ([library, materials, shares].some((r) => !r.ok && r.status === 401)) redirect("/kirish?keyin=/kabinet/materiallar");

  const items = library.ok ? library.data.items : [];
  const studio = materials.ok ? materials.data : [];
  const materialShares = new Map((shares.ok ? shares.data : []).filter((s) => s.kind === "material").map((s) => [s.id, s.token]));

  const subjects = [...new Set([...items.map((i) => i.subject), ...studio.map((m) => m.subject)].filter((s): s is string => Boolean(s)))].sort();
  const q = rawQuery?.trim().toLocaleLowerCase("uz") ?? "";
  const shownItems = items.filter((i) => (!fan || i.subject === fan) && (!q || matches(q, i.title, i.topic, i.subject, i.fileName)));
  const shownStudio = studio.filter((m) => (!fan || m.subject === fan) && (!q || matches(q, m.title, m.subject)));
  const error = xato ?? (!library.ok ? library.message : !materials.ok ? materials.message : null);
  const filterHref = (subject?: string) => {
    const params = new URLSearchParams();
    if (subject) params.set("fan", subject);
    if (rawQuery) params.set("q", rawQuery);
    const query = params.toString();
    return `/kabinet/materiallar${query ? `?${query}` : ""}`;
  };

  return (
    <div className="tv-studio">
      <StudioHeader />
      <main className="tv-studio-in">
        <div className="tv-studio-head">
          <div>
            <p className="tv-breadcrumb">
              <Link href="/kabinet">Kabinet</Link>
            </p>
            <h1 className="tv-h2" style={{ marginTop: 8 }}>
              Mening materiallarim
            </h1>
            <p className="tv-muted">
              Studiyada yaratgan işlaringiz va tayyor taqdimotlaringiz bir joyda. Istalgan kompyuter yoki telefondan kirib, darsda töliq ekranda körsating.
            </p>
          </div>
          <Link className="tv-btn tv-btn-primary" href="/studiya/yangi">
            Studiyada yaratiş +
          </Link>
        </div>

        {error && (
          <p className="tv-form-msg err" role="alert">
            {error}
          </p>
        )}

        <SubjectOptions />
        <AddMaterialForm maxFileMb={library.ok ? Math.round(library.data.maxFileBytes / 1024 / 1024) : 4} />

        <form className="tv-lib-search" role="search" action="/kabinet/materiallar">
          {fan && <input type="hidden" name="fan" value={fan} />}
          <div className="tv-field">
            <label htmlFor="lib-q">Qidiruv</label>
            <input id="lib-q" name="q" type="search" defaultValue={rawQuery} placeholder="Nomi, mavzusi yoki fani" />
          </div>
          <button className="tv-btn tv-btn-ghost" type="submit">
            Izlaş
          </button>
        </form>

        {subjects.length > 0 && (
          <nav className="tv-filter" aria-label="Fan böyiça saralaş">
            <Link href={filterHref()} aria-current={!fan}>
              Hammasi
            </Link>
            {subjects.map((subject) => (
              <Link key={subject} href={filterHref(subject)} aria-current={fan === subject}>
                {subject}
              </Link>
            ))}
          </nav>
        )}

        <section className="tv-lib-section" aria-labelledby="yuklangan-t">
          <div className="tv-lib-section-head">
            <h2 className="tv-h3" id="yuklangan-t">
              Yuklangan fayl va havolalar ({shownItems.length})
            </h2>
            {library.ok && (
              <p className="tv-field-hint">
                Band: {formatSize(library.data.usedBytes)} / {formatSize(library.data.quotaBytes)}
              </p>
            )}
          </div>
          {shownItems.length === 0 ? (
            <p className="tv-muted">{items.length === 0 ? "Hali fayl yoki havola saqlanmagan. Yuqoridagi formadan qöşing." : "Bu filtr böyiça heç narsa topilmadi."}</p>
          ) : (
            <ul className="tv-material-grid">{shownItems.map((item) => <ItemCard key={item.id} item={item} />)}</ul>
          )}
        </section>

        <section className="tv-lib-section" aria-labelledby="studiya-t">
          <div className="tv-lib-section-head">
            <h2 className="tv-h3" id="studiya-t">
              Studiyada yaratilganlar ({shownStudio.length})
            </h2>
            <Link href="/studiya">Studiyada oçiş →</Link>
          </div>
          {shownStudio.length === 0 ? (
            <p className="tv-muted">
              {studio.length === 0 ? (
                <>
                  Hali material yöq. <Link href="/studiya/yangi">Şablondan sxema yoki infografika yarating</Link> — u şu yerda avtomatik saqlanadi.
                </>
              ) : (
                "Bu filtr böyiça heç narsa topilmadi."
              )}
            </p>
          ) : (
            <ul className="tv-material-grid">
              {shownStudio.map((material) => (
                <StudioCard key={material.id} material={material} token={materialShares.get(material.id)} />
              ))}
            </ul>
          )}
        </section>
      </main>
    </div>
  );
}

function ItemCard({ item }: { item: LibraryItem }) {
  return (
    <li className="tv-card tv-material-card tv-lib-card">
      <span className={`tv-lib-kind ${item.kind}`}>{kindLabel(item)}</span>
      <h3 className="tv-h3">
        <Link href={`/kabinet/materiallar/${item.id}`}>{item.title}</Link>
      </h3>
      <p className="tv-material-meta">
        {[item.subject, item.topic].filter(Boolean).join(" · ") || "Fan va mavzu körsatilmagan"}
        <br />
        {item.kind === "file" ? `${item.fileName} · ${formatSize(item.sizeBytes)}` : item.url && new URL(item.url).hostname.replace(/^www\./, "")}
        {" · "}
        {dateFormat.format(utc(item.updatedAt))}
      </p>
      <div className="tv-material-actions">
        <Link className="tv-btn tv-btn-primary tv-btn-sm" href={`/kabinet/materiallar/${item.id}`}>
          Körsatiş
        </Link>
        <DeleteItemButton id={item.id} title={item.title} />
      </div>
      <ShareControl kind="item" id={item.id} token={item.shareToken} />
      <EditItemForm item={item} />
    </li>
  );
}

function StudioCard({ material, token }: { material: Material; token?: string }) {
  return (
    <li className="tv-card tv-material-card">
      <Link className="tv-material-thumb" href={`/studiya/${material.id}`} tabIndex={-1} aria-hidden="true">
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={svgDataUri(renderMaterial(material).svg)} alt={describeMaterial(material)} loading="lazy" />
      </Link>
      <h3 className="tv-h3">
        <Link href={`/studiya/${material.id}`}>{material.title}</Link>
      </h3>
      <p className="tv-material-meta">
        {[getTemplate(material.templateKey)?.name, material.subject, material.grade ? `${material.grade}-sinf` : null].filter(Boolean).join(" · ")}
        <br />
        Özgartirilgan: {dateFormat.format(utc(material.updatedAt))}
      </p>
      <div className="tv-material-actions">
        <Link className="tv-btn tv-btn-primary tv-btn-sm" href={`/studiya/${material.id}/korsatish`}>
          Körsatiş
        </Link>
        <Link className="tv-btn tv-btn-ghost tv-btn-sm" href={`/studiya/${material.id}`}>
          Tahrirlaş
        </Link>
      </div>
      <ShareControl kind="material" id={material.id} token={token} />
    </li>
  );
}
