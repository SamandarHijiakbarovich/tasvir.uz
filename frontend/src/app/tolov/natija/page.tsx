import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { getMyPayments, getOrder } from "@/features/payments/api";
import { PaymentStatusWatcher } from "@/features/payments/components/PaymentStatusWatcher";
import { PaymentsHeader } from "@/features/payments/components/PaymentsHeader";
import { formatDate, formatMoney, formatMonths, STATUS_LABEL } from "@/features/payments/format";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/classroom.css";
import "@/styles/payments.css";

export const metadata: Metadata = { title: "Tölov natijasi — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { searchParams: Promise<{ buyurtma?: string }> };

const GUID = /^[0-9a-f]{8}-?[0-9a-f]{4}-?[0-9a-f]{4}-?[0-9a-f]{4}-?[0-9a-f]{12}$/i;

export default async function PaymentResultPage({ searchParams }: Props) {
  const { buyurtma } = await searchParams;
  if (!buyurtma || !GUID.test(buyurtma)) redirect("/tariflar");

  const result = await getOrder(buyurtma);
  if (!result.ok && result.status === 401) redirect(`/kirish?keyin=${encodeURIComponent(`/tolov/natija?buyurtma=${buyurtma}`)}`);

  const order = result.ok ? result.data : null;
  const payments = order?.status === "paid" ? await getMyPayments() : null;
  const subscription = payments?.ok ? payments.data.subscription : null;

  return (
    <div className="tv-legal">
      <PaymentsHeader signedIn />
      <main className="tv-pay-in tv-pay-narrow">
        <h1 className="tv-h2">Tölov natijasi</h1>

        {!result.ok ? (
          <p className="tv-form-msg err" role="alert">
            {result.message}
          </p>
        ) : (
          order && (
            <section className="tv-card tv-pay-result" aria-live="polite">
              <span className={`tv-pay-badge is-${order.status}`} aria-hidden="true">
                {order.status === "paid" ? "✓" : order.status === "pending" ? "…" : "×"}
              </span>
              {order.status === "paid" && (
                <>
                  <h2 className="tv-h3">Tölov qabul qilindi</h2>
                  <p>
                    {order.planName} tarifi faollaşdi
                    {subscription?.validUntil ? `, ${formatDate(subscription.validUntil)} gaça amal qiladi` : ""}. Rahmat!
                  </p>
                </>
              )}
              {order.status === "pending" && (
                <>
                  <h2 className="tv-h3">Tölov tekşirilmoqda</h2>
                  <p>Tölov tizimi tasdiq yuborgaç, şu sahifa özi yangilanadi. Tölovni bekor qilgan bölsangiz, qaytadan urinib köring.</p>
                  <PaymentStatusWatcher />
                </>
              )}
              {(order.status === "cancelled" || order.status === "refunded") && (
                <>
                  <h2 className="tv-h3">{order.status === "refunded" ? "Tölov qaytarildi" : "Tölov bekor qilindi"}</h2>
                  <p>Obuna faollaşmadi. Qaytadan urinib köriş mumkin.</p>
                </>
              )}
              <div className="tv-pay-summary" style={{ width: "100%" }}>
                <dl>
                  <div>
                    <dt>Tarif</dt>
                    <dd>
                      {order.planName} · {formatMonths(order.months)}
                    </dd>
                  </div>
                  <div>
                    <dt>Summa</dt>
                    <dd>{formatMoney(order.amount)}</dd>
                  </div>
                  <div>
                    <dt>Tölov tizimi</dt>
                    <dd>{order.provider === "payme" ? "Payme" : "Click"}</dd>
                  </div>
                  <div>
                    <dt>Holat</dt>
                    <dd>
                      <span className={`tv-pay-status is-${order.status}`}>{STATUS_LABEL[order.status]}</span>
                    </dd>
                  </div>
                </dl>
              </div>
              <div className="tv-pay-actions">
                {order.status === "paid" ? (
                  <Link className="tv-btn tv-btn-primary" href="/kabinet">
                    Kabinetga ötiş
                  </Link>
                ) : (
                  <Link className="tv-btn tv-btn-primary" href={`/tolov?tarif=${encodeURIComponent(order.planKey)}`}>
                    Qaytadan tölaş
                  </Link>
                )}
                <Link className="tv-btn tv-btn-ghost" href="/tariflar">
                  Tariflar
                </Link>
              </div>
            </section>
          )
        )}
      </main>
    </div>
  );
}
