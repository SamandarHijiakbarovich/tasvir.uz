import Link from "next/link";
import { getTemplate } from "@/features/studio/lib/templates";
import { deleteGroupAction, removeMemberAction } from "../actions";
import { fixed, formatDate, percent } from "../lib/format";
import type { GroupDashboard, ItemAnalysis, StudentRow, SubmissionSummary, SurveySummary } from "../types";
import { DiagnosticsPanel, type CompareState } from "./DiagnosticsPanel";
import { ConfirmForm } from "./forms";

const TABS = [
  { key: "talabalar", label: "Talabalar" },
  { key: "ishlar", label: "Işlar" },
  { key: "test", label: "Test tahlili" },
  { key: "sorovnoma", label: "Sörovnoma" },
  { key: "diagnostika", label: "Diagnostika" },
] as const;

export type TabKey = (typeof TABS)[number]["key"];

export const toTab = (value?: string): TabKey => TABS.find((t) => t.key === value)?.key ?? "talabalar";

export function TeacherDashboard({ data, tab, compare }: { data: GroupDashboard; tab: TabKey; compare: CompareState }) {
  const { group } = data;
  const path = `/guruhlar/${group.id}`;
  const pending = data.submissions.filter((s) => s.review?.version !== s.version).length;

  return (
    <>
      <section className="tv-card tv-code-card" aria-label="Guruh kodi">
        <div>
          <p className="tv-muted" style={{ margin: 0 }}>
            Talabalarga şu kodni bering
          </p>
          <p className="tv-join-code">{group.joinCode}</p>
        </div>
        <p className="tv-field-hint" style={{ margin: 0, maxWidth: 340 }}>
          Talaba röyxatdan ötadi, “Guruhlar” sahifasida kodni kiritadi va natijalari sizga körinişiga rozilik beradi.
        </p>
      </section>

      <nav className="tv-filter" aria-label="Guruh bölimlari">
        {TABS.map((t) => (
          <Link key={t.key} href={`${path}?b=${t.key}`} aria-current={tab === t.key}>
            {t.label}
            {t.key === "ishlar" && pending > 0 && ` (${pending})`}
          </Link>
        ))}
      </nav>

      {tab === "talabalar" && <Students groupId={group.id} students={data.students} />}
      {tab === "ishlar" && <Submissions submissions={data.submissions} />}
      {tab === "test" && <Items groupId={group.id} items={data.items} />}
      {tab === "sorovnoma" && <Survey groupId={group.id} survey={data.survey} />}
      {tab === "diagnostika" && <DiagnosticsPanel groupId={group.id} data={data.diagnostics} compare={compare} />}

      <ConfirmForm
        action={deleteGroupAction.bind(null, group.id)}
        message={`“${group.name}” guruhi va undagi barça işlar, baholar va sörovnoma javoblari öçirilsinmi? Buni qaytarib bölmaydi.`}
        label="Guruhni öçiriş"
      />
    </>
  );
}

function ExportLink({ groupId, kind, label }: { groupId: string; kind: string; label: string }) {
  return (
    <a className="tv-btn tv-btn-ghost tv-btn-sm" href={`/guruhlar/${groupId}/eksport/${kind}`} download>
      {label}
    </a>
  );
}

function Students({ groupId, students }: { groupId: string; students: StudentRow[] }) {
  if (students.length === 0) {
    return (
      <section className="tv-card tv-empty">
        <h2 className="tv-h3">Hali talaba yöq</h2>
        <p className="tv-muted">Yuqoridagi kodni talabalarga bering. Qöşilganlar şu yerda körinadi.</p>
      </section>
    );
  }

  const courses = students[0].courses;

  return (
    <section className="tv-card tv-panel" aria-labelledby="talabalar-t">
      <div className="tv-panel-head">
        <h2 className="tv-h3" id="talabalar-t">
          Talabalar ({students.length})
        </h2>
        <ExportLink groupId={groupId} kind="natijalar" label="Excel uçun yuklab oliş" />
      </div>
      <div className="tv-table-wrap" tabIndex={0} role="region" aria-labelledby="talabalar-t">
        <table className="tv-table">
          <thead>
            <tr>
              <th scope="col">F.I.Ş.</th>
              {courses.map((c) => (
                <th scope="col" key={c.title}>
                  {c.title}
                </th>
              ))}
              <th scope="col">Testlar (1-uriniş)</th>
              <th scope="col">Işlar</th>
              <th scope="col">Örtaça baho</th>
              <th scope="col">Oxirgi faollik</th>
              <th scope="col">
                <span className="tv-sr">Amal</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {students.map((student) => (
              <tr key={student.userId}>
                <th scope="row">{student.name}</th>
                {student.courses.map((course) => (
                  <td key={course.title}>
                    <span className="tv-mini-progress" aria-hidden="true">
                      <i style={{ width: `${percent(course.completedLessons, course.totalLessons)}%` }} />
                    </span>
                    {course.completedLessons} / {course.totalLessons}
                  </td>
                ))}
                <td>{student.firstTryTotal === 0 ? "—" : `${percent(student.firstTryCorrect, student.firstTryTotal)}% (${student.firstTryCorrect}/${student.firstTryTotal})`}</td>
                <td>
                  {student.submissionCount}
                  {student.submissionCount > student.reviewedCount && <span className="tv-muted"> · {student.submissionCount - student.reviewedCount} baholanmagan</span>}
                </td>
                <td>{student.averageScorePercent == null ? "—" : `${Math.round(student.averageScorePercent)}%`}</td>
                <td>{student.lastActivityAt ? formatDate(student.lastActivityAt) : "—"}</td>
                <td>
                  <ConfirmForm
                    action={removeMemberAction.bind(null, groupId, student.userId)}
                    message={`${student.name} guruhdan çiqarilsinmi? Uning şu guruhdagi işlari va baholari öçadi.`}
                    label="Çiqariş"
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

function Submissions({ submissions }: { submissions: SubmissionSummary[] }) {
  if (submissions.length === 0) {
    return (
      <section className="tv-card tv-empty">
        <h2 className="tv-h3">Hali iş topşirilmagan</h2>
        <p className="tv-muted">Talabalar Studiyada yaratgan materiallarini guruh sahifasidan topşiradi.</p>
      </section>
    );
  }

  return (
    <section className="tv-card tv-panel" aria-labelledby="ishlar-t">
      <h2 className="tv-h3" id="ishlar-t">
        Topşirilgan işlar
      </h2>
      <div className="tv-table-wrap" tabIndex={0} role="region" aria-labelledby="ishlar-t">
        <table className="tv-table">
          <thead>
            <tr>
              <th scope="col">Talaba</th>
              <th scope="col">Iş</th>
              <th scope="col">Şakl</th>
              <th scope="col">Topşirilgan</th>
              <th scope="col">Baho</th>
            </tr>
          </thead>
          <tbody>
            {submissions.map((s) => {
              const isCurrent = s.review?.version === s.version;
              return (
                <tr key={s.id}>
                  <th scope="row">{s.studentName}</th>
                  <td>
                    <Link href={`/guruhlar/ish/${s.id}`}>{s.title}</Link>
                    {s.version > 1 && <span className="tv-muted"> · {s.version}-versiya</span>}
                  </td>
                  <td>{getTemplate(s.templateKey)?.name ?? s.templateKey}</td>
                  <td>{formatDate(s.submittedAt)}</td>
                  <td>{isCurrent ? `${s.review!.totalScore} / ${s.review!.maxScore}` : <span className="tv-chip-soon">Baholanmagan</span>}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </section>
  );
}

// Test nazariyasidagi odatiy chegaralar
const HARD = 0.3;
const EASY = 0.9;
const WEAK_DISCRIMINATION = 0.2;

function Items({ groupId, items }: { groupId: string; items: ItemAnalysis[] }) {
  if (items.length === 0) {
    return (
      <section className="tv-card tv-empty">
        <h2 className="tv-h3">Hali ma’lumot yöq</h2>
        <p className="tv-muted">Talabalar darslardagi test va maşqlarni bajargaç, har bir savol tahlili şu yerda çiqadi.</p>
      </section>
    );
  }

  return (
    <section className="tv-card tv-panel" aria-labelledby="test-t">
      <div className="tv-panel-head">
        <h2 className="tv-h3" id="test-t">
          Test savollari tahlili
        </h2>
        <ExportLink groupId={groupId} kind="test-tahlili" label="Excel uçun yuklab oliş" />
      </div>
      <p className="tv-field-hint" style={{ margin: 0 }}>
        Hisob birinçi urinişlarga asoslanadi. Qiyinlik: töğri javob berganlar uluşi. Farqlaş: kuçli va zaif talabalar orasidagi farq; 4 tadan kam javobda hisoblanmaydi.
        Guruh kiçik bölsa, körsatkiçlarni taxminiy deb qarang.
      </p>
      <ul className="tv-item-list">
        {items.map((item) => (
          <li key={`${item.lessonTitle}-${item.question}`} className="tv-item">
            <p className="tv-material-meta">
              {item.courseTitle} · {item.lessonTitle}
            </p>
            <h3 className="tv-item-q">{item.question}</h3>
            <p className="tv-item-stats">
              <span>Javob berganlar: {item.respondents}</span>
              <span>Qiyinlik: {fixed(item.difficultyIndex)}</span>
              <span>Farqlaş: {item.discriminationIndex == null ? "—" : fixed(item.discriminationIndex)}</span>
              <span>Örtaça uriniş: {fixed(item.averageAttempts, 1)}</span>
              {item.difficultyIndex < HARD && <span className="tv-chip-warn">Juda qiyin</span>}
              {item.difficultyIndex > EASY && <span className="tv-chip-soon">Juda oson</span>}
              {item.discriminationIndex != null && item.discriminationIndex < WEAK_DISCRIMINATION && <span className="tv-chip-warn">Zaif farqlaydi</span>}
            </p>
            {item.options.length > 0 && (
              <ul className="tv-option-bars">
                {item.options.map((option) => (
                  <li key={option.text}>
                    <span>
                      {option.isCorrect && <strong>Töğri: </strong>}
                      {option.text}
                    </span>
                    <span className="tv-bar" aria-hidden="true">
                      <i className={option.isCorrect ? "ok" : undefined} style={{ width: `${percent(option.firstTryCount, item.respondents)}%` }} />
                    </span>
                    <span>{option.firstTryCount}</span>
                  </li>
                ))}
              </ul>
            )}
          </li>
        ))}
      </ul>
    </section>
  );
}

function Survey({ groupId, survey }: { groupId: string; survey: SurveySummary }) {
  if (survey.respondents === 0) {
    return (
      <section className="tv-card tv-empty">
        <h2 className="tv-h3">Hali javob yöq</h2>
        <p className="tv-muted">Talabalar guruh sahifasida sörovnomani töldiradi. Bu yerda faqat umumiy natija va ismsiz izohlar körinadi.</p>
      </section>
    );
  }

  return (
    <section className="tv-card tv-panel" aria-labelledby="sorov-t">
      <div className="tv-panel-head">
        <h2 className="tv-h3" id="sorov-t">
          Sörovnoma natijalari ({survey.respondents} ta javob)
        </h2>
        <ExportLink groupId={groupId} kind="sorovnoma" label="Excel uçun yuklab oliş" />
      </div>
      <ul className="tv-option-bars tv-survey-bars">
        {survey.questions.map((question) => (
          <li key={question.key}>
            <span>{question.text}</span>
            <span className="tv-bar" aria-hidden="true">
              <i className="ok" style={{ width: `${((question.average ?? 0) / 5) * 100}%` }} />
            </span>
            <span>{question.average == null ? "—" : `${fixed(question.average, 1)} / 5`}</span>
          </li>
        ))}
      </ul>
      {survey.comments.length > 0 && (
        <>
          <h3 className="tv-item-q">Izohlar</h3>
          <ul className="tv-comments">
            {survey.comments.map((comment, index) => (
              <li key={index}>{comment}</li>
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
