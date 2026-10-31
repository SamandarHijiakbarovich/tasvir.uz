const dateFormat = new Intl.DateTimeFormat("uz-UZ", { dateStyle: "medium", timeZone: "Asia/Tashkent" });

/** Backend vaqtni UTC da yuboradi, "Z" siz bo‘lishi mumkin. */
export const formatDate = (value: string) => dateFormat.format(new Date(/(Z|[+-]\d\d:\d\d)$/.test(value) ? value : `${value}Z`));

export const percent = (part: number, whole: number) => Math.round((100 * part) / whole);

export const fixed = (value: number, digits = 2) => value.toFixed(digits).replace(".", ",");
