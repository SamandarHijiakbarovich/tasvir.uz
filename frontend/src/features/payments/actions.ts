"use server";

import { redirect } from "next/navigation";
import type { FormState } from "@/features/auth/actions";
import { authedRequest } from "@/shared/lib/backend";
import type { CreatedOrder } from "./types";

const text = (form: FormData, name: string) => {
  const value = form.get(name);
  return typeof value === "string" ? value : "";
};

/** Buyurtma yaratadi va foydalanuvchini Payme yoki Click tölov sahifasiga yuboradi. */
export async function startPaymentAction(_: FormState, form: FormData): Promise<FormState> {
  const planKey = text(form, "planKey");
  const result = await authedRequest<CreatedOrder>("/api/payments/orders", {
    body: { planKey, provider: text(form, "provider") },
  });

  if (!result.ok) {
    if (result.status === 401) redirect(`/kirish?keyin=${encodeURIComponent(`/tolov?tarif=${planKey}`)}`);
    return {
      message: result.fields && Object.keys(result.fields).length > 0 ? undefined : result.message,
      fields: result.fields,
    };
  }

  redirect(result.data.paymentUrl);
}
