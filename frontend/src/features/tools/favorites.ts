"use client";

import { useSyncExternalStore } from "react";

const KEY = "tasvir.sevimli-vositalar";
const EMPTY: ReadonlySet<string> = new Set();
const listeners = new Set<() => void>();
let cache: { raw: string | null; set: ReadonlySet<string> } = { raw: null, set: EMPTY };

function read(): ReadonlySet<string> {
  let raw: string | null = null;
  try {
    raw = window.localStorage.getItem(KEY);
  } catch {
    // Brauzer xotirasi yopiq (maxfiy rejim): sevimlilar shu sahifa ochiq turganda saqlanadi
    return cache.set;
  }
  if (raw !== cache.raw) {
    let keys: string[] = [];
    try {
      const parsed: unknown = raw ? JSON.parse(raw) : [];
      if (Array.isArray(parsed)) keys = parsed.filter((k): k is string => typeof k === "string");
    } catch {
      keys = [];
    }
    cache = { raw, set: new Set(keys) };
  }
  return cache.set;
}

function write(next: ReadonlySet<string>) {
  const raw = JSON.stringify([...next]);
  cache = { raw, set: next };
  try {
    window.localStorage.setItem(KEY, raw);
  } catch {
    // saqlab bo‘lmadi — xotiradagi qiymat qoladi
  }
  listeners.forEach((l) => l());
}

function subscribe(listener: () => void) {
  listeners.add(listener);
  const onStorage = (e: StorageEvent) => e.key === KEY && listener();
  window.addEventListener("storage", onStorage);
  return () => {
    listeners.delete(listener);
    window.removeEventListener("storage", onStorage);
  };
}

/** Sevimli dasturlar (kalitlari). Shu brauzerda saqlanadi, boshqa oynalar bilan sinxron. */
export function useFavoriteTools() {
  const set = useSyncExternalStore(subscribe, read, () => EMPTY);
  return {
    size: set.size,
    has: (key: string) => set.has(key),
    toggle: (key: string) => {
      const next = new Set(set);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      write(next);
    },
  };
}
