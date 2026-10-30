import { ArrowRightIcon, PlayIcon } from "@/shared/ui/icons";
import { HeroStudio } from "./HeroStudio";

export function Hero() {
  return (
    <section className="hero" aria-labelledby="hero-t">
      <div className="hero-in">
        <div className="hero-copy">
          <span className="tv-badge tv-badge-mint">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" aria-hidden="true">
              <path d="M12 3l2.6 5.6 6 .7-4.5 4.1 1.2 6L12 16.4 6.7 19.4l1.2-6L3.4 9.3l6-.7z" />
            </svg>
            Ilmiy tadqiqot asosidagi metodika
          </span>
          <h1 className="tv-display" id="hero-t">
            Murakkabni <span className="tv-mark">oddiy</span>, zerikarlini{" "}
            <span className="tv-circled">
              qiziqarli
              <svg viewBox="0 0 200 70" preserveAspectRatio="none" aria-hidden="true">
                <path d="M30 12C70 2 150 2 186 22s-6 42-86 44S6 58 8 38 40 10 84 8" />
              </svg>
            </span>{" "}
            qiling
          </h1>
          <p className="tv-lead">
            Tasvir.uz o‘qituvchilarga o‘quv materialini infografika, sxema, diagramma va aqliy xaritaga aylantirishni
            o‘rgatadi — va buni bir necha bosishda bajarishga yordam beradi.
          </p>
          <div className="hero-cta">
            <a className="tv-btn tv-btn-primary" href="#boshlash">
              Bepul boshlash <ArrowRightIcon />
            </a>
            <a className="tv-btn tv-btn-ghost" href="#demo">
              <PlayIcon /> Demo ko‘rish
            </a>
          </div>
          <div className="hero-note">
            <svg viewBox="0 0 24 24" aria-hidden="true">
              <path d="M5 12l5 5 9-10" />
            </svg>
            Karta talab qilinmaydi · O‘zbek tilida
          </div>
          <div className="hero-doodle" aria-hidden="true">
            <span className="tv-hand">matn → infografika</span>
            <svg viewBox="0 0 70 50">
              <path d="M4 40c18 4 40-6 52-30M46 12l10-2 2 11" />
            </svg>
          </div>
        </div>
        <HeroStudio />
      </div>
    </section>
  );
}
