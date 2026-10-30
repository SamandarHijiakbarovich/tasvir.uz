import "server-only";
import { cache } from "react";
import { authedRequest, backendRequest, type ApiFailure, type ApiResult } from "@/shared/lib/backend";
import type { AuthUser } from "./session-cookies";

export type { ApiFailure, ApiResult };

type RequestOptions = Parameters<typeof backendRequest>[1];

export function identityRequest<T>(path: string, options?: RequestOptions) {
  return backendRequest<T>(`/api/identity/${path}`, options);
}

/** Joriy foydalanuvchi; bitta so‘rov davomida backend’ga bir marta murojaat qiladi. */
export const getCurrentUser = cache(async (): Promise<AuthUser | null> => {
  const result = await authedRequest<AuthUser>("/api/identity/me", { method: "GET" });
  return result.ok ? result.data : null;
});
