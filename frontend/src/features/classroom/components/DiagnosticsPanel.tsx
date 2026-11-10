import Link from "next/link";
import { setDiagnosticStateAction } from "../actions";
import { fixed } from "../lib/format";
import type { Diagnostics, GroupComparison, GroupSummary, PairedTest, TwoSampleTest } from "../types";

const ALPHA = 0.05;

export type CompareState = {
  groups: GroupSummary[];
  selected?: string;
  result?: GroupComparison | null;
  error?: string;
};

const pValue = (p?: number | null) => (p == null ? "—" : p < 0.001 ? "< 0,001" : fixed(p, 3));

/** Koen bo‘yicha odatiy chegaralar: 0,2 — kichik, 0,5 — o‘rta, 0,8 — katta. */
function effectLabel(d?: number | null) {
  if (d == null) return "—";
  const size = Math.abs(d);
  const label = size < 0.2 ? "juda kiçik" : size < 0.5 ? "kichik" : size < 0.8 ? "örta" : "katta";
  return `${fixed(d)} (${label})`;
}

export function DiagnosticsPanel({ groupId, data, compare }: { groupId: string; data: Diagnostics; compare: CompareState }) {
  const exportHref = `/guruhlar/${groupId}/eksport/diagnostika`;

  return (
    <>
      <section className="tv-card tv-panel" aria-labelledby="diag-t">
        <div className="tv-panel-head">
          <h2 className="tv-h3" id="diag-t">
            Diagnostika testi
          </h2>
          <a className="tv-btn tv-btn-ghost tv-btn-sm" href={exportHref} download>
            Excel uçun yuklab oliş
          </a>
        </div>
        <p className="tv-field-hint" style={{ margin: 0 }}>
          {data.maxScore} ta savoldan iborat test bilimlilik mezonini ölçaydi. Boşlanğiç testni kurs boşida, yakuniyni oxirida oçing: talabalar ikkalasida bir xil
          savollarga javob beradi, har birini bir marta topşiradi.
        </p>
        <ul className="tv-diag-stages">
          {data.stages.map((stage) => (
            <li key={stage.stage}>
              <div>
                <strong>{stage.title}</strong>
                <p className="tv-material-meta">
                  {stage.isOpen ? "Oçiq" : "Yopiq"} · topşirganlar: {stage.respondents}
                </p>
              </div>
              <form action={setDiagnosticStateAction.bind(null, groupId, stage.stage, !stage.isOpen)}>
                <button className={`tv-btn tv-btn-sm ${stage.isOpen ? "tv-btn-ghost" : "tv-btn-primary"}`} type="submit">
                  {stage.isOpen ? "Yopiş" : "Oçiş"}
                </button>
              </form>
            </li>
          ))}
        </ul>
      </section>

      <PairedSummary paired={data.paired} maxScore={data.maxScore} />

      {data.students.length > 0 && (
        <section className="tv-card tv-panel" aria-labelledby="diag-students-t">
          <h2 className="tv-h3" id="diag-students-t">
            Talabalar natijasi
          </h2>
          <div className="tv-table-wrap" tabIndex={0} role="region" aria-labelledby="diag-students-t">
            <table className="tv-table">
              <thead>
                <tr>
                  <th scope="col">F.I.Ş.</th>
                  <th scope="col">Boşlanğiç</th>
                  <th scope="col">Yakuniy</th>
                  <th scope="col">Ösiş</th>
                </tr>
              </thead>
              <tbody>
                {data.students.map((s) => (
                  <tr key={s.userId}>
                    <th scope="row">{s.name}</th>
                    <td>{s.pre ?? "—"}</td>
                    <td>{s.post ?? "—"}</td>
                    <td>{s.pre != null && s.post != null ? (s.post - s.pre > 0 ? `+${s.post - s.pre}` : s.post - s.pre) : "—"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      <section className="tv-card tv-panel" aria-labelledby="diag-items-t">
        <h2 className="tv-h3" id="diag-items-t">
          Savollar böyiça
        </h2>
        <p className="tv-field-hint" style={{ margin: 0 }}>
          Töğri javob berganlar uluşi. Yakuniy testda ösmagan savollar mavzusini darslarda qayta körib çiqing.
        </p>
        <div className="tv-table-wrap" tabIndex={0} role="region" aria-labelledby="diag-items-t">
          <table className="tv-table">
            <thead>
              <tr>
                <th scope="col">№</th>
                <th scope="col">Savol</th>
                <th scope="col">Mavzu</th>
                <th scope="col">Boşlanğiç</th>
                <th scope="col">Yakuniy</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((item, index) => (
                <tr key={item.key}>
                  <td>{index + 1}</td>
                  <td>{item.text}</td>
                  <td>{item.topic}</td>
                  <td>{item.preCorrectShare == null ? "—" : `${Math.round(item.preCorrectShare * 100)}%`}</td>
                  <td>{item.postCorrectShare == null ? "—" : `${Math.round(item.postCorrectShare * 100)}%`}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <Comparison groupId={groupId} compare={compare} />
    </>
  );
}

function PairedSummary({ paired, maxScore }: { paired?: PairedTest | null; maxScore: number }) {
  if (!paired) {
    return (
      <section className="tv-card tv-empty">
        <h2 className="tv-h3">Statistik tahlil hali yöq</h2>
        <p className="tv-muted">Ikkala testni kamida ikki talaba topşirgaç, boşlanğiç va yakuniy natija farqi Styudent mezoni bilan hisoblanadi.</p>
      </section>
    );
  }

  const significant = paired.p != null && paired.p < ALPHA;
  const conclusion =
    paired.t == null
      ? "Hamma talabaning ösişi bir xil: t-mezonni hisoblab bölmaydi."
      : significant
        ? paired.meanGain > 0
          ? "Yakuniy natija boşlanğiçdan statistik ahamiyatli yuqori (p < 0,05)."
          : "Yakuniy natija boşlanğiçdan statistik ahamiyatli past (p < 0,05)."
        : "Boşlanğiç va yakuniy natija farqi statistik ahamiyatli emas (p ≥ 0,05).";

  return (
    <section className="tv-card tv-panel" aria-labelledby="diag-stat-t">
      <h2 className="tv-h3" id="diag-stat-t">
        Boşlanğiç va yakuniy natija (juftlangan Styudent mezoni)
      </h2>
      <dl className="tv-diag-stats">
        <div>
          <dt>Talabalar (ikkala test)</dt>
          <dd>{paired.n}</dd>
        </div>
        <div>
          <dt>Boşlanğiç örtaça</dt>
          <dd>
            {fixed(paired.meanPre)} / {maxScore}
          </dd>
        </div>
        <div>
          <dt>Yakuniy örtaça</dt>
          <dd>
            {fixed(paired.meanPost)} / {maxScore}
          </dd>
        </div>
        <div>
          <dt>Örtaça ösiş</dt>
          <dd>{fixed(paired.meanGain)}</dd>
        </div>
        <div>
          <dt>t (erkinlik darajasi)</dt>
          <dd>{paired.t == null ? "—" : `${fixed(paired.t)} (${paired.df})`}</dd>
        </div>
        <div>
          <dt>p (ikki tomonlama)</dt>
          <dd>{pValue(paired.p)}</dd>
        </div>
        <div>
          <dt>Effekt kattaligi (Koen dz)</dt>
          <dd>{effectLabel(paired.effectSize)}</dd>
        </div>
      </dl>
      <p className={`tv-form-msg ${significant && paired.meanGain > 0 ? "ok" : "err"}`} style={{ margin: 0 }}>
        {conclusion}
      </p>
      {paired.n < 30 && (
        <p className="tv-field-hint" style={{ margin: 0 }}>
          Talabalar soni kam ({paired.n} ta): natijani ehtiyotkorlik bilan talqin qiling.
        </p>
      )}
    </section>
  );
}

function Comparison({ groupId, compare }: { groupId: string; compare: CompareState }) {
  const others = compare.groups.filter((g) => g.id !== groupId);
  const result = compare.result;

  const rows: { label: string; test?: TwoSampleTest | null }[] = result
    ? [
        { label: "Boşlanğiç test", test: result.pre },
        { label: "Yakuniy test", test: result.post },
        { label: "Ösiş", test: result.gain },
      ]
    : [];

  return (
    <section className="tv-card tv-panel" aria-labelledby="diag-compare-t">
      <h2 className="tv-h3" id="diag-compare-t">
        Nazorat guruhi bilan soliştiriş
      </h2>
      {others.length === 0 ? (
        <p className="tv-muted" style={{ margin: 0 }}>
          Soliştiriş uçun ikkinçi guruh yarating (masalan, nazorat guruhi) va unda ham diagnostika testini ötkazing.
        </p>
      ) : (
        <form className="tv-diag-compare" method="get">
          <input type="hidden" name="b" value="diagnostika" />
          <label htmlFor="qiyos-select">Ikkinçi guruh</label>
          <select id="qiyos-select" name="qiyos" defaultValue={compare.selected ?? ""} className="tv-select">
            <option value="" disabled>
              Tanlang…
            </option>
            {others.map((g) => (
              <option key={g.id} value={g.id}>
                {g.name}
              </option>
            ))}
          </select>
          <button className="tv-btn tv-btn-ghost tv-btn-sm" type="submit">
            Soliştiriş
          </button>
        </form>
      )}
      {compare.error && (
        <p className="tv-form-msg err" role="alert" style={{ margin: 0 }}>
          {compare.error}
        </p>
      )}
      {result && (
        <>
          <p className="tv-field-hint" style={{ margin: 0 }}>
            A — “{result.groupA.name}”, B — “{result.groupB.name}”. Uelç t-mezoni (guruhlar dispersiyasi teng bölişi şart emas). Tajriba boşida guruhlar teng bölişi
            uçun boşlanğiç test farqi ahamiyatsiz (p ≥ 0,05) bölişi kerak.
          </p>
          <div className="tv-table-wrap" tabIndex={0} role="region" aria-labelledby="diag-compare-t">
            <table className="tv-table">
              <thead>
                <tr>
                  <th scope="col">Körsatkiç</th>
                  <th scope="col">A: n, örtaça</th>
                  <th scope="col">B: n, örtaça</th>
                  <th scope="col">t (df)</th>
                  <th scope="col">p</th>
                  <th scope="col">Koen d</th>
                </tr>
              </thead>
              <tbody>
                {rows.map(({ label, test }) => (
                  <tr key={label}>
                    <th scope="row">{label}</th>
                    {test ? (
                      <>
                        <td>
                          {test.countA}, {fixed(test.meanA)}
                        </td>
                        <td>
                          {test.countB}, {fixed(test.meanB)}
                        </td>
                        <td>{test.t == null || test.df == null ? "—" : `${fixed(test.t)} (${fixed(test.df, 1)})`}</td>
                        <td>{pValue(test.p)}</td>
                        <td>{effectLabel(test.effectSize)}</td>
                      </>
                    ) : (
                      <td colSpan={5} className="tv-muted">
                        Har bir guruhda kamida ikki natija kerak
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Link href={`/guruhlar/${result.groupB.id}?b=diagnostika`}>“{result.groupB.name}” guruhi natijalari</Link>
        </>
      )}
    </section>
  );
}
