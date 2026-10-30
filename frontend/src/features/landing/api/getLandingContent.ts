import { apiGet } from "@/shared/lib/http";
import type { LandingContent } from "./types";

export function getLandingContent() {
  return apiGet<LandingContent>("/api/content/landing", { revalidate: 60 });
}
