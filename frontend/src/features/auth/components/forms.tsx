"use client";

import Link from "next/link";
import { useActionState } from "react";
import {
  changePasswordAction,
  confirmEmailAction,
  forgotPasswordAction,
  loginAction,
  registerAction,
  resendConfirmationAction,
  resetPasswordAction,
  type FormState,
} from "../actions";
import { FormMessage, PasswordField, SubmitButton, TextField, useFocusFirstError } from "./fields";

const initial: FormState = {};
const PASSWORD_HINT = "Kamida 8 ta belgi. Uzunroq parol xavfsizroq.";

export function RegisterForm({ next }: { next?: string }) {
  const [state, action] = useActionState(registerAction, initial);
  const ref = useFocusFirstError(state);

  return (
    <form ref={ref} action={action} className="tv-form" noValidate>
      <FormMessage state={state} />
      {next && <input type="hidden" name="keyin" value={next} />}
      <TextField
        label="Ism va familiya"
        name="fullName"
        autoComplete="name"
        placeholder="Masalan: Dilnoza Karimova"
        defaultValue={state.values?.fullName}
        error={state.fields?.fullName}
        required
      />
      <TextField
        label="Email"
        name="email"
        type="email"
        inputMode="email"
        autoComplete="email"
        autoCapitalize="none"
        placeholder="sizning@email.uz"
        defaultValue={state.values?.email}
        error={state.fields?.email}
        required
      />
      <PasswordField label="Parol" name="password" autoComplete="new-password" hint={PASSWORD_HINT} error={state.fields?.password} showStrength required />
      <div className="tv-field">
        <label className="tv-check tv-check-top">
          <input type="checkbox" name="acceptPrivacy" defaultChecked={state.values?.acceptPrivacy} aria-invalid={state.fields?.acceptPrivacy ? true : undefined} />
          <span>
            <Link href="/maxfiylik" target="_blank">
              Maxfiylik siyosati
            </Link>{" "}
            bilan tanishdim va shaxsiy ma’lumotlarim shu asosda qayta ishlanishiga roziman.
          </span>
        </label>
        {state.fields?.acceptPrivacy && <p className="tv-field-error">{state.fields.acceptPrivacy}</p>}
      </div>
      <SubmitButton pendingText="Hisob yaratilmoqda…">Hisob yaratish</SubmitButton>
    </form>
  );
}

export function LoginForm({ next }: { next?: string }) {
  const [state, action] = useActionState(loginAction, initial);
  const ref = useFocusFirstError(state);

  return (
    <form ref={ref} action={action} className="tv-form" noValidate>
      <FormMessage state={state} />
      {next && <input type="hidden" name="keyin" value={next} />}
      <TextField
        label="Email"
        name="email"
        type="email"
        inputMode="email"
        autoComplete="username"
        autoCapitalize="none"
        placeholder="sizning@email.uz"
        defaultValue={state.values?.email}
        error={state.fields?.email}
        required
      />
      <PasswordField label="Parol" name="password" autoComplete="current-password" error={state.fields?.password} required />
      <div className="tv-form-row">
        <label className="tv-check">
          <input type="checkbox" name="rememberMe" defaultChecked />
          Meni eslab qol
        </label>
        <Link href="/parolni-tiklash">Parolni unutdingizmi?</Link>
      </div>
      <SubmitButton pendingText="Kirilmoqda…">Kirish</SubmitButton>
    </form>
  );
}

export function ForgotPasswordForm() {
  const [state, action] = useActionState(forgotPasswordAction, initial);
  const ref = useFocusFirstError(state);

  if (state.success) return <FormMessage state={state} />;

  return (
    <form ref={ref} action={action} className="tv-form" noValidate>
      <FormMessage state={state} />
      <TextField
        label="Email"
        name="email"
        type="email"
        inputMode="email"
        autoComplete="email"
        autoCapitalize="none"
        placeholder="sizning@email.uz"
        defaultValue={state.values?.email}
        error={state.fields?.email}
        required
      />
      <SubmitButton pendingText="Yuborilmoqda…">Havola yuborish</SubmitButton>
    </form>
  );
}

export function ResetPasswordForm({ token }: { token: string }) {
  const [state, action] = useActionState(resetPasswordAction, initial);
  const ref = useFocusFirstError(state);

  return (
    <form ref={ref} action={action} className="tv-form" noValidate>
      <FormMessage state={state} />
      <input type="hidden" name="token" value={token} />
      <PasswordField label="Yangi parol" name="password" autoComplete="new-password" hint={PASSWORD_HINT} error={state.fields?.password} showStrength required />
      <SubmitButton pendingText="Saqlanmoqda…">Parolni saqlash</SubmitButton>
    </form>
  );
}

/** Tasdiqlash tugma bilan bajariladi: pochta skanerlari havolani ochganda email o‘z-o‘zidan tasdiqlanib qolmasin. */
export function ConfirmEmailForm({ token }: { token: string }) {
  const [state, action] = useActionState(confirmEmailAction, initial);

  if (state.success) {
    return (
      <div className="tv-form">
        <FormMessage state={state} />
        <Link className="tv-btn tv-btn-primary tv-btn-block" href="/kabinet">
          Kabinetga o‘tish
        </Link>
      </div>
    );
  }

  return (
    <form action={action} className="tv-form">
      <FormMessage state={state} />
      <input type="hidden" name="token" value={token} />
      <SubmitButton pendingText="Tasdiqlanmoqda…">Emailni tasdiqlash</SubmitButton>
    </form>
  );
}

export function ResendConfirmationForm() {
  const [state, action] = useActionState(resendConfirmationAction, initial);

  return (
    <form action={action} className="tv-banner-action">
      {state.message ? <FormMessage state={state} /> : <SubmitButton block={false} pendingText="Yuborilmoqda…">Xatni qayta yuborish</SubmitButton>}
    </form>
  );
}

export function ChangePasswordForm() {
  const [state, action] = useActionState(changePasswordAction, initial);
  const ref = useFocusFirstError(state);

  return (
    <form
      ref={ref}
      action={action}
      className="tv-form"
      noValidate
      // Muvaffaqiyatdan keyin React formani tozalaydi; kuch ko‘rsatkichi ham qayta boshlanishi uchun kalit
      key={state.success ? state.message : "form"}
    >
      <FormMessage state={state} />
      <PasswordField label="Joriy parol" name="currentPassword" autoComplete="current-password" error={state.fields?.currentPassword} required />
      <PasswordField label="Yangi parol" name="newPassword" autoComplete="new-password" hint={PASSWORD_HINT} error={state.fields?.newPassword} showStrength required />
      <SubmitButton block={false} pendingText="Saqlanmoqda…">Parolni o‘zgartirish</SubmitButton>
    </form>
  );
}
