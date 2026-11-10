import type { MaterialDraft, TemplateKey } from "./types";

export type TemplateDefinition = {
  key: TemplateKey;
  name: string;
  /** Mazmundagi bog‘liqlik turi: shablon shu asosda tanlanadi. */
  relation: string;
  whenToUse: string;
  minItems: number;
  maxItems: number;
  itemName: string;
  headingLabel: string;
  textLabel?: string;
  noteLabel: string;
  starter: Omit<MaterialDraft, "templateKey">;
};

export const SUBJECTS = [
  "Matematika", "Fizika", "Kimyo", "Biologiya", "Geografiya", "Tarix", "Ona tili", "Adabiyot", "Çet tili", "Informatika", "Boşlanğiç ta’lim",
];

export const TEMPLATES: TemplateDefinition[] = [
  {
    key: "process",
    name: "Blok-sxema",
    relation: "Ketma-ketlik",
    whenToUse: "Jarayon, algoritm yoki bosqiçlarni tuşuntirişda.",
    minItems: 2,
    maxItems: 8,
    itemName: "Bosqiç",
    headingLabel: "Bosqiç nomi",
    textLabel: "Qisqa izoh",
    noteLabel: "Pastki izoh",
    starter: {
      title: "Suvning tabiatda aylanişi",
      subject: "Geografiya",
      content: {
        accent: "sun",
        note: null,
        items: [
          { heading: "Buğlaniş", text: "Quyoş issiğida suv buğga aylanadi" },
          { heading: "Kondensatsiya", text: "Buğ soviydi va bulut hosil qiladi" },
          { heading: "Yoğin", text: "Bulutdan yomğir yoki qor yoğadi" },
          { heading: "Oqim", text: "Suv daryolar orqali dengizga qaytadi" },
        ],
      },
    },
  },
  {
    key: "timeline",
    name: "Vaqt çiziği",
    relation: "Xronologiya",
    whenToUse: "Tarixiy voqealar yoki sanalar tartibini körsatişda.",
    minItems: 2,
    maxItems: 8,
    itemName: "Voqea",
    headingLabel: "Sana",
    textLabel: "Voqea",
    noteLabel: "Pastki izoh",
    starter: {
      title: "Amir Temur hayotidan sanalar",
      subject: "Tarix",
      content: {
        accent: "coral",
        note: null,
        items: [
          { heading: "1336", text: "Keşda (hozirgi Şahrisabz) tuğildi" },
          { heading: "1370", text: "Movarounnahrda hokimiyatni qölga oldi" },
          { heading: "1405", text: "Xitoyga yuriş vaqtida Ötrorda vafot etdi" },
        ],
      },
    },
  },
  {
    key: "mindmap",
    name: "Aqliy xarita",
    relation: "Ierarxiya va tasnif",
    whenToUse: "Mavzuni umumlaştiriş yoki ğoyalarni yiğişda.",
    minItems: 2,
    maxItems: 8,
    itemName: "Tarmoq",
    headingLabel: "Tarmoq nomi",
    textLabel: "Tarkibi",
    noteLabel: "Pastki izoh",
    starter: {
      title: "Söz turkumlari",
      subject: "Ona tili",
      content: {
        accent: "mint",
        note: null,
        items: [
          { heading: "Mustaqil", text: "Ot, sifat, son, olmoş, fe’l, raviş" },
          { heading: "Yordamçi", text: "Kömakçi, boğlovçi, yuklama" },
          { heading: "Alohida guruh", text: "Modal, undov, taqlid sözlar" },
        ],
      },
    },
  },
  {
    key: "comparison",
    name: "Qiyosiy jadval",
    relation: "Taqqoslaş",
    whenToUse: "Ikki-törtta tuşunçani bir xil mezonlar böyiça soliştirişda.",
    minItems: 1,
    maxItems: 8,
    itemName: "Mezon",
    headingLabel: "Mezon",
    noteLabel: "Pastki izoh",
    starter: {
      title: "Fotosintez va nafas oliş",
      subject: "Biologiya",
      content: {
        accent: "brand",
        note: null,
        columns: ["Fotosintez", "Nafas oliş"],
        items: [
          { heading: "Qayerda", cells: ["Xloroplastda", "Mitoxondriyada"] },
          { heading: "Nima kerak", cells: ["Yoruğlik, CO₂, suv", "Glyukoza, kislorod"] },
          { heading: "Nima hosil böladi", cells: ["Glyukoza, kislorod", "CO₂, suv, energiya"] },
        ],
      },
    },
  },
  {
    key: "infographic",
    name: "Infografika",
    relation: "Bitta asosiy ğoya",
    whenToUse: "Bir ğoyani bir neçta qisqa blok bilan yetkazişda.",
    minItems: 2,
    maxItems: 6,
    itemName: "Blok",
    headingLabel: "Blok sarlavhasi",
    textLabel: "Qisqa matn",
    noteLabel: "Xulosa yoki manba",
    starter: {
      title: "Nega suv içiş muhim?",
      subject: "Biologiya",
      content: {
        accent: "mint",
        note: "Bu namuna. Matnni öz mavzuingizga almaştiring va manbani körsating.",
        items: [
          { heading: "Haroratni boşqaradi", text: "Terlaş orqali tana soviydi" },
          { heading: "Moddalarni taşiydi", text: "Qon orqali ozuqa hujayralarga yetadi" },
          { heading: "Hazmga yordam beradi", text: "Ovqat oson parçalanadi" },
          { heading: "Diqqatni saqlaydi", text: "Suv yetişmasa, tez çarçaymiz" },
        ],
      },
    },
  },
];

export const getTemplate = (key: string) => TEMPLATES.find((t) => t.key === key);

export const ACCENTS = [
  { key: "sun", label: "Sariq", fill: "#ffc93c", ink: "#17162b" },
  { key: "mint", label: "Yaşil", fill: "#2ec4a0", ink: "#17162b" },
  { key: "coral", label: "Marjon", fill: "#ff6b57", ink: "#17162b" },
  { key: "brand", label: "Binafşa", fill: "#3d35c6", ink: "#ffffff" },
] as const;
