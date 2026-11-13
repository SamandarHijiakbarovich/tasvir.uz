"use client";

import { useActionState } from "react";
import type { FormState } from "@/features/auth/actions";
import { FormMessage, SubmitButton } from "@/features/auth/components/fields";
import { startPaymentAction } from "../actions";
import type { Provider } from "../types";

const initial: FormState = {};

export function CheckoutForm({ planKey, providers }: { planKey: string; providers: Provider[] }) {
  const [state, action] = useActionState(startPaymentAction, initial);
  const available = providers.filter((p) => p.isAvailable);

  return (
    <form action={action} className="tv-form" noValidate>
      <input type="hidden" name="planKey" value={planKey} />
      <FormMessage state={state} />
      <fieldset className="tv-pay-providers" aria-describedby={state.fields?.provider ? "provider-xato" : undefined}>
        <legend className="tv-h3">Tölov tizimi</legend>
        {providers.map((p) => (
          <label key={p.key} className={`tv-pay-provider${p.isAvailable ? "" : " is-off"}`}>
            <input
              type="radio"
              name="provider"
              value={p.key}
              disabled={!p.isAvailable}
              defaultChecked={p.isAvailable && available[0]?.key === p.key}
              aria-invalid={state.fields?.provider ? true : undefined}
            />
            <span className={`tv-pay-logo tv-pay-logo-${p.key}`} aria-hidden="true">
              {p.name}
            </span>
            <span className="tv-pay-provider-text">
              <strong>{p.name}</strong>
              <span className="tv-muted">{p.isAvailable ? `Karta yoki ${p.name} ilovasi orqali` : "Hali ulanmagan"}</span>
            </span>
          </label>
        ))}
        {state.fields?.provider && (
          <p className="tv-field-error" id="provider-xato">
            {state.fields.provider}
          </p>
        )}
      </fieldset>
      {available.length === 0 ? (
        <p className="tv-form-msg err" role="status">
          Onlayn tölov tez orada ulanadi. Hozirça tölov qabul qilinmaydi.
        </p>
      ) : (
        <SubmitButton pendingText="Tölov sahifasi oçilmoqda…">Tölovga ötiş</SubmitButton>
      )}
      <p className="tv-muted tv-pay-note">
        Tölov {available.map((p) => p.name).join(" yoki ") || "Payme yoki Click"} sahifasida amalga oşiriladi. Karta ma’lumotlaringiz Tasvir.uz ga berilmaydi va
        saqlanmaydi.
      </p>
    </form>
  );
}
