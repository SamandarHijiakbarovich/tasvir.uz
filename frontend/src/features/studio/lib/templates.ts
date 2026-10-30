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
  "Matematika", "Fizika", "Kimyo", "Biologiya", "Geografiya", "Tarix", "Ona tili", "Adabiyot", "Chet tili", "Informatika", "Boshlang‘ich ta’lim",
];

export const TEMPLATES: TemplateDefinition[] = [
  {
    key: "process",
    name: "Blok-sxema",
    relation: "Ketma-ketlik",
    whenToUse: "Jarayon, algoritm yoki bosqichlarni tushuntirishda.",
    minItems: 2,
    maxItems: 8,
    itemName: "Bosqich",
    headingLabel: "Bosqich nomi",
    textLabel: "Qisqa izoh",
    noteLabel: "Pastki izoh",
    starter: {
      title: "Suvning tabiatda aylanishi",
      subject: "Geografiya",
      content: {
        accent: "sun",
        note: null,
        items: [
          { heading: "Bug‘lanish", text: "Quyosh issig‘ida suv bug‘ga aylanadi" },
          { heading: "Kondensatsiya", text: "Bug‘ soviydi va bulut hosil qiladi" },
          { heading: "Yog‘in", text: "Bulutdan yomg‘ir yoki qor yog‘adi" },
          { heading: "Oqim", text: "Suv daryolar orqali dengizga qaytadi" },
        ],
      },
    },
  },
  {
    key: "timeline",
    name: "Vaqt chizig‘i",
    relation: "Xronologiya",
    whenToUse: "Tarixiy voqealar yoki sanalar tartibini ko‘rsatishda.",
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
          { heading: "1336", text: "Keshda (hozirgi Shahrisabz) tug‘ildi" },
          { heading: "1370", text: "Movarounnahrda hokimiyatni qo‘lga oldi" },
          { heading: "1405", text: "Xitoyga yurish vaqtida O‘trorda vafot etdi" },
        ],
      },
    },
  },
  {
    key: "mindmap",
    name: "Aqliy xarita",
    relation: "Ierarxiya va tasnif",
    whenToUse: "Mavzuni umumlashtirish yoki g‘oyalarni yig‘ishda.",
    minItems: 2,
    maxItems: 8,
    itemName: "Tarmoq",
    headingLabel: "Tarmoq nomi",
    textLabel: "Tarkibi",
    noteLabel: "Pastki izoh",
    starter: {
      title: "So‘z turkumlari",
      subject: "Ona tili",
      content: {
        accent: "mint",
        note: null,
        items: [
          { heading: "Mustaqil", text: "Ot, sifat, son, olmosh, fe’l, ravish" },
          { heading: "Yordamchi", text: "Ko‘makchi, bog‘lovchi, yuklama" },
          { heading: "Alohida guruh", text: "Modal, undov, taqlid so‘zlar" },
        ],
      },
    },
  },
  {
    key: "comparison",
    name: "Qiyosiy jadval",
    relation: "Taqqoslash",
    whenToUse: "Ikki-to‘rtta tushunchani bir xil mezonlar bo‘yicha solishtirishda.",
    minItems: 1,
    maxItems: 8,
    itemName: "Mezon",
    headingLabel: "Mezon",
    noteLabel: "Pastki izoh",
    starter: {
      title: "Fotosintez va nafas olish",
      subject: "Biologiya",
      content: {
        accent: "brand",
        note: null,
        columns: ["Fotosintez", "Nafas olish"],
        items: [
          { heading: "Qayerda", cells: ["Xloroplastda", "Mitoxondriyada"] },
          { heading: "Nima kerak", cells: ["Yorug‘lik, CO₂, suv", "Glyukoza, kislorod"] },
          { heading: "Nima hosil bo‘ladi", cells: ["Glyukoza, kislorod", "CO₂, suv, energiya"] },
        ],
      },
    },
  },
  {
    key: "infographic",
    name: "Infografika",
    relation: "Bitta asosiy g‘oya",
    whenToUse: "Bir g‘oyani bir nechta qisqa blok bilan yetkazishda.",
    minItems: 2,
    maxItems: 6,
    itemName: "Blok",
    headingLabel: "Blok sarlavhasi",
    textLabel: "Qisqa matn",
    noteLabel: "Xulosa yoki manba",
    starter: {
      title: "Nega suv ichish muhim?",
      subject: "Biologiya",
      content: {
        accent: "mint",
        note: "Bu namuna. Matnni o‘z mavzuingizga almashtiring va manbani ko‘rsating.",
        items: [
          { heading: "Haroratni boshqaradi", text: "Terlash orqali tana soviydi" },
          { heading: "Moddalarni tashiydi", text: "Qon orqali ozuqa hujayralarga yetadi" },
          { heading: "Hazmga yordam beradi", text: "Ovqat oson parchalanadi" },
          { heading: "Diqqatni saqlaydi", text: "Suv yetishmasa, tez charchaymiz" },
        ],
      },
    },
  },
];

export const getTemplate = (key: string) => TEMPLATES.find((t) => t.key === key);

export const ACCENTS = [
  { key: "sun", label: "Sariq", fill: "#ffc93c", ink: "#17162b" },
  { key: "mint", label: "Yashil", fill: "#2ec4a0", ink: "#17162b" },
  { key: "coral", label: "Marjon", fill: "#ff6b57", ink: "#17162b" },
  { key: "brand", label: "Binafsha", fill: "#3d35c6", ink: "#ffffff" },
] as const;
