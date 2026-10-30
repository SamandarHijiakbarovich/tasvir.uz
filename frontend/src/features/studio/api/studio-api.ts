import "server-only";
import { authedRequest } from "@/shared/lib/backend";
import type { Material } from "../lib/types";

const BASE = "/api/studio/materials";

export function listMaterials() {
  return authedRequest<Material[]>(BASE, { method: "GET" });
}

export function getMaterial(id: string) {
  return authedRequest<Material>(`${BASE}/${encodeURIComponent(id)}`, { method: "GET" });
}
