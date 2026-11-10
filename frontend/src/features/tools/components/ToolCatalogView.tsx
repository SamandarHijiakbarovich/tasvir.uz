import Link from "next/link";
import type { Subject, ToolCatalog, VisualTool } from "../types";

const ACCESS: Record<VisualTool["access"], string> = { free: "Bepul", freemium: "Qisman bepul", paid: "Pullik" };
const LEVEL: Record<VisualTool["level"], string> = { easy: "Oson", medium: "Örta", coding: "Dasturlaş kerak" };

export function ToolCatalogView({ catalog, subject }: { catalog: ToolCatalog; subject?: Subject }) {
  return (
    <>
      <WhatIsVisualization />
      <ShapeGuide catalog={catalog} />
      <SubjectTable catalog={catalog} />
      <Catalog catalog={catalog} subject={subject} />
      <p className="tv-field-hint tv-tools-note">
        Ma’lumotlar {catalog.checkedAt} holatiga köra yozilgan. Narx va şartlar özgarişi mumkin, işlatişdan oldin rasmiy saytdan aniqlang. Tasvir.uz bu
        xizmatlar bilan hamkor emas, ularning nomlari egalariga tegişli.
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
            <strong>Vizualizatsiya</strong> axborotni köz bilan idrok etiladigan şaklga keltiriş: matn, son va tuşunçalar orasidagi boğliqlik sxema, diagramma,
            jadval yoki xaritada körsatiladi. Öquvçi tuzilmani bir qaraşda köradi va uni matndan köra osonroq eslab qoladi.
          </p>
          <p>
            <strong>Dasturda vizualizatsiya</strong> deganda şu işni dasturiy vosita yordamida bajariş tuşuniladi. Dastur:
          </p>
          <ul>
            <li>tayyor şablon va elementlar beradi, çizişni tezlaştiradi;</li>
            <li>jadvaldagi sonlardan diagrammani özi çizadi;</li>
            <li>model va simulyatsiyada öquvçiga hodisani özi boşqariş imkonini beradi;</li>
            <li>materialni tahrirlaş, saqlaş, ulaşiş va darsda körsatişni osonlaştiradi.</li>
          </ul>
          <p>
            Qaysi şakl kerakligini esa dastur emas, öqituvçi hal qiladi. Buni <Link href="/akademiya">Akademiyadagi darslar</Link> örgatadi.
          </p>
        </div>
        <figure className="tv-card tv-tools-demo" aria-label="Bir mazmun matn va sxema körinişida">
          <figcaption className="tv-material-meta">Matn</figcaption>
          <p>Quyoş issiğida suv buğlanadi. Buğ yuqorida soviydi va bulut hosil qiladi. Bulutdan yomğir yoki qor yoğadi. Suv daryolar orqali yana dengizga qaytadi.</p>
          <figcaption className="tv-material-meta">Sxema</figcaption>
          <ol className="tv-tools-cycle">
            <li>Buğlaniş</li>
            <li>Bulut</li>
            <li>Yoğin</li>
            <li>Dengizga qaytiş</li>
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
        Mazmunga qarab şakl va dastur
      </h2>
      <div className="tv-table-wrap" tabIndex={0} role="region" aria-labelledby="shakl-t">
        <table className="tv-table">
          <thead>
            <tr>
              <th scope="col">Nimani körsatmoqçisiz</th>
              <th scope="col">Şakl</th>
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
                <td>{shape.inStudio ? <Link href="/studiya/yangi">Şablon bor</Link> : "—"}</td>
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
              <th scope="col">Köp qöllaniladigan dasturlar</th>
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
        Canva, Miro, PowerPoint kabi umumiy dasturlar har qanday fanda işlatiladi, ular katalogda alohida körsatilgan.
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
      <a className="tv-tool-link" href={tool.url} target="_blank" rel="noopener noreferrer">
        Rasmiy sayt<span className="tv-sr"> (yangi oynada oçiladi)</span> ↗
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
      <nav className="tv-filter" aria-label="Fan böyiça saralaş">
        <Link href="/vositalar#katalog" aria-current={!subject}>
          Barçasi
        </Link>
        {catalog.subjects.map((s) => (
          <Link key={s.key} href={`/vositalar?fan=${s.key}#katalog`} aria-current={subject?.key === s.key}>
            {s.name}
          </Link>
        ))}
      </nav>
      {subject && <h3 className="tv-h3">{subject.name} uçun</h3>}
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
