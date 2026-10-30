"use client";

import { useInView } from "@/shared/lib/useInView";
import type { ResultCriterion } from "../api/types";

function barStyle(value: number | null, visible: boolean) {
  if (value === null) return undefined;
  return { height: visible ? `${Math.max(2, Math.min(100, value))}%` : 0 };
}

/** Tajriba va nazorat guruhlari diagrammasi. Qiymat yo‘q bo‘lsa — shtrixli namuna ustun. */
export function ResultsChart({ criteria }: { criteria: ResultCriterion[] }) {
  const { ref, inView } = useInView<HTMLDivElement>();
  const hasData = criteria.some((c) => c.experimentValue !== null || c.controlValue !== null);

  return (
    <>
      <div
        ref={ref}
        className="tv-chart"
        role="img"
        aria-label={
          hasData
            ? `Tajriba va nazorat guruhlari natijalari: ${criteria
                .map((c) => `${c.label} — tajriba ${c.experimentValue ?? "—"}%, nazorat ${c.controlValue ?? "—"}%`)
                .join("; ")}`
            : `Tajriba va nazorat guruhlari natijalarini ${criteria.length} mezon bo‘yicha taqqoslovchi diagramma — ma’lumotlar hali kiritilmagan`
        }
        style={{ gridTemplateColumns: `repeat(${criteria.length}, 1fr)` }}
      >
        {criteria.map((c, i) => (
          <div key={i} className="tv-bars">
            <span className={`tv-bar exp${c.experimentValue === null ? " empty" : ""}`} style={barStyle(c.experimentValue, inView)} />
            <span className={`tv-bar ctl${c.controlValue === null ? " empty" : ""}`} style={barStyle(c.controlValue, inView)} />
          </div>
        ))}
      </div>
      <div className="chart-x" style={{ gridTemplateColumns: `repeat(${criteria.length}, 1fr)` }}>
        {criteria.map((c, i) => (
          <span key={i}>{c.label}</span>
        ))}
      </div>
    </>
  );
}
