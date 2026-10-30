import "@/styles/landing.css";
import { RevealObserver } from "@/shared/ui/RevealObserver";
import { getLandingContent } from "./api/getLandingContent";
import { Audience } from "./components/Audience";
import { BeforeAfter } from "./components/BeforeAfter";
import { Faq } from "./components/Faq";
import { Features } from "./components/Features";
import { FinalCta } from "./components/FinalCta";
import { Footer } from "./components/Footer";
import { Hero } from "./components/Hero";
import { HowItWorks } from "./components/HowItWorks";
import { Navbar } from "./components/Navbar";
import { Pricing } from "./components/Pricing";
import { Problem } from "./components/Problem";
import { Research } from "./components/Research";
import { Results } from "./components/Results";
import { TemplateGallery } from "./components/TemplateGallery";
import { Testimonials } from "./components/Testimonials";
import { Trust } from "./components/Trust";

/**
 * Landing sahifa. Statik bloklar shu yerda, dinamik ma’lumotlar (narxlar, FAQ, sharhlar, natijalar, hamkorlar)
 * API Gateway orqali Content modulidan keladi. Backend ishlamasa, dinamik bloklar yashiriladi.
 */
export async function LandingPage() {
  const content = await getLandingContent();

  return (
    <>
      <a className="tv-sr" href="#asosiy">
        Asosiy kontentga o‘tish
      </a>
      <Navbar />
      <main id="asosiy">
        <Hero />
        <Trust partners={content?.partners ?? []} />
        <Problem />
        <HowItWorks />
        <BeforeAfter />
        <Features />
        <TemplateGallery />
        {content && content.results.criteria.length > 0 && <Results results={content.results} />}
        <Audience />
        {content && content.pricingPlans.length > 0 && <Pricing plans={content.pricingPlans} />}
        {content && content.testimonials.length > 0 && <Testimonials items={content.testimonials} />}
        <Research />
        {content && content.faq.length > 0 && <Faq items={content.faq} />}
        <FinalCta />
      </main>
      <Footer />
      <RevealObserver />
    </>
  );
}
