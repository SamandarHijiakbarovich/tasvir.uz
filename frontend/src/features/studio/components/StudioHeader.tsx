import Link from "next/link";
import { Logo } from "@/shared/ui/Logo";
import { ThemeToggle } from "@/shared/ui/ThemeToggle";

export function StudioHeader() {
  return (
    <header className="tv-nav">
      <div className="tv-nav-in">
        <Logo />
        <div className="tv-nav-act" style={{ marginLeft: "auto" }}>
          <ThemeToggle />
          <Link className="tv-academy-link" href="/studiya">
            Studiya
          </Link>
          <Link className="tv-academy-link" href="/kabinet">
            Kabinet
          </Link>
        </div>
      </div>
    </header>
  );
}
