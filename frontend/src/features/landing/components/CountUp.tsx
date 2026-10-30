"use client";

import { useEffect, useState } from "react";
import { prefersReducedMotion, useInView } from "@/shared/lib/useInView";

/** Raqamni ekranga kirganda 0 dan sanab chiqaradi. Qiymat yo‘q bo‘lsa "—". */
export function CountUp({ value }: { value: number | null }) {
  const { ref, inView } = useInView<HTMLSpanElement>();
  const [shown, setShown] = useState<number | null>(null);

  useEffect(() => {
    if (!inView || value === null) return;
    if (prefersReducedMotion()) {
      setShown(value);
      return;
    }
    const start = performance.now();
    let frame = 0;
    const step = (now: number) => {
      const p = Math.min(1, (now - start) / 1400);
      setShown(Math.round(value * (1 - Math.pow(1 - p, 3))));
      if (p < 1) frame = requestAnimationFrame(step);
    };
    frame = requestAnimationFrame(step);
    return () => cancelAnimationFrame(frame);
  }, [inView, value]);

  return (
    <span ref={ref} className="tv-count">
      {value === null ? "—" : (shown ?? 0)}
    </span>
  );
}
