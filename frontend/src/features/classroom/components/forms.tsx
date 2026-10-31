"use client";

import Link from "next/link";
import { useActionState, useState } from "react";
import type { FormState } from "@/features/auth/actions";
import { FormMessage, SubmitButton, TextField, useFocusFirstError } from "@/features/auth/components/fields";
import {
  createGroupAction,
  joinGroupAction,
  saveReviewAction,
  saveSurveyAction,
  submitWorkAction,
} from "../actions";
import type { Review, RubricCriterion, SurveyQuestion } from "../types";

const initial: FormState = {};

export function CreateGroupForm() {
  const [state, action] = useActionState(createGroupAction, initial);
  const ref = useFocusFirstError(state);

  return (
    <form ref={ref} action={action} className="tv-form" noValidate>
      <FormMessage state={state} />
      <TextField label="Guruh nomi" name="name" placeholder="Masalan: Biologiya, 3-kurs" error={state.fields?.name} required />
      <SubmitButton pendingText="Yaratilmoqda…">Guruh yaratish</SubmitButton>
    </form>
  );
}

export function JoinGroupForm({ code }: { code?: string }) {
  const [state, action] = useActionState(joinGroupAction, initial);
  const ref = useFocusFirstError(state);

  return (
    <form ref={ref} action={action} className="tv-form" noValidate>
      <FormMessage state={state} />
      <TextField
        label="Guruh kodi"
        name="code"
        defaultValue={code}
        placeholder="Masalan: 4KMP7XQ2"
        autoCapitalize="characters"
        autoComplete="off"
        spellCheck={false}
        error={state.fields?.code}
        required
      />
      <div className="tv-field">
        <label className="tv-check tv-check-top">
          <input type="checkbox" name="acceptSharing" aria-invalid={state.fields?.acceptSharing ? true : undefined} />
          <span>
            Ismim, darslardagi progressim, test natijalarim va topshirgan ishlarim shu guruh o‘qituvchisiga ko‘rinishiga roziman.{" "}
            <Link href="/maxfiylik" target="_blank">
              Batafsil
            </Link>
          </span>
        </label>
        {state.fields?.acceptSharing && <p className="tv-field-error">{state.fields.acceptSharing}</p>}
      </div>
      <SubmitButton pendingText="Qo‘shilmoqda…">Guruhga qo‘shilish</SubmitButton>
    </form>
  );
}

type MaterialOption = { id: string; title: string };

export function SubmitWorkForm({ groupId, materials }: { groupId: string; materials: MaterialOption[] }) {
  const [state, action] = useActionState(submitWorkAction.bind(null, groupId), initial);

  return (
    <form action={action} className="tv-form" noValidate>
      <FormMessage state={state} />
      <div className="tv-field">
        <label htmlFor="material-select">Materialingiz</label>
        <select id="material-select" name="materialId" defaultValue="" className="tv-select" aria-invalid={state.fields?.materialId ? true : undefined}>
          <option value="" disabled>
            Tanlang…
          </option>
          {materials.map((m) => (
            <option key={m.id} value={m.id}>
              {m.title}
            </option>
          ))}
        </select>
        {state.fields?.materialId && <p className="tv-field-error">{state.fields.materialId}</p>}
        <p className="tv-field-hint">O‘qituvchiga hozirgi holati yuboriladi. Keyin o‘zgartirsangiz, qayta topshiring.</p>
      </div>
      <SubmitButton pendingText="Topshirilmoqda…" block={false}>
        Topshirish
      </SubmitButton>
    </form>
  );
}

type ReviewFormProps = {
  submissionId: string;
  rubric: RubricCriterion[];
  maxScore: number;
  review?: Review | null;
  isCurrent: boolean;
};

export function ReviewForm({ submissionId, rubric, maxScore, review, isCurrent }: ReviewFormProps) {
  const [state, action] = useActionState(saveReviewAction.bind(null, submissionId), initial);
  // Eski versiya bahosi yangi versiyaga boshlang‘ich qiymat bo‘ladi: o‘qituvchi faqat o‘zgarganini tuzatadi
  const [scores, setScores] = useState<Record<string, number>>(review?.scores ?? {});
  const filled = rubric.filter((c) => scores[c.key] !== undefined).length;
  const total = Object.values(scores).reduce((sum, value) => sum + value, 0);

  return (
    <form action={action} className="tv-form tv-rubric">
      <FormMessage state={state} />
      {state.fields?.scores && (
        <p className="tv-form-msg err" role="alert">
          {state.fields.scores}
        </p>
      )}
      {!isCurrent && review && <p className="tv-form-msg ok">Talaba ishni qayta topshirgan. Quyida oldingi versiya bahosi turibdi: yangi versiyani ko‘rib, saqlang.</p>}
      {rubric.map((criterion) => (
        <fieldset key={criterion.key} className="tv-rubric-row">
          <legend>{criterion.title}</legend>
          <p className="tv-field-hint">{criterion.description}</p>
          <div className="tv-score">
            {Array.from({ length: maxScore + 1 }, (_, value) => (
              <label key={value} className={scores[criterion.key] === value ? "on" : undefined}>
                <input
                  type="radio"
                  name={`score:${criterion.key}`}
                  value={value}
                  checked={scores[criterion.key] === value}
                  onChange={() => setScores((current) => ({ ...current, [criterion.key]: value }))}
                />
                {value}
              </label>
            ))}
          </div>
        </fieldset>
      ))}
      <div className="tv-field">
        <label htmlFor="review-comment">Izoh talabaga</label>
        <textarea id="review-comment" name="comment" rows={4} maxLength={1000} defaultValue={review?.comment ?? ""} className="tv-textarea" placeholder="Nima yaxshi chiqdi va nimani yaxshilash mumkin?" />
      </div>
      <p className="tv-rubric-total" aria-live="polite">
        Jami: <strong>{total}</strong> / {rubric.length * maxScore}
        {filled < rubric.length && ` · yana ${rubric.length - filled} ta mezon qoldi`}
      </p>
      <SubmitButton pendingText="Saqlanmoqda…" block={false}>
        Bahoni saqlash
      </SubmitButton>
    </form>
  );
}

const LIKERT = ["To‘liq qo‘shilmayman", "Qo‘shilmayman", "Betaraf", "Qo‘shilaman", "To‘liq qo‘shilaman"];

type SurveyFormProps = {
  groupId: string;
  questions: SurveyQuestion[];
  answers?: Record<string, number> | null;
  comment?: string | null;
};

export function SurveyForm({ groupId, questions, answers, comment }: SurveyFormProps) {
  const [state, action] = useActionState(saveSurveyAction.bind(null, groupId), initial);

  return (
    <form action={action} className="tv-form">
      <FormMessage state={state} />
      {state.fields?.answers && (
        <p className="tv-form-msg err" role="alert">
          {state.fields.answers}
        </p>
      )}
      {questions.map((question) => (
        <fieldset key={question.key} className="tv-rubric-row">
          <legend>{question.text}</legend>
          <div className="tv-likert">
            {LIKERT.map((label, index) => (
              <label key={label}>
                <input type="radio" name={`answer:${question.key}`} value={index + 1} defaultChecked={answers?.[question.key] === index + 1} />
                {label}
              </label>
            ))}
          </div>
        </fieldset>
      ))}
      <div className="tv-field">
        <label htmlFor="survey-comment">Nimani o‘zgartirish kerak? (ixtiyoriy)</label>
        <textarea id="survey-comment" name="comment" rows={3} maxLength={1000} defaultValue={comment ?? ""} className="tv-textarea" />
      </div>
      <SubmitButton pendingText="Yuborilmoqda…" block={false}>
        {answers ? "Javobni yangilash" : "Yuborish"}
      </SubmitButton>
    </form>
  );
}

/** Qaytarib bo‘lmaydigan amal: tasdiqlashdan keyin yuboriladi. */
export function ConfirmForm({ action, message, label, danger = true }: { action: () => Promise<void>; message: string; label: string; danger?: boolean }) {
  return (
    <form
      action={action}
      onSubmit={(event) => {
        if (!window.confirm(message)) event.preventDefault();
      }}
    >
      <button className={`tv-btn tv-btn-text${danger ? " tv-btn-danger" : ""}`} type="submit">
        {label}
      </button>
    </form>
  );
}
