"use client";

import { useEffect, useId, useRef, useState, type InputHTMLAttributes, type ReactNode } from "react";
import { useFormStatus } from "react-dom";
import type { FormState } from "../actions";

type FieldProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string;
  name: string;
  error?: string;
  hint?: ReactNode;
};

export function TextField({ label, name, error, hint, ...input }: FieldProps) {
  const id = useId();
  const describedBy = error ? `${id}-error` : hint ? `${id}-hint` : undefined;

  return (
    <div className="tv-field">
      <label htmlFor={id}>{label}</label>
      <input id={id} name={name} aria-invalid={error ? true : undefined} aria-describedby={describedBy} {...input} />
      {hint && !error && (
        <p className="tv-field-hint" id={`${id}-hint`}>
          {hint}
        </p>
      )}
      {error && (
        <p className="tv-field-error" id={`${id}-error`}>
          {error}
        </p>
      )}
    </div>
  );
}

type Strength = { level: 1 | 2 | 3 | 4; label: string };

function measure(password: string): Strength {
  let score = 0;
  if (password.length >= 8) score++;
  if (password.length >= 12) score++;
  if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score++;
  if (/\d/.test(password) && /[^a-zA-Z0-9]/.test(password)) score++;

  if (password.length < 8 || score <= 1) return { level: 1, label: "Zaif" };
  if (score === 2) return { level: 2, label: "O‘rtacha" };
  if (score === 3) return { level: 3, label: "Yaxshi" };
  return { level: 4, label: "Kuchli" };
}

type PasswordFieldProps = Omit<FieldProps, "type"> & { showStrength?: boolean };

export function PasswordField({ label, name, error, hint, showStrength, ...input }: PasswordFieldProps) {
  const id = useId();
  const [visible, setVisible] = useState(false);
  const [value, setValue] = useState("");
  const strength = showStrength && value ? measure(value) : null;
  const describedBy = [error ? `${id}-error` : hint ? `${id}-hint` : null, strength ? `${id}-strength` : null].filter(Boolean).join(" ") || undefined;

  return (
    <div className="tv-field">
      <label htmlFor={id}>{label}</label>
      <div className="tv-password">
        <input
          id={id}
          name={name}
          type={visible ? "text" : "password"}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          onChange={(e) => setValue(e.target.value)}
          autoCapitalize="none"
          autoCorrect="off"
          spellCheck={false}
          {...input}
        />
        <button type="button" onClick={() => setVisible((v) => !v)} aria-pressed={visible} aria-label={visible ? "Parolni yashirish" : "Parolni ko‘rsatish"}>
          <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7-10-7-10-7z" />
            <circle cx="12" cy="12" r="3" />
            {visible && <path d="M4 4l16 16" />}
          </svg>
        </button>
      </div>
      {strength && (
        <div className="tv-strength" data-level={strength.level} id={`${id}-strength`} aria-live="polite">
          <span className="tv-strength-bars" aria-hidden="true">
            <i />
            <i />
            <i />
            <i />
          </span>
          <span>Parol kuchi: {strength.label}</span>
        </div>
      )}
      {hint && !error && (
        <p className="tv-field-hint" id={`${id}-hint`}>
          {hint}
        </p>
      )}
      {error && (
        <p className="tv-field-error" id={`${id}-error`}>
          {error}
        </p>
      )}
    </div>
  );
}

export function SubmitButton({ children, pendingText, block = true }: { children: ReactNode; pendingText: string; block?: boolean }) {
  const { pending } = useFormStatus();
  return (
    <button className={`tv-btn tv-btn-primary${block ? " tv-btn-block" : ""}`} type="submit" disabled={pending} aria-busy={pending}>
      {pending ? pendingText : children}
    </button>
  );
}

export function FormMessage({ state }: { state: FormState }) {
  if (!state.message) return null;
  return (
    <p className={`tv-form-msg ${state.success ? "ok" : "err"}`} role={state.success ? "status" : "alert"}>
      {state.message}
    </p>
  );
}

/** Xatodan keyin fokusni birinchi noto‘g‘ri maydonga o‘tkazadi (klaviatura va ekran o‘quvchilari uchun). */
export function useFocusFirstError(state: FormState) {
  const ref = useRef<HTMLFormElement>(null);
  useEffect(() => {
    ref.current?.querySelector<HTMLInputElement>('[aria-invalid="true"]')?.focus();
  }, [state]);
  return ref;
}
