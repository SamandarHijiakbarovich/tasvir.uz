// Backend shartnomasi: Tasvir.Modules.Payments.Application/PaymentService.cs bilan mos bo‘lishi kerak.

export type PaymentProviderKey = "payme" | "click";
export type OrderStatus = "pending" | "paid" | "cancelled" | "refunded";

export type Plan = {
  key: string;
  name: string;
  description: string | null;
  months: number;
  price: number | null;
  features: string[];
  isAvailable: boolean;
};

export type Provider = { key: PaymentProviderKey; name: string; isAvailable: boolean };

export type Catalog = { plans: Plan[]; providers: Provider[] };

export type Order = {
  id: string;
  planKey: string;
  planName: string;
  months: number;
  amount: number;
  provider: PaymentProviderKey;
  status: OrderStatus;
  createdAt: string;
  paidAt: string | null;
};

export type CreatedOrder = { order: Order; paymentUrl: string };

export type SubscriptionInfo = { isActive: boolean; planName: string | null; validUntil: string | null };

export type MyPayments = { subscription: SubscriptionInfo; orders: Order[] };
