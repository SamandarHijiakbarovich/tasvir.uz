import "server-only";
import { authedRequest, backendRequest } from "@/shared/lib/backend";
import type { Catalog, MyPayments, Order } from "./types";

const BASE = "/api/payments";

/** Tariflar va ulangan tölov tizimlari. Kalit qöyilgaç darhol körinişi uçun keşlanmaydi. */
export function getCatalog() {
  return backendRequest<Catalog>(`${BASE}/plans`, { method: "GET" });
}

export function getOrder(id: string) {
  return authedRequest<Order>(`${BASE}/orders/${encodeURIComponent(id)}`, { method: "GET" });
}

export function getMyPayments() {
  return authedRequest<MyPayments>(`${BASE}/me`, { method: "GET" });
}
