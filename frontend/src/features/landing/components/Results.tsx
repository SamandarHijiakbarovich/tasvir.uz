import { InfoIcon } from "@/shared/ui/icons";
import type { Results as ResultsData } from "../api/types";
import { CountUp } from "./CountUp";
import { ResultsChart } from "./ResultsChart";
import { SectionHead } from "./SectionHead";

export function Results({ results }: { results: ResultsData }) {
  const hasEmpty = results.criteria.some((c) => c.experimentValue === null || c.controlValue === null);

  return (
    <section className="tv-section tv-alt" aria-labelledby="res-t">
      <div className="tv-wrap">
        <SectionHead
          id="res-t"
          label="Tadqiqot"
          title="Tajriba-sinov natijalari"
          lead="Metodika tajriba va nazorat guruhlarida sinovdan o‘tkaziladi. Natijalar tajriba yakunlangach shu yerda e’lon qilinadi."
        />
        <div className="res">
          <div className="tv-card tv-reveal">
            <div className="tv-legend">
              <span>
                <i style={{ background: "var(--mint)" }} />
                Tajriba guruhi
              </span>
              <span>
                <i style={{ background: "var(--line)" }} />
                Nazorat guruhi
              </span>
            </div>
            <ResultsChart criteria={results.criteria} />
            {hasEmpty && (
              <div className="res-note">
                <InfoIcon />
                Shtrixli ustunlar — ma’lumot uchun joy. Real qiymatlar kiritilganda ustunlar animatsiya bilan o‘sadi.
              </div>
            )}
          </div>
          <div className="stats">
            {results.stats.map((s, i) => (
              <div key={i} className="tv-stat tv-reveal">
                <b>
                  <CountUp value={s.value} />
                  {s.suffix && <span aria-hidden="true">{s.suffix}</span>}
                </b>
                <span className="tv-muted">{s.label}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
