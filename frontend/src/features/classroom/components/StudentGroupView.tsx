import Link from "next/link";
import { getTemplate } from "@/features/studio/lib/templates";
import { leaveGroupAction } from "../actions";
import { formatDate } from "../lib/format";
import type { StudentGroup } from "../types";
import { ConfirmForm, SubmitWorkForm, SurveyForm } from "./forms";

type Props = {
  data: StudentGroup;
  userId: string;
  materials: { id: string; title: string }[];
};

export function StudentGroupView({ data, userId, materials }: Props) {
  const { group } = data;

  return (
    <>
      <section className="tv-card tv-panel" aria-labelledby="topshirish-t">
        <h2 className="tv-h3" id="topshirish-t">
          Iş topşiriş
        </h2>
        {materials.length === 0 ? (
          <p className="tv-muted" style={{ margin: 0 }}>
            Avval <Link href="/studiya/yangi">Studiyada material yarating</Link>, keyin uni şu yerdan topşirasiz.
          </p>
        ) : (
          <SubmitWorkForm groupId={group.id} materials={materials} />
        )}
      </section>

      <section className="tv-card tv-panel" aria-labelledby="ishlarim-t">
        <h2 className="tv-h3" id="ishlarim-t">
          Topşirgan işlarim
        </h2>
        {data.submissions.length === 0 ? (
          <p className="tv-muted" style={{ margin: 0 }}>
            Hali iş topşirmagansiz.
          </p>
        ) : (
          <ul className="tv-submission-list">
            {data.submissions.map((s) => {
              const isCurrent = s.review?.version === s.version;
              return (
                <li key={s.id}>
                  <div>
                    <Link href={`/guruhlar/ish/${s.id}`}>{s.title}</Link>
                    <p className="tv-material-meta">
                      {getTemplate(s.templateKey)?.name} · {formatDate(s.submittedAt)}
                      {s.version > 1 && ` · ${s.version}-versiya`}
                    </p>
                  </div>
                  {isCurrent ? (
                    <strong>
                      {s.review!.totalScore} / {s.review!.maxScore}
                    </strong>
                  ) : (
                    <span className="tv-chip-soon">{s.review ? "Yangi versiya kutilmoqda" : "Baholanmoqda"}</span>
                  )}
                </li>
              );
            })}
          </ul>
        )}
      </section>

      <section className="tv-card tv-panel" aria-labelledby="sorov-t">
        <div>
          <h2 className="tv-h3" id="sorov-t">
            Sörovnoma
          </h2>
          <p className="tv-muted" style={{ margin: "var(--space-2) 0 0" }}>
            Fikringiz platformani yaxşilaşga yordam beradi. Öqituvçi faqat umumiy natijani va ismsiz izohlarni köradi.
          </p>
        </div>
        <SurveyForm groupId={group.id} questions={data.surveyQuestions} answers={data.surveyAnswers} comment={data.surveyComment} />
      </section>

      <ConfirmForm
        action={leaveGroupAction.bind(null, group.id, userId)}
        message={`“${group.name}” guruhidan çiqasizmi? Şu guruhdagi işlaringiz va baholaringiz öçadi. Studiyadagi materiallaringiz qoladi.`}
        label="Guruhdan çiqiş"
      />
    </>
  );
}
