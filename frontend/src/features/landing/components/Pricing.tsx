import { CheckIcon } from "@/shared/ui/icons";
import type { PricingPlan } from "../api/types";
import { SectionHead } from "./SectionHead";

const priceFormat = new Intl.NumberFormat("uz-UZ", { maximumFractionDigits: 0 });

export function Pricing({ plans }: { plans: PricingPlan[] }) {
  return (
    <section className="tv-section tv-alt" id="narxlar" aria-labelledby="narx-t">
      <div className="tv-wrap">
        <SectionHead id="narx-t" label="Narxlar" title="O‘zingizga mos tarifni tanlang" />
        <div className="tv-grid-3 price-grid">
          {plans.map((p) => (
            <article key={p.id} className={`tv-card tv-price tv-reveal${p.isFeatured ? " featured" : ""}`}>
              {p.isFeatured && <span className="tv-sticker tv-sticker-sun">Eng mashhur</span>}
              <h3 className="tv-h3">{p.name}</h3>
              <p className="tv-muted" style={{ margin: 0 }}>
                {p.description}
              </p>
              <div>
                <span className="amt">{p.price === null ? "—" : priceFormat.format(p.price)}</span>{" "}
                <span className="tv-muted">{p.priceUnit}</span>
              </div>
              <ul>
                {p.features.map((f) => (
                  <li key={f}>
                    <CheckIcon />
                    {f}
                  </li>
                ))}
              </ul>
              <a
                className={`tv-btn ${p.isFeatured ? "tv-btn-primary" : "tv-btn-ghost"} tv-btn-block`}
                href={p.ctaHref}
                style={{ marginTop: "auto" }}
              >
                {p.ctaText}
              </a>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
