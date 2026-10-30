"use client";

import { useEffect } from "react";

/**
 * .tv-reveal elementlari ko‘rinish maydoniga kirganda .in klassini qo‘shadi (scroll animatsiyasi).
 * Sahifaga bir marta qo‘yiladi.
 */
export function RevealObserver() {
  useEffect(() => {
    const items = Array.from(document.querySelectorAll<HTMLElement>(".tv-reveal"));
    const showAll = () => items.forEach((el) => el.classList.add("in"));

    if (!("IntersectionObserver" in window)) {
      showAll();
      return;
    }

    const io = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (!entry.isIntersecting) continue;
          entry.target.classList.add("in");
          io.unobserve(entry.target);
        }
      },
      { threshold: 0.15 },
    );

    items.forEach((el, i) => {
      el.style.transitionDelay = `${(i % 3) * 80}ms`;
      io.observe(el);
    });

    // Zaxira: biror sababga ko‘ra kuzatuvchi ishlamasa ham kontent yashirin qolmasin.
    const fallback = window.setTimeout(showAll, 2500);

    return () => {
      io.disconnect();
      window.clearTimeout(fallback);
    };
  }, []);

  return null;
}
