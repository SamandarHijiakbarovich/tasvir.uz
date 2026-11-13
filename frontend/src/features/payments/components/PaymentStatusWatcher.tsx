"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

const INTERVAL_MS = 3000;
const MAX_TRIES = 40;

/** To‘lov tizimi tasdiqni biroz kechikib yuborishi mumkin: holat o‘zgarguncha sahifani 2 daqiqagacha yangilab turadi. */
export function PaymentStatusWatcher() {
  const router = useRouter();
  const [tries, setTries] = useState(0);

  useEffect(() => {
    if (tries >= MAX_TRIES) return;
    const timer = setTimeout(() => {
      router.refresh();
      setTries((t) => t + 1);
    }, INTERVAL_MS);
    return () => clearTimeout(timer);
  }, [router, tries]);

  return tries >= MAX_TRIES ? (
    <p className="tv-muted" role="status">
      Tasdiq hali kelmadi. Bir necça daqiqadan söng sahifani yangilang — tölov ötgan bölsa, obuna avtomatik faollaşadi.
    </p>
  ) : (
    <p className="tv-muted tv-pay-wait" role="status">
      <span className="tv-pay-spinner" aria-hidden="true" /> Tölov tizimidan tasdiq kutilmoqda…
    </p>
  );
}
