import "server-only";
import { authedRequest } from "@/shared/lib/backend";
import type { DiagnosticTest, GroupComparison, GroupDashboard, MyGroups, StudentGroup, SubmissionDetail } from "./types";

const BASE = "/api/classroom";

export function getMyGroups() {
  return authedRequest<MyGroups>(`${BASE}/groups`, { method: "GET" });
}

export function getDashboard(groupId: string) {
  return authedRequest<GroupDashboard>(`${BASE}/groups/${encodeURIComponent(groupId)}/dashboard`, { method: "GET" });
}

export function getStudentView(groupId: string) {
  return authedRequest<StudentGroup>(`${BASE}/groups/${encodeURIComponent(groupId)}/me`, { method: "GET" });
}

export function getDiagnosticTest(groupId: string, stage: string) {
  return authedRequest<DiagnosticTest>(`${BASE}/groups/${encodeURIComponent(groupId)}/diagnostics/${encodeURIComponent(stage)}`, { method: "GET" });
}

export function getComparison(groupId: string, otherGroupId: string) {
  return authedRequest<GroupComparison>(`${BASE}/groups/${encodeURIComponent(groupId)}/compare/${encodeURIComponent(otherGroupId)}`, { method: "GET" });
}

export function getSubmission(id: string) {
  return authedRequest<SubmissionDetail>(`${BASE}/submissions/${encodeURIComponent(id)}`, { method: "GET" });
}
