import { apiGet } from "@/shared/lib/http";
import type { ToolCatalog } from "./types";

export function getToolCatalog() {
  return apiGet<ToolCatalog>("/api/content/tools", { revalidate: 3600 });
}
