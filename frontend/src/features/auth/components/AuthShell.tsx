import type { ReactNode } from "react";
import { Logo } from "@/shared/ui/Logo";
import { ThemeToggle } from "@/shared/ui/ThemeToggle";

type AuthShellProps = {
  title: string;
  lead?: ReactNode;
  children: ReactNode;
  footer?: ReactNode;
};

export function AuthShell({ title, lead, children, footer }: AuthShellProps) {
  return (
    <div className="tv-auth">
      <aside className="tv-auth-aside" aria-hidden="true">
        <p className="tv-hand">matn → sxema</p>
        <p className="tv-auth-slogan">Murakkabni oddiy, zerikarlini qiziqarli qiling</p>
        <div className="tv-auth-visual">
          <div className="tv-auth-text">
            <i />
            <i />
            <i />
            <i />
          </div>
          <svg viewBox="0 0 48 24">
            <path d="M2 12h40M34 4l8 8-8 8" />
          </svg>
          <div className="tv-auth-diagram">
            <b />
            <span>
              <b />
              <b />
            </span>
          </div>
        </div>
      </aside>

      <main className="tv-auth-main">
        <div className="tv-auth-top">
          <Logo />
          <ThemeToggle />
        </div>
        <div className="tv-auth-card">
          <h1 className="tv-h2">{title}</h1>
          {lead && <p className="tv-muted tv-auth-lead">{lead}</p>}
          {children}
          {footer && <p className="tv-auth-foot">{footer}</p>}
        </div>
      </main>
    </div>
  );
}
