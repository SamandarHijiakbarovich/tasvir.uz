import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { getCurrentUser } from "@/features/auth/lib/identity-api";
import { getCatalog } from "@/features/payments/api";
import { CheckoutForm } from "@/features/payments/components/CheckoutForm";
import { PaymentsHeader } from "@/features/payments/components/PaymentsHeader";
import { formatMoney, formatMonths } from "@/features/payments/format";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/classroom.css";
import "@/styles/payments.css";

export const metadata: Metadata = { title: "Tölov — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { searchParams: Promise<{ tarif?: string }> };

export default async function CheckoutPage({ searchParams }: Props) {
  const { tarif } = await searchParams;
  const user = await getCurrentUser();
  if (!user) redirect(`/kirish?keyin=${encodeURIComponent(`/tolov?tarif=${tarif ?? ""}`)}`);

  const catalog = await getCatalog();
  const plan = catalog.ok ? catalog.data.plans.find((p) => p.key === tarif && p.isAvailable) : undefined;
  if (catalog.ok && !plan) redirect("/tariflar");

  return (
    <div className="tv-legal">
      <PaymentsHeader signedIn />
      <main className="tv-pay-in tv-pay-narrow">
        <div className="tv-pay-head">
          <p style={{ margin: 0 }}>
            <Link href="/tariflar">← Tariflar</Link>
          </p>
          <h1 className="tv-h2">Tölov</h1>
        </div>

        {!catalog.ok || !plan ? (
          <p className="tv-form-msg err" role="alert">
            {catalog.ok ? "Tarif topilmadi." : catalog.message}
          </p>
        ) : (
          <>
            <section className="tv-card tv-pay-summary" aria-labelledby="buyurtma-t">
              <h2 className="tv-h3" id="buyurtma-t">
                Buyurtma
              </h2>
              <dl>
                <div>
                  <dt>Tarif</dt>
                  <dd>{plan.name}</dd>
                </div>
                <div>
                  <dt>Muddat</dt>
                  <dd>{formatMonths(plan.months)}</dd>
                </div>
                <div>
                  <dt>Hisob</dt>
                  <dd>{user.email}</dd>
                </div>
                <div className="tv-pay-total">
                  <dt>Jami</dt>
                  <dd>{formatMoney(plan.price ?? 0)}</dd>
                </div>
              </dl>
            </section>
            <section className="tv-card">
              <CheckoutForm planKey={plan.key} providers={catalog.data.providers} />
            </section>
          </>
        )}
      </main>
    </div>
  );
}
