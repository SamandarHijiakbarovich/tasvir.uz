import Link from "next/link";
import { formatDate, formatMoney, STATUS_LABEL } from "../format";
import type { MyPayments } from "../types";

/** Kabinetdagi obuna kartasi: holat va oxirgi tölovlar. */
export function SubscriptionCard({ payments }: { payments: MyPayments | null }) {
  const subscription = payments?.subscription;
  const orders = payments?.orders.filter((o) => o.status !== "pending").slice(0, 5) ?? [];

  return (
    <section className="tv-card tv-card-outline tv-course-card" aria-labelledby="obuna-t">
      <div>
        <h2 className="tv-h3" id="obuna-t">
          Obuna
        </h2>
        {subscription?.isActive && subscription.validUntil ? (
          <p className="tv-muted">
            <strong>{subscription.planName}</strong> tarifi {formatDate(subscription.validUntil)} gaça amal qiladi.
          </p>
        ) : (
          <p className="tv-muted">Hozir bepul tarifdasiz. Pro tarifi barça kurslar va şablonlarni oçadi.</p>
        )}
        {orders.length > 0 && (
          <ul className="tv-pay-history">
            {orders.map((o) => (
              <li key={o.id}>
                <span>
                  {formatDate(o.paidAt ?? o.createdAt)} · {o.planName}
                </span>
                <span>{formatMoney(o.amount)}</span>
                <span className={`tv-pay-status is-${o.status}`}>{STATUS_LABEL[o.status]}</span>
              </li>
            ))}
          </ul>
        )}
      </div>
      <div className="tv-lesson-actions">
        <Link className="tv-btn tv-btn-primary" href="/tariflar">
          {subscription?.isActive ? "Obunani uzaytiriş" : "Tariflar"}
        </Link>
      </div>
    </section>
  );
}
