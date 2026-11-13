import type { OrderStatus } from "./types";

const money = new Intl.NumberFormat("uz-UZ", { maximumFractionDigits: 0 });
const date = new Intl.DateTimeFormat("uz-UZ", { dateStyle: "long", timeZone: "Asia/Tashkent" });

export const formatMoney = (amount: number) => `${money.format(amount)} söm`;

/** Backend vaqtni UTC da, ba’zan "Z" belgisisiz yuboradi. */
export const formatDate = (value: string) => date.format(new Date(/Z$|[+-]\d\d:\d\d$/.test(value) ? value : `${value}Z`));

export const formatMonths = (months: number) => (months % 12 === 0 ? `${months / 12} yil` : `${months} oy`);

export const STATUS_LABEL: Record<OrderStatus, string> = {
  pending: "Kutilmoqda",
  paid: "Tölangan",
  cancelled: "Bekor qilingan",
  refunded: "Qaytarilgan",
};
