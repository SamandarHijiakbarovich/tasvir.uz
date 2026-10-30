/** Backend DTO’lari bilan mos (Tasvir.Modules.Academy.Application). */
export type LessonStatus = "locked" | "available" | "inProgress" | "completed";

export type LessonSummary = {
  slug: string;
  title: string;
  summary: string;
  estimatedMinutes: number;
  status: LessonStatus;
  completedSteps: number;
  totalSteps: number;
};

export type Course = {
  slug: string;
  title: string;
  description: string;
  completedLessons: number;
  totalLessons: number;
  nextLessonSlug: string | null;
  hasStarted: boolean;
  lessons: LessonSummary[];
};

export type StepAnswer = {
  optionId?: string | null;
  text?: string | null;
  items?: string[] | null;
};

export type ReadingContent = {
  paragraphs: string[];
  callout?: string | null;
  example?: { beforeLabel: string; before: string; afterLabel: string; after: string[] } | null;
};

export type QuizContent = {
  question: string;
  options: { id: string; text: string }[];
};

export type PracticeContent = {
  mode: "text" | "process" | "checklist";
  prompt: string;
  placeholder?: string | null;
  minItems: number;
  maxItems: number;
  checklist?: string[] | null;
};

export type VideoContent = {
  youtubeId: string;
  sourceTitle: string;
  intro: string;
};

export type ExerciseContent = {
  prompt: string;
  unit?: string | null;
};

export type LessonStep = {
  id: string;
  kind: "reading" | "quiz" | "practice" | "video" | "exercise";
  title: string;
  isCompleted: boolean;
  reading?: ReadingContent | null;
  quiz?: QuizContent | null;
  practice?: PracticeContent | null;
  video?: VideoContent | null;
  exercise?: ExerciseContent | null;
  savedResponse?: StepAnswer | null;
};

export type Lesson = {
  slug: string;
  title: string;
  summary: string;
  estimatedMinutes: number;
  courseSlug: string;
  courseTitle: string;
  position: number;
  totalLessons: number;
  status: LessonStatus;
  nextLessonSlug: string | null;
  steps: LessonStep[];
};

export type MyWorkItem = {
  stepId: string;
  lessonSlug: string;
  lessonTitle: string;
  stepTitle: string;
  mode: "text" | "process";
  text?: string | null;
  items?: string[] | null;
  completedAt: string;
};

export type StepResult = {
  stepCompleted: boolean;
  isCorrect?: boolean | null;
  feedback?: string | null;
  lessonCompleted: boolean;
  nextLessonSlug?: string | null;
};
