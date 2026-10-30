"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { identityRequest, type ApiFailure } from "./lib/identity-api";
import {
  ACCESS_COOKIE,
  REFRESH_COOKIE,
  applySession,
  clearSession,
  safeNextPath,
  type AuthSession,
  type AuthUser,
} from "./lib/session-cookies";

export type FormState = {
  message?: string;
  success?: boolean;
  fields?: Record<string, string>;
  /** Xatodan keyin formada qoladigan qiymatlar. Parollar hech qachon qaytarilmaydi. */
  values?: { fullName?: string; email?: string };
};

const text = (form: FormData, name: string) => {
  const value = form.get(name);
  return typeof value === "string" ? value : "";
};

const failed = (failure: ApiFailure, values?: FormState["values"]): FormState => ({
  // Maydon xatosi bo‘lsa, u maydon yonida ko‘rsatiladi
  message: failure.fields && Object.keys(failure.fields).length > 0 ? undefined : failure.message,
  fields: failure.fields,
  values,
});

export async function registerAction(_: FormState, form: FormData): Promise<FormState> {
  const values = { fullName: text(form, "fullName"), email: text(form, "email") };

  const result = await identityRequest<AuthSession>("register", {
    body: { ...values, password: text(form, "password") },
  });
  if (!result.ok) return failed(result, values);

  applySession(await cookies(), result.data);
  redirect(safeNextPath(text(form, "keyin")));
}

export async function loginAction(_: FormState, form: FormData): Promise<FormState> {
  const values = { email: text(form, "email") };
  const password = text(form, "password");

  const fields: Record<string, string> = {};
  if (!values.email.trim()) fields.email = "Emailingizni kiriting.";
  if (!password) fields.password = "Parolingizni kiriting.";
  if (Object.keys(fields).length > 0) return { fields, values };

  const result = await identityRequest<AuthSession>("login", {
    body: { ...values, password, rememberMe: form.get("rememberMe") === "on" },
  });
  if (!result.ok) return failed(result, values);

  applySession(await cookies(), result.data);
  redirect(safeNextPath(text(form, "keyin")));
}

export async function logoutAction(): Promise<void> {
  const jar = await cookies();
  const refreshToken = jar.get(REFRESH_COOKIE)?.value;
  if (refreshToken) {
    await identityRequest<void>("logout", { body: { refreshToken } });
  }
  clearSession(jar);
  redirect("/");
}

export async function forgotPasswordAction(_: FormState, form: FormData): Promise<FormState> {
  const values = { email: text(form, "email") };
  const result = await identityRequest<void>("password/forgot", { body: values });
  if (!result.ok) return failed(result, values);

  return {
    success: true,
    message: "Agar bu email ro‘yxatdan o‘tgan bo‘lsa, unga parolni tiklash havolasini yubordik. Pochtangizni, jumladan “Spam” bo‘limini tekshiring.",
  };
}

export async function resetPasswordAction(_: FormState, form: FormData): Promise<FormState> {
  const result = await identityRequest<void>("password/reset", {
    body: { token: text(form, "token"), password: text(form, "password") },
  });
  if (!result.ok) return failed(result);

  // Barcha sessiyalar yopildi, shu jumladan bu brauzerdagi ham
  clearSession(await cookies());
  redirect("/kirish?parol=yangilandi");
}

export async function confirmEmailAction(_: FormState, form: FormData): Promise<FormState> {
  const result = await identityRequest<AuthUser>("email/confirm", { body: { token: text(form, "token") } });
  if (!result.ok) return failed(result);
  return { success: true, message: "Emailingiz tasdiqlandi. Rahmat!" };
}

export async function resendConfirmationAction(_: FormState): Promise<FormState> {
  const accessToken = (await cookies()).get(ACCESS_COOKIE)?.value;
  const result = await identityRequest<void>("email/resend", { accessToken });
  if (!result.ok) return failed(result);
  return { success: true, message: "Tasdiqlash xatini qayta yubordik. Pochtangizni tekshiring." };
}

export async function changePasswordAction(_: FormState, form: FormData): Promise<FormState> {
  const jar = await cookies();
  const result = await identityRequest<AuthSession>("password/change", {
    accessToken: jar.get(ACCESS_COOKIE)?.value,
    body: { currentPassword: text(form, "currentPassword"), newPassword: text(form, "newPassword") },
  });
  if (!result.ok) return failed(result);

  applySession(jar, result.data);
  return { success: true, message: "Parol o‘zgartirildi. Boshqa qurilmalardagi sessiyalar yopildi." };
}
