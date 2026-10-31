/** Backend DTO’lari bilan mos (Tasvir.Modules.Classroom.Application). */
import type { MaterialContent, TemplateKey } from "@/features/studio/lib/types";

export type GroupSummary = {
  id: string;
  name: string;
  teacherName: string;
  memberCount: number;
  createdAt: string;
  joinCode?: string | null;
};

export type MyGroups = { teaching: GroupSummary[]; studying: GroupSummary[] };

export type Review = {
  version: number;
  scores: Record<string, number>;
  totalScore: number;
  maxScore: number;
  comment?: string | null;
  reviewedAt: string;
};

export type SubmissionSummary = {
  id: string;
  userId: string;
  studentName: string;
  templateKey: TemplateKey;
  title: string;
  subject?: string | null;
  grade?: number | null;
  version: number;
  submittedAt: string;
  review?: Review | null;
};

export type RubricCriterion = { key: string; title: string; description: string };

export type SubmissionDetail = {
  summary: SubmissionSummary;
  groupId: string;
  groupName: string;
  canReview: boolean;
  content: MaterialContent;
  rubric: RubricCriterion[];
  maxCriterionScore: number;
};

export type CourseProgress = { title: string; completedLessons: number; totalLessons: number };

export type StudentRow = {
  userId: string;
  name: string;
  joinedAt: string;
  courses: CourseProgress[];
  firstTryCorrect: number;
  firstTryTotal: number;
  lastActivityAt?: string | null;
  submissionCount: number;
  reviewedCount: number;
  averageScorePercent?: number | null;
};

export type ItemOption = { text: string; isCorrect: boolean; firstTryCount: number };

export type ItemAnalysis = {
  courseTitle: string;
  lessonTitle: string;
  question: string;
  kind: "quiz" | "exercise";
  respondents: number;
  difficultyIndex: number;
  discriminationIndex?: number | null;
  averageAttempts: number;
  options: ItemOption[];
};

export type SurveyQuestionSummary = { key: string; text: string; average?: number | null; counts: number[] };
export type SurveySummary = { respondents: number; questions: SurveyQuestionSummary[]; comments: string[] };

export type GroupDashboard = {
  group: GroupSummary;
  students: StudentRow[];
  submissions: SubmissionSummary[];
  items: ItemAnalysis[];
  survey: SurveySummary;
};

export type SurveyQuestion = { key: string; text: string };

export type StudentGroup = {
  group: GroupSummary;
  joinedAt: string;
  submissions: SubmissionSummary[];
  surveyQuestions: SurveyQuestion[];
  surveyAnswers?: Record<string, number> | null;
  surveyComment?: string | null;
};
