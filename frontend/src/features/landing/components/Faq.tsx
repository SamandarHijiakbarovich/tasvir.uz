import type { FaqItem } from "../api/types";
import { SectionHead } from "./SectionHead";

export function Faq({ items }: { items: FaqItem[] }) {
  return (
    <section className="tv-section" aria-labelledby="faq-t">
      <div className="tv-wrap">
        <SectionHead id="faq-t" label="Savol-javob" title="Ko‘p beriladigan savollar" />
        <div className="tv-faq">
          {items.map((f, i) => (
            <details key={f.id} open={i === 0}>
              <summary>{f.question}</summary>
              <p>{f.answer}</p>
            </details>
          ))}
        </div>
      </div>
    </section>
  );
}
