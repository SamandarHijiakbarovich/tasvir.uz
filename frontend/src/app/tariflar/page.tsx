import type { Metadata } from "next";
import Link from "next/link";
import { getCurrentUser } from "@/features/auth/lib/identity-api";
import { getCatalog, getMyPayments } from "@/features/payments/api";
import { PaymentsHeader } from "@/features/payments/components/PaymentsHeader";
import { formatDate, formatMoney, formatMonths } from "@/features/payments/format";
import { CheckIcon } from "@/shared/ui/icons";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/classroom.css";
import "@/styles/payments.css";

export const metadata: Metadata = {
  title: "Tariflar — Tasvir.uz",
  description: "Tasvir.uz Pro tarifi: barça kurslar, şablonlar va öqituvçi vositalari. Payme yoki Click orqali tölov.",
};

export default async function PlansPage() {
  const [catalog, user] = await Promise.all([getCatalog(), getCurrentUser()]);
  const payments = user ? await getMyPayments() : null;
  const subscription = payments?.ok ? payments.data.subscription : null;
  const plans = catalog.ok ? catalog.data.plans : [];
  const anyProvider = catalog.ok && catalog.data.providers.some((p) => p.isAvailable);

  return (
    <div className="tv-legal">
      <PaymentsHeader signedIn={!!user} />
      <main className="tv-pay-in">
        <div className="tv-pay-head">
          <h1 className="tv-h2">Tariflar</h1>
          <p className="tv-lead">Asosiy imkoniyatlar bepul. Pro tarifi barça kurslar, şablonlar va öqituvçi vositalarini oçadi.</p>
        </div>

        {subscription?.isActive && subscription.validUntil && (
          <div className="tv-banner" role="status">
            <p>
              <strong>{subscription.planName}</strong> obunangiz {formatDate(subscription.validUntil)} gaça amal qiladi. Yangi tölov muddatni şu sanadan boşlab
              uzaytiradi.
            </p>
          </div>
        )}

        {!catalog.ok && (
          <p className="tv-form-msg err" role="alert">
            {catalog.message}
          </p>
        )}

        {catalog.ok && !anyProvider && (
          <p className="tv-form-msg err" role="status">
            Onlayn tölov tez orada ulanadi. Hozirça tariflar bilan tanişib çiqişingiz mumkin.
          </p>
        )}

        {catalog.ok && plans.length === 0 && <p className="tv-muted">Pullik tariflar hali e’lon qilinmagan.</p>}

        <div className="tv-pay-plans">
          {plans.map((plan, index) => (
            <article key={plan.key} className={`tv-card tv-price${index === 0 ? " featured" : ""}`}>
              <h2 className="tv-h3">{plan.name}</h2>
              {plan.description && (
                <p className="tv-muted" style={{ margin: 0 }}>
                  {plan.description}
                </p>
              )}
              <div>
                {plan.price === null ? (
                  <span className="tv-pay-period">Narxi tez orada e’lon qilinadi · {formatMonths(plan.months)}</span>
                ) : (
                  <>
                    <span className="amt">{formatMoney(plan.price)}</span> <span className="tv-pay-period">/ {formatMonths(plan.months)}</span>
                  </>
                )}
              </div>
              {plan.features.length > 0 && (
                <ul>
                  {plan.features.map((f) => (
                    <li key={f}>
                      <CheckIcon />
                      {f}
                    </li>
                  ))}
                </ul>
              )}
              {plan.isAvailable && anyProvider ? (
                <Link
                  className={`tv-btn ${index === 0 ? "tv-btn-primary" : "tv-btn-ghost"} tv-btn-block`}
                  href={`/tolov?tarif=${encodeURIComponent(plan.key)}`}
                  style={{ marginTop: "auto" }}
                >
                  Tanlaş
                </Link>
              ) : (
                <span className="tv-btn tv-btn-ghost tv-btn-block" aria-disabled="true" style={{ marginTop: "auto", opacity: 0.6, cursor: "not-allowed" }}>
                  Tez orada
                </span>
              )}
            </article>
          ))}
        </div>

        <p className="tv-muted" style={{ margin: 0 }}>
          Tölov Payme yoki Click orqali, Özbekiston bank kartalari (Uzcard, Humo) bilan qabul qilinadi. Savollar böyiça{" "}
          <Link href="/#boglanish">biz bilan boğlaning</Link>.
        </p>
      </main>
    </div>
  );
}
