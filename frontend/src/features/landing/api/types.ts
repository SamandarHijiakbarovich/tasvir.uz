// Backenddagi Tasvir.Modules.Content.Application/Landing/LandingContentDto.cs bilan mos.

export type LandingContent = {
  partners: Partner[];
  results: Results;
  pricingPlans: PricingPlan[];
  testimonials: Testimonial[];
  faq: FaqItem[];
};

export type Partner = { name: string; logoUrl: string | null };

export type Results = { criteria: ResultCriterion[]; stats: ResultStat[] };

export type ResultCriterion = { label: string; experimentValue: number | null; controlValue: number | null };

export type ResultStat = { label: string; value: number | null; suffix: string | null };

export type PricingPlan = {
  id: string;
  name: string;
  description: string;
  price: number | null;
  priceUnit: string;
  features: string[];
  isFeatured: boolean;
  ctaText: string;
  ctaHref: string;
};

export type Testimonial = {
  quote: string;
  authorName: string;
  authorRole: string;
  avatarUrl: string | null;
  isVerified: boolean;
};

export type FaqItem = { id: string; question: string; answer: string };
