"use client";

import Link from "next/link";
import { Fragment, useEffect, useId, useRef, useState, useTransition } from "react";
import { completeStepAction, startLessonAction } from "../actions";
import type { Lesson, LessonStep, PracticeContent, ReadingContent, StepAnswer, StepResult } from "../api/types";
import { DiagramDownload } from "./DiagramDownload";
import { ProcessDiagram } from "./ProcessDiagram";
import { VideoStep } from "./VideoStep";

/** Matndagi **qalin** belgisini ko‘rsatadi. Mazmun bazadan keladi, HTML sifatida talqin qilinmaydi. */
function RichText({ text }: { text: string }) {
  return (
    <>
      {text.split(/(\*\*[^*]+\*\*)/g).map((part, index) =>
        part.startsWith("**") ? <strong key={index}>{part.slice(2, -2)}</strong> : <Fragment key={index}>{part}</Fragment>,
      )}
    </>
  );
}

function ReadingStep({ content }: { content: ReadingContent }) {
  return (
    <div className="tv-step-body">
      {content.paragraphs.map((paragraph) => (
        <p key={paragraph}>
          <RichText text={paragraph} />
        </p>
      ))}
      {content.example && (
        <div className="tv-example">
          <div className="tv-example-side">
            <span className="tv-label">{content.example.beforeLabel}</span>
            <p>{content.example.before}</p>
          </div>
          <div className="tv-example-side">
            <span className="tv-label">{content.example.afterLabel}</span>
            <ProcessDiagram items={content.example.after} />
          </div>
        </div>
      )}
      {content.callout && <p className="tv-callout">{content.callout}</p>}
    </div>
  );
}

type AnswerProps = {
  step: LessonStep;
  answer: StepAnswer;
  onChange: (answer: StepAnswer) => void;
  readOnly: boolean;
};

function QuizStep({ step, answer, onChange, readOnly }: AnswerProps) {
  const name = useId();
  const quiz = step.quiz!;

  return (
    <fieldset className="tv-quiz" disabled={readOnly}>
      <legend>{quiz.question}</legend>
      {quiz.options.map((option) => (
        <label key={option.id} className={`tv-option${answer.optionId === option.id ? " on" : ""}`}>
          <input type="radio" name={name} checked={answer.optionId === option.id} onChange={() => onChange({ optionId: option.id })} />
          <span>{option.text}</span>
        </label>
      ))}
    </fieldset>
  );
}

function ProcessBuilder({ step, practice, answer, onChange, readOnly }: AnswerProps & { practice: PracticeContent }) {
  const items = answer.items?.length ? answer.items : Array.from({ length: practice.minItems }, () => "");
  const setItem = (index: number, value: string) => onChange({ items: items.map((item, i) => (i === index ? value : item)) });

  return (
    <div className="tv-builder">
      <div className="tv-builder-fields">
        {items.map((item, index) => (
          <div className="tv-builder-row" key={index}>
            <label className="tv-sr" htmlFor={`bosqich-${index}`}>
              {index + 1}-bosqich nomi
            </label>
            <span className="tv-diagram-num" aria-hidden="true">
              {index + 1}
            </span>
            <input
              id={`bosqich-${index}`}
              value={item}
              maxLength={80}
              disabled={readOnly}
              placeholder={practice.placeholder ?? undefined}
              onChange={(e) => setItem(index, e.target.value)}
            />
            {!readOnly && items.length > practice.minItems && (
              <button
                type="button"
                className="tv-iconbtn tv-builder-remove"
                aria-label={`${index + 1}-bosqichni o‘chirish`}
                onClick={() => onChange({ items: items.filter((_, i) => i !== index) })}
              >
                <svg viewBox="0 0 24 24" aria-hidden="true">
                  <path d="M6 6l12 12M18 6L6 18" />
                </svg>
              </button>
            )}
          </div>
        ))}
        {!readOnly && items.length < practice.maxItems && (
          <button type="button" className="tv-btn tv-btn-text tv-builder-add" onClick={() => onChange({ items: [...items, ""] })}>
            + Bosqich qo‘shish
          </button>
        )}
      </div>
      <div className="tv-builder-preview" aria-live="polite">
        <span className="tv-label">Sizning sxemangiz</span>
        <ProcessDiagram items={items} />
        {readOnly && <DiagramDownload items={items} name={`sxema-${step.id.slice(0, 8)}`} />}
      </div>
    </div>
  );
}

function PracticeStep({ step, answer, onChange, readOnly }: AnswerProps) {
  const practice = step.practice!;
  const textId = useId();

  return (
    <div className="tv-step-body">
      <p>{practice.prompt}</p>

      {practice.mode === "text" && (
        <div className="tv-field">
          <label htmlFor={textId}>Javobingiz</label>
          <textarea
            id={textId}
            rows={5}
            maxLength={2000}
            disabled={readOnly}
            placeholder={practice.placeholder ?? undefined}
            value={answer.text ?? ""}
            onChange={(e) => onChange({ text: e.target.value })}
          />
        </div>
      )}

      {practice.mode === "process" && <ProcessBuilder step={step} practice={practice} answer={answer} onChange={onChange} readOnly={readOnly} />}

      {practice.mode === "checklist" && (
        <fieldset className="tv-quiz" disabled={readOnly}>
          <legend className="tv-sr">O‘zini tekshirish bandlari</legend>
          {practice.checklist?.map((item) => {
            const checked = answer.items?.includes(item) ?? false;
            return (
              <label key={item} className={`tv-option${checked ? " on" : ""}`}>
                <input
                  type="checkbox"
                  checked={checked}
                  onChange={() =>
                    onChange({ items: checked ? (answer.items ?? []).filter((i) => i !== item) : [...(answer.items ?? []), item] })
                  }
                />
                <span>{item}</span>
              </label>
            );
          })}
        </fieldset>
      )}
    </div>
  );
}

function ExerciseStep({ step, answer, onChange, onSubmit, readOnly }: AnswerProps & { onSubmit: () => void }) {
  const id = useId();
  const exercise = step.exercise!;

  return (
    <div className="tv-step-body">
      <p className="tv-exercise-prompt">{exercise.prompt}</p>
      <div className="tv-field tv-exercise-answer">
        <label htmlFor={id}>Javobingiz</label>
        <div>
          <input
            id={id}
            inputMode="decimal"
            autoComplete="off"
            maxLength={20}
            disabled={readOnly}
            value={answer.text ?? ""}
            onChange={(e) => onChange({ text: e.target.value })}
            onKeyDown={(e) => {
              if (e.key === "Enter" && answer.text?.trim()) onSubmit();
            }}
          />
          {exercise.unit && <span>{exercise.unit}</span>}
        </div>
      </div>
    </div>
  );
}

function CompletionScreen({ lesson, nextLessonSlug }: { lesson: Lesson; nextLessonSlug: string | null }) {
  const courseDone = !nextLessonSlug;

  return (
    <section className="tv-card tv-card-outline tv-lesson-done" aria-labelledby="done-t">
      <span className="tv-hand">{courseDone ? "kurs yakunlandi!" : "barakalla!"}</span>
      <h2 className="tv-h2" id="done-t" tabIndex={-1}>
        “{lesson.title}” darsi yakunlandi
      </h2>
      <p className="tv-muted">
        {courseDone
          ? "Siz bu kursning barcha darslarini tugatdingiz. Sxemangizni endi o‘z darsingizda sinab ko‘ring."
          : "Natijalaringiz saqlandi. Keyingi dars ochildi."}
      </p>
      <div className="tv-lesson-actions">
        {nextLessonSlug ? (
          <form action={startLessonAction.bind(null, nextLessonSlug)}>
            <button className="tv-btn tv-btn-primary" type="submit">
              Keyingi darsni boshlash →
            </button>
          </form>
        ) : null}
        <Link className="tv-btn tv-btn-ghost" href={`/akademiya/kurslar/${lesson.courseSlug}`}>
          Kurs sahifasiga qaytish
        </Link>
        <Link className="tv-btn tv-btn-ghost" href="/akademiya/ishlarim">
          Mening ishlarim
        </Link>
      </div>
    </section>
  );
}

const firstOpenStep = (steps: LessonStep[]) => {
  const index = steps.findIndex((s) => !s.isCompleted);
  return index === -1 ? 0 : index;
};

export function LessonPlayer({ lesson }: { lesson: Lesson }) {
  const [index, setIndex] = useState(() => firstOpenStep(lesson.steps));
  const [completed, setCompleted] = useState(() => new Set(lesson.steps.filter((s) => s.isCompleted).map((s) => s.id)));
  const [answers, setAnswers] = useState<Record<string, StepAnswer>>(() =>
    Object.fromEntries(lesson.steps.filter((s) => s.savedResponse).map((s) => [s.id, s.savedResponse!])),
  );
  const [feedback, setFeedback] = useState<{ stepId: string; correct: boolean; text: string } | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [finished, setFinished] = useState<{ nextLessonSlug: string | null } | null>(null);
  const [pending, startTransition] = useTransition();
  const headingRef = useRef<HTMLHeadingElement>(null);
  const isFirstRender = useRef(true);

  const step = lesson.steps[index];
  const isStepDone = completed.has(step.id);
  const answer = answers[step.id] ?? {};
  const stepFeedback = feedback?.stepId === step.id ? feedback : null;
  const progress = Math.round((completed.size / lesson.steps.length) * 100);

  // Bosqich almashganda fokus sarlavhaga o‘tadi (klaviatura va ekran o‘quvchilari uchun)
  useEffect(() => {
    if (isFirstRender.current) {
      isFirstRender.current = false;
      return;
    }
    headingRef.current?.focus();
    window.scrollTo({ top: 0 });
  }, [index]);

  const goTo = (next: number) => {
    setError(null);
    setIndex(next);
  };

  const advance = (result?: StepResult) => {
    if (index < lesson.steps.length - 1) {
      goTo(index + 1);
    } else {
      setFinished({ nextLessonSlug: result ? (result.nextLessonSlug ?? null) : lesson.nextLessonSlug });
    }
  };

  const submit = () => {
    setError(null);
    startTransition(async () => {
      const response = await completeStepAction(lesson.slug, step.id, answer);

      if (!response.ok) {
        if (response.sessionExpired) {
          window.location.assign(`/kirish?keyin=${encodeURIComponent(`/akademiya/darslar/${lesson.slug}`)}`);
          return;
        }
        setError(response.message);
        return;
      }

      const { result } = response;
      if (result.stepCompleted) {
        setCompleted((prev) => new Set(prev).add(step.id));
      }

      if ((step.kind === "quiz" || step.kind === "exercise") && result.feedback) {
        // Test izohini o‘qib olish uchun keyingi bosqichga o‘zi o‘tmaydi
        setFeedback({ stepId: step.id, correct: Boolean(result.isCorrect), text: result.feedback });
        return;
      }

      if (result.stepCompleted) advance(result);
    });
  };

  if (finished) {
    return <CompletionScreen lesson={lesson} nextLessonSlug={finished.nextLessonSlug} />;
  }

  // Yangi javob tanlanganda oldingi "noto‘g‘ri" izohi o‘chadi
  const onAnswer = (value: StepAnswer) => {
    setAnswers((prev) => ({ ...prev, [step.id]: value }));
    setFeedback(null);
    setError(null);
  };

  const primaryLabel = isStepDone
    ? index === lesson.steps.length - 1
      ? "Yakunlash"
      : "Davom etish"
    : step.kind === "quiz" || step.kind === "exercise"
      ? "Javobni tekshirish"
      : step.kind === "video"
        ? "Ko‘rdim, mashqqa o‘taman"
        : step.kind === "reading"
          ? "Tushundim, davom etamiz"
          : "Saqlash va davom etish";

  return (
    <div className="tv-lesson">
      <div className="tv-lesson-progress">
        <span>
          {index + 1}-bosqich / {lesson.steps.length}
        </span>
        <div
          className="tv-progress"
          role="progressbar"
          aria-label="Dars bo‘yicha natija"
          aria-valuenow={progress}
          aria-valuemin={0}
          aria-valuemax={100}
        >
          <i style={{ width: `${progress}%` }} />
        </div>
      </div>

      <section className="tv-card tv-lesson-card" aria-labelledby="step-t">
        <h2 className="tv-h3" id="step-t" ref={headingRef} tabIndex={-1}>
          {step.title}
        </h2>

        {step.kind === "reading" && step.reading && <ReadingStep content={step.reading} />}
        {step.kind === "video" && step.video && <VideoStep content={step.video} />}
        {step.kind === "exercise" && <ExerciseStep step={step} answer={answer} onChange={onAnswer} onSubmit={submit} readOnly={isStepDone} />}
        {step.kind === "quiz" && <QuizStep step={step} answer={answer} onChange={onAnswer} readOnly={isStepDone} />}
        {step.kind === "practice" && <PracticeStep step={step} answer={answer} onChange={onAnswer} readOnly={isStepDone} />}

        {stepFeedback && (
          <p className={`tv-form-msg ${stepFeedback.correct ? "ok" : "err"}`} role="status">
            {stepFeedback.text}
          </p>
        )}
        {error && (
          <p className="tv-form-msg err" role="alert">
            {error}
          </p>
        )}

        <div className="tv-lesson-actions">
          {index > 0 && (
            <button className="tv-btn tv-btn-ghost" type="button" onClick={() => goTo(index - 1)} disabled={pending}>
              Orqaga
            </button>
          )}
          <button
            className="tv-btn tv-btn-primary"
            type="button"
            disabled={pending || (!isStepDone && ((step.kind === "quiz" && !answer.optionId) || (step.kind === "exercise" && !answer.text?.trim())))}
            aria-busy={pending}
            onClick={isStepDone ? () => advance() : submit}
          >
            {pending ? "Saqlanmoqda…" : primaryLabel}
          </button>
        </div>
      </section>
    </div>
  );
}
