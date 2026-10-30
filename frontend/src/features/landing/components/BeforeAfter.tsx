"use client";

import { useState, type CSSProperties } from "react";
import { SectionHead } from "./SectionHead";

const num = { font: "800 30px var(--font-sans)", fill: "#17162b" };
const lbl = { font: "700 26px var(--font-sans)", fill: "#17162b" };
const note = { font: "500 24px var(--font-sans)", fill: "var(--ink-muted)" };
const center = { font: "800 34px var(--font-display)", fill: "var(--on-brand)" };

export function BeforeAfter() {
  const [pos, setPos] = useState(50);

  return (
    <section className="tv-section" id="demo" aria-labelledby="ba-t">
      <div className="tv-wrap">
        <SectionHead
          id="ba-t"
          label="Oldin / Keyin"
          title="Bir xil mavzu — butunlay boshqa taassurot"
          lead="Slayderni suring va oddiy matnli slayd Tasvir.uz da qanday ko‘rinishga kelishini solishtiring."
        />
        <div className="tv-ba tv-reveal" style={{ "--pos": `${pos}%` } as CSSProperties}>
          <div className="tv-ba-pane ba-before" role="img" aria-label="Oldin: ‘Suvning tabiatda aylanishi’ mavzusidagi faqat matndan iborat slayd">
            <h4>Suvning tabiatda aylanishi</h4>
            <p>
              Quyosh issiqligi ta’sirida dengiz, ko‘l va daryolar yuzasidan suv bug‘lanadi. Suv bug‘i havoga ko‘tariladi,
              soviydi va mayda tomchilarga aylanib bulutlarni hosil qiladi (kondensatsiya). Tomchilar yiriklashgach,
              bulutlardan yomg‘ir yoki qor ko‘rinishida yog‘in tushadi. Yog‘in suvlari daryolar va yer osti suvlari orqali
              yana dengiz va okeanlarga oqib boradi. Shu tariqa jarayon qayta-qayta takrorlanadi.
            </p>
          </div>
          <div
            className="tv-ba-pane ba-after tv-ba-after"
            role="img"
            aria-label="Keyin: xuddi shu mavzu — bug‘lanish, kondensatsiya, yog‘in va oqim bosqichlari aylana sxemada"
          >
            <svg viewBox="0 0 1600 900" preserveAspectRatio="xMidYMid slice">
              <text x="800" y="96" textAnchor="middle" style={{ font: "800 56px var(--font-display)", fill: "var(--ink)" }}>
                Suvning tabiatda aylanishi
              </text>
              <circle cx="800" cy="500" r="250" fill="none" strokeDasharray="4 18" strokeLinecap="round" style={{ stroke: "var(--ink-muted)", strokeWidth: 5 }} />
              <g style={{ fill: "none", stroke: "var(--ink)", strokeWidth: 6, strokeLinecap: "round", strokeLinejoin: "round" }}>
                <path d="M620 330a250 250 0 0 1 150-80" />
                <path d="M752 232l22 18-26 14" />
                <path d="M1030 360a250 250 0 0 1 20 150" />
                <path d="M1070 492l-20 22-16-24" />
                <path d="M980 680a250 250 0 0 1-150 70" />
                <path d="M850 770l-24-18 26-14" />
                <path d="M570 640a250 250 0 0 1-20-150" />
                <path d="M530 508l20-22 16 24" />
              </g>
              <circle cx="800" cy="500" r="120" style={{ fill: "var(--brand)" }} />
              <text x="800" y="490" textAnchor="middle" style={center}>Suv</text>
              <text x="800" y="530" textAnchor="middle" style={center}>aylanishi</text>
              <g>
                <circle cx="560" cy="250" r="92" style={{ fill: "var(--sun)" }} />
                <text x="560" y="244" textAnchor="middle" style={num}>1</text>
                <text x="560" y="282" textAnchor="middle" style={lbl}>Bug‘lanish</text>
              </g>
              <g>
                <circle cx="1060" cy="260" r="92" style={{ fill: "var(--card)", stroke: "var(--line-strong)", strokeWidth: 4 }} />
                <text x="1060" y="254" textAnchor="middle" style={{ ...num, fill: "var(--ink)" }}>2</text>
                <text x="1060" y="292" textAnchor="middle" style={{ font: "700 22px var(--font-sans)", fill: "var(--ink)" }}>Kondensatsiya</text>
              </g>
              <g>
                <circle cx="1060" cy="750" r="92" style={{ fill: "var(--mint)" }} />
                <text x="1060" y="744" textAnchor="middle" style={num}>3</text>
                <text x="1060" y="782" textAnchor="middle" style={lbl}>Yog‘in</text>
              </g>
              <g>
                <circle cx="540" cy="740" r="92" style={{ fill: "var(--coral)" }} />
                <text x="540" y="734" textAnchor="middle" style={num}>4</text>
                <text x="540" y="772" textAnchor="middle" style={lbl}>Oqim</text>
              </g>
              <text x="250" y="250" textAnchor="middle" style={note}>quyosh isitadi</text>
              <text x="1350" y="260" textAnchor="middle" style={note}>bulut hosil bo‘ladi</text>
              <text x="1350" y="750" textAnchor="middle" style={note}>yomg‘ir, qor</text>
              <text x="250" y="740" textAnchor="middle" style={note}>daryo → dengiz</text>
            </svg>
          </div>
          <span className="tv-ba-tag l tv-sticker tv-sticker-sun" style={{ transform: "rotate(-3deg)" }}>
            Oldin
          </span>
          <span className="tv-ba-tag r tv-sticker tv-sticker-mint" style={{ transform: "rotate(3deg)" }}>
            Keyin
          </span>
          <input
            type="range"
            min={0}
            max={100}
            value={pos}
            onChange={(e) => setPos(Number(e.target.value))}
            aria-label="Oldin va keyin ko‘rinishini solishtirish slayderi"
          />
          <div className="tv-ba-line">
            <div className="tv-ba-knob">
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M9 6l-5 6 5 6M15 6l5 6-5 6" />
              </svg>
            </div>
          </div>
        </div>
        <p className="ba-hint">← Strelka tugmalari bilan ham boshqarish mumkin →</p>
      </div>
    </section>
  );
}
