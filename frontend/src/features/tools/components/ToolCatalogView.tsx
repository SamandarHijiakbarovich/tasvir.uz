import Link from "next/link";
import type { Subject, ToolCatalog, VisualTool } from "../types";

const ACCESS: Record<VisualTool["access"], string> = { free: "Bepul", freemium: "Qisman bepul", paid: "Pullik" };
const LEVEL: Record<VisualTool["level"], string> = { easy: "Oson", medium: "O‘rta", coding: "Dasturlash kerak" };

export function ToolCatalogView({ catalog, subject }: { catalog: ToolCatalog; subject?: Subject }) {
  return (
    <>
      <WhatIsVisualization />
      <ShapeGuide catalog={catalog} />
      <SubjectTable catalog={catalog} />
      <Catalog catalog={catalog} subject={subject} />
      <p className="tv-field-hint tv-tools-note">
        Ma’lumotlar {catalog.checkedAt} holatiga ko‘ra yozilgan. Narx va shartlar o‘zgarishi mumkin, ishlatishdan oldin rasmiy saytdan aniqlang. Tasvir.uz bu
        xizmatlar bilan hamkor emas, ularning nomlari egalariga tegishli.
      </p>
    </>
  );
}

function WhatIsVisualization() {
  return (
    <section className="tv-tools-section" aria-labelledby="nima-t">
      <h2 className="tv-h3" id="nima-t">
        Vizualizatsiya nima?
      </h2>
      <div className="tv-tools-what">
        <div className="tv-tools-prose">
          <p>
            <strong>Vizualizatsiya</strong> axborotni ko‘z bilan idrok etiladigan shaklga keltirish: matn, son va tushunchalar orasidagi bog‘liqlik sxema, diagramma,
            jadval yoki xaritada ko‘rsatiladi. O‘quvchi tuzilmani bir qarashda ko‘radi va uni matndan ko‘ra osonroq eslab qoladi.
          </p>
          <p>
            <strong>Dasturda vizualizatsiya</strong> deganda shu ishni dasturiy vosita yordamida bajarish tushuniladi. Dastur:
          </p>
          <ul>
            <li>tayyor shablon va elementlar beradi, chizishni tezlashtiradi;</li>
            <li>jadvaldagi sonlardan diagrammani o‘zi chizadi;</li>
            <li>model va simulyatsiyada o‘quvchiga hodisani o‘zi boshqarish imkonini beradi;</li>
            <li>materialni tahrirlash, saqlash, ulashish va darsda ko‘rsatishni osonlashtiradi.</li>
          </ul>
          <p>
            Qaysi shakl kerakligini esa dastur emas, o‘qituvchi hal qiladi. Buni <Link href="/akademiya">Akademiyadagi darslar</Link> o‘rgatadi.
          </p>
        </div>
        <figure className="tv-card tv-tools-demo" aria-label="Bir mazmun matn va sxema ko‘rinishida">
          <figcaption className="tv-material-meta">Matn</figcaption>
          <p>Quyosh issig‘ida suv bug‘lanadi. Bug‘ yuqorida soviydi va bulut hosil qiladi. Bulutdan yomg‘ir yoki qor yog‘adi. Suv daryolar orqali yana dengizga qaytadi.</p>
          <figcaption className="tv-material-meta">Sxema</figcaption>
          <ol className="tv-tools-cycle">
            <li>Bug‘lanish</li>
            <li>Bulut</li>
            <li>Yog‘in</li>
            <li>Dengizga qaytish</li>
          </ol>
        </figure>
      </div>
    </section>
  );
}

function names(tools: VisualTool[]) {
  return tools.map((t) => t.name).join(", ");
}

function ShapeGuide({ catalog }: { catalog: ToolCatalog }) {
  return (
    <section className="tv-tools-section" aria-labelledby="shakl-t">
      <h2 className="tv-h3" id="shakl-t">
        Mazmunga qarab shakl va dastur
      </h2>
      <div className="tv-table-wrap" tabIndex={0} role="region" aria-labelledby="shakl-t">
        <table className="tv-table">
          <thead>
            <tr>
              <th scope="col">Nimani ko‘rsatmoqchisiz</th>
              <th scope="col">Shakl</th>
              <th scope="col">Dasturlar</th>
              <th scope="col">Tasvir.uz Studiyasida</th>
            </tr>
          </thead>
          <tbody>
            {catalog.shapes.map((shape) => (
              <tr key={shape.key}>
                <td>{shape.useFor}</td>
                <th scope="row">{shape.name}</th>
                <td>{names(catalog.tools.filter((t) => t.shapes.includes(shape.key)))}</td>
                <td>{shape.inStudio ? <Link href="/studiya/yangi">Shablon bor</Link> : "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

function SubjectTable({ catalog }: { catalog: ToolCatalog }) {
  return (
    <section className="tv-tools-section" aria-labelledby="fan-t">
      <h2 className="tv-h3" id="fan-t">
        Fanlar kesimida
      </h2>
      <div className="tv-table-wrap" tabIndex={0} role="region" aria-labelledby="fan-t">
        <table className="tv-table">
          <thead>
            <tr>
              <th scope="col">Fan</th>
              <th scope="col">Ko‘p qo‘llaniladigan dasturlar</th>
              <th scope="col">Maqsad</th>
            </tr>
          </thead>
          <tbody>
            {catalog.subjects.map((subject) => (
              <tr key={subject.key}>
                <th scope="row">
                  <Link href={`/vositalar?fan=${subject.key}#katalog`}>{subject.name}</Link>
                </th>
                <td>{names(catalog.tools.filter((t) => t.subjects.includes(subject.key)))}</td>
                <td>{subject.purpose}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="tv-field-hint" style={{ margin: 0 }}>
        Canva, Miro, PowerPoint kabi umumiy dasturlar har qanday fanda ishlatiladi, ular katalogda alohida ko‘rsatilgan.
      </p>
    </section>
  );
}

function Catalog({ catalog, subject }: { catalog: ToolCatalog; subject?: Subject }) {
  const subjectName = (key: string) => catalog.subjects.find((s) => s.key === key)?.name ?? key;
  const shapeName = (key: string) => catalog.shapes.find((s) => s.key === key)?.name ?? key;
  const special = subject ? catalog.tools.filter((t) => t.subjects.includes(subject.key)) : catalog.tools;
  const universal = subject ? catalog.tools.filter((t) => t.subjects.length === 0) : [];

  const card = (tool: VisualTool) => (
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
          <dt>Shakllar</dt>
          <dd>{tool.shapes.map(shapeName).join(", ")}</dd>
        </div>
        <div>
          <dt>Fanlar</dt>
          <dd>{tool.subjects.length === 0 ? "Har qanday fan" : tool.subjects.map(subjectName).join(", ")}</dd>
        </div>
        <div>
          <dt>O‘rganish</dt>
          <dd>{LEVEL[tool.level]}</dd>
        </div>
        <div>
          <dt>Narxi</dt>
          <dd>{tool.accessNote}</dd>
        </div>
      </dl>
      <a className="tv-tool-link" href={tool.url} target="_blank" rel="noopener noreferrer">
        Rasmiy sayt<span className="tv-sr"> (yangi oynada ochiladi)</span> ↗
      </a>
    </li>
  );

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
      <nav className="tv-filter" aria-label="Fan bo‘yicha saralash">
        <Link href="/vositalar#katalog" aria-current={!subject}>
          Barchasi
        </Link>
        {catalog.subjects.map((s) => (
          <Link key={s.key} href={`/vositalar?fan=${s.key}#katalog`} aria-current={subject?.key === s.key}>
            {s.name}
          </Link>
        ))}
      </nav>
      {subject && <h3 className="tv-h3">{subject.name} uchun</h3>}
      {grouped(special)}
      {universal.length > 0 && (
        <>
          <h3 className="tv-h3">Har qanday fanda ishlatiladigan dasturlar</h3>
          {grouped(universal)}
        </>
      )}
    </section>
  );
}
