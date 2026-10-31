import type { Metadata } from "next";
import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { getCurrentUser } from "@/features/auth/lib/identity-api";
import { getDashboard, getStudentView } from "@/features/classroom/api";
import { ClassroomHeader } from "@/features/classroom/components/ClassroomHeader";
import { StudentGroupView } from "@/features/classroom/components/StudentGroupView";
import { TeacherDashboard, toTab } from "@/features/classroom/components/TeacherDashboard";
import { listMaterials } from "@/features/studio/api/studio-api";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/studio.css";
import "@/styles/classroom.css";

export const metadata: Metadata = { title: "Guruh — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { params: Promise<{ id: string }>; searchParams: Promise<{ b?: string; xato?: string }> };

export default async function GroupPage({ params, searchParams }: Props) {
  const { id } = await params;
  const { b, xato } = await searchParams;
  const login = `/kirish?keyin=${encodeURIComponent(`/guruhlar/${id}`)}`;

  // O‘qituvchi bo‘lmasa, backend 404 qaytaradi: unda talaba ko‘rinishi sinab ko‘riladi
  const dashboard = await getDashboard(id);
  if (!dashboard.ok && dashboard.status === 401) redirect(login);

  const student = dashboard.ok ? null : await getStudentView(id);
  if (student && !student.ok) {
    if (student.status === 401) redirect(login);
    if (student.status === 404) notFound();
  }

  const group = dashboard.ok ? dashboard.data.group : student?.ok ? student.data.group : null;

  return (
    <div className="tv-studio">
      <ClassroomHeader />
      <main className="tv-studio-in">
        <p className="tv-breadcrumb" style={{ margin: 0 }}>
          <Link href="/guruhlar">Guruhlar</Link>
        </p>

        {xato && (
          <p className="tv-form-msg err" role="alert">
            {xato}
          </p>
        )}

        {!group && (
          <p className="tv-form-msg err" role="alert">
            {!dashboard.ok && dashboard.status !== 404 ? dashboard.message : student && !student.ok ? student.message : ""}
          </p>
        )}

        {group && (
          <div>
            <h1 className="tv-h2">{group.name}</h1>
            <p className="tv-muted" style={{ margin: "var(--space-2) 0 0" }}>
              {dashboard.ok ? `${group.memberCount} ta talaba` : `O‘qituvchi: ${group.teacherName}`}
            </p>
          </div>
        )}

        {dashboard.ok && <TeacherDashboard data={dashboard.data} tab={toTab(b)} />}
        {student?.ok && <StudentSection id={id} />}
      </main>
    </div>
  );
}

async function StudentSection({ id }: { id: string }) {
  const [view, user, materials] = await Promise.all([getStudentView(id), getCurrentUser(), listMaterials()]);
  if (!view.ok || !user) notFound();

  return <StudentGroupView data={view.data} userId={user.id} materials={materials.ok ? materials.data.map((m) => ({ id: m.id, title: m.title })) : []} />;
}
