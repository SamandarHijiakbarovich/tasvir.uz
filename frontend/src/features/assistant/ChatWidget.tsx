"use client";

import Link from "next/link";
import { useEffect, useRef, useState, useTransition, type FormEvent } from "react";
import { askAssistantAction } from "./actions";

type Message = {
  id: number;
  role: "user" | "bot";
  text: string;
  link?: { label: string; href: string } | null;
};

const MAX_LENGTH = 500;
const WELCOME = "Assalomu alaykum! Men Tasvir.uz yordamchisiman. Sayt, darslar va vizuallashtirish asoslari haqidagi oddiy savollarga javob beraman.";
const INITIAL_SUGGESTIONS = ["Tasvir.uz nima?", "Qanday boshlayman?", "Qaysi shaklni tanlayman?", "Parolimni unutdim"];

export function ChatWidget() {
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([{ id: 0, role: "bot", text: WELCOME }]);
  const [suggestions, setSuggestions] = useState(INITIAL_SUGGESTIONS);
  const [draft, setDraft] = useState("");
  const [pending, startTransition] = useTransition();
  const logRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const toggleRef = useRef<HTMLButtonElement>(null);
  const nextId = useRef(1);

  useEffect(() => {
    if (open) inputRef.current?.focus();
  }, [open]);

  useEffect(() => {
    logRef.current?.scrollTo({ top: logRef.current.scrollHeight });
  }, [messages, pending, open]);

  const close = () => {
    setOpen(false);
    toggleRef.current?.focus();
  };

  const add = (message: Omit<Message, "id">) => {
    const id = nextId.current++;
    setMessages((prev) => [...prev, { ...message, id }]);
  };

  const ask = (question: string) => {
    const text = question.trim();
    if (!text || pending) return;

    add({ role: "user", text });
    setDraft("");
    setSuggestions([]);

    startTransition(async () => {
      try {
        const response = await askAssistantAction(text);
        if (response.ok) {
          add({ role: "bot", text: response.data.answer, link: response.data.link });
          setSuggestions(response.data.suggestions);
        } else {
          add({ role: "bot", text: response.message });
        }
      } catch {
        add({ role: "bot", text: "Hozir javob bera olmadim. Birozdan so‘ng qayta urinib ko‘ring." });
      }
      inputRef.current?.focus();
    });
  };

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    ask(draft);
  };

  return (
    <div className="tv-chat">
      {open && (
        <section
          className="tv-chat-panel"
          role="dialog"
          aria-label="Tasvir.uz yordamchisi"
          onKeyDown={(e) => {
            if (e.key === "Escape") close();
          }}
        >
          <header className="tv-chat-head">
            <div>
              <strong>Yordamchi</strong>
              <span>Avtomatik javob beradi, inson emas</span>
            </div>
            <button className="tv-chat-close" type="button" onClick={close} aria-label="Yordamchini yopish">
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M6 6l12 12M18 6L6 18" />
              </svg>
            </button>
          </header>

          <div className="tv-chat-log" ref={logRef} role="log" aria-live="polite" aria-label="Suhbat" tabIndex={0}>
            {messages.map((message) => (
              <div key={message.id} className={`tv-chat-msg ${message.role}`}>
                <span className="tv-sr">{message.role === "user" ? "Siz:" : "Yordamchi:"}</span>
                <p>{message.text}</p>
                {message.link && (
                  <Link href={message.link.href} onClick={() => setOpen(false)}>
                    {message.link.label} →
                  </Link>
                )}
              </div>
            ))}
            {pending && <div className="tv-chat-msg bot tv-chat-typing">Javob tayyorlanmoqda…</div>}
          </div>

          {suggestions.length > 0 && !pending && (
            <div className="tv-chat-suggest" aria-label="Tavsiya etilgan savollar">
              {suggestions.map((suggestion) => (
                <button key={suggestion} type="button" onClick={() => ask(suggestion)}>
                  {suggestion}
                </button>
              ))}
            </div>
          )}

          <form className="tv-chat-form" onSubmit={onSubmit}>
            <label className="tv-sr" htmlFor="tv-chat-input">
              Savolingiz
            </label>
            <input
              id="tv-chat-input"
              ref={inputRef}
              value={draft}
              maxLength={MAX_LENGTH}
              autoComplete="off"
              placeholder="Savolingizni yozing…"
              onChange={(e) => setDraft(e.target.value)}
            />
            <button type="submit" disabled={pending || !draft.trim()} aria-label="Yuborish">
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M5 12h14M13 6l6 6-6 6" />
              </svg>
            </button>
          </form>
        </section>
      )}

      {!open && (
        <button className="tv-chat-toggle" ref={toggleRef} type="button" onClick={() => setOpen(true)} aria-haspopup="dialog">
          <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M21 12a8 8 0 0 1-8 8H4l2.5-3.2A8 8 0 1 1 21 12z" />
          </svg>
          Savol bering
        </button>
      )}
    </div>
  );
}
