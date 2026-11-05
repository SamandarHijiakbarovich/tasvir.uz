// Backenddagi Tasvir.Modules.Content.Application/Tools/ToolCatalog.cs bilan mos.

export type ToolCatalog = {
  checkedAt: string;
  categories: ToolCategory[];
  shapes: Shape[];
  subjects: Subject[];
  tools: VisualTool[];
};

export type ToolCategory = { key: string; title: string; description: string };

export type Shape = { key: string; name: string; useFor: string; inStudio: boolean };

export type Subject = { key: string; name: string; purpose: string };

export type VisualTool = {
  key: string;
  name: string;
  url: string;
  category: string;
  summary: string;
  classroomUse: string;
  access: "free" | "freemium" | "paid";
  accessNote: string;
  level: "easy" | "medium" | "coding";
  shapes: string[];
  /** Bo‘sh bo‘lsa, vosita har qanday fanda ishlatiladi. */
  subjects: string[];
};
