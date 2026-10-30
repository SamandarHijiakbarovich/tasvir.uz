/** Backend DTO’lari bilan mos (Tasvir.Modules.Studio.Application). */
export type TemplateKey = "process" | "timeline" | "mindmap" | "comparison" | "infographic";
export type Accent = "sun" | "mint" | "coral" | "brand";

export type MaterialItem = {
  heading: string;
  text?: string | null;
  cells?: string[] | null;
};

export type MaterialContent = {
  accent?: Accent | null;
  note?: string | null;
  columns?: string[] | null;
  items: MaterialItem[];
};

export type Material = {
  id: string;
  templateKey: TemplateKey;
  title: string;
  subject?: string | null;
  grade?: number | null;
  content: MaterialContent;
  createdAt: string;
  updatedAt: string;
};

export type MaterialDraft = {
  templateKey: TemplateKey;
  title: string;
  subject?: string | null;
  grade?: number | null;
  content: MaterialContent;
};
