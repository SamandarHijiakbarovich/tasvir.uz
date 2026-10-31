import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { getMyGroups } from "@/features/classroom/api";
import { ClassroomHeader } from "@/features/classroom/components/ClassroomHeader";
import { CreateGroupForm, JoinGroupForm } from "@/features/classroom/components/forms";
import type { GroupSummary } from "@/features/classroom/types";
import "@/styles/auth.css";
import "@/styles/academy.css";
import "@/styles/studio.css";
import "@/styles/classroom.css";

export const metadata: Metadata = { title: "Guruhlar — Tasvir.uz", robots: { index: false, follow: false } };

type Props = { searchParams: Promise<{ kod?: string }> };

function GroupList({ groups, role }: { groups: GroupSummary[]; role: "teacher" | "student" }) {
  return (
    <ul className="tv-material-grid">
      {groups.map((group) => (
        <li key={group.id} className="tv-card tv-material-card">
          <h3 className="tv-h3">
            <Link href={`/guruhlar/${group.id}`}>{group.name}</Link>
          </h3>
          <p className="tv-material-meta">{role === "teacher" ? `${group.memberCount} ta talaba · kod: ${group.joinCode}` : `O‘qituvchi: ${group.teacherName}`}</p>
        </li>
      ))}
    </ul>
  );
}

export default async function GroupsPage({ searchParams }: Props) {
  const { kod } = await searchParams;
  const result = await getMyGroups();

  if (!result.ok && result.status === 401) redirect("/kirish?keyin=/guruhlar");

  const teaching = result.ok ? result.data.teaching : [];
  const studying = result.ok ? result.data.studying : [];

  return (
    <div className="tv-studio">
      <ClassroomHeader />
      <main className="tv-studio-in">
        <div>
          <h1 className="tv-h2">Guruhlar</h1>
          <p className="tv-muted" style={{ margin: "var(--space-2) 0 0" }}>
            Guruh yaratsangiz, uning o‘qituvchisi bo‘lasiz. Kod bilan qo‘shilsangiz, talabasi.
          </p>
        </div>

        {!result.ok && (
          <p className="tv-form-msg err" role="alert">
            {result.message}
          </p>
        )}

        {teaching.length > 0 && (
          <section aria-labelledby="oqitaman-t" className="tv-group-section">
            <h2 className="tv-h3" id="oqitaman-t">
              O‘qitayotgan guruhlarim
            </h2>
            <GroupList groups={teaching} role="teacher" />
          </section>
        )}

        {studying.length > 0 && (
          <section aria-labelledby="oqiyman-t" className="tv-group-section">
            <h2 className="tv-h3" id="oqiyman-t">
              O‘qiyotgan guruhlarim
            </h2>
            <GroupList groups={studying} role="student" />
          </section>
        )}

        <div className="tv-cabinet-grid">
          <section className="tv-card" aria-labelledby="qoshilish-t">
            <h2 className="tv-h3" id="qoshilish-t">
              Guruhga qo‘shilish
            </h2>
            <JoinGroupForm code={kod} />
          </section>
          <section className="tv-card" aria-labelledby="yaratish-t">
            <h2 className="tv-h3" id="yaratish-t">
              Yangi guruh yaratish
            </h2>
            <CreateGroupForm />
          </section>
        </div>
      </main>
    </div>
  );
}
