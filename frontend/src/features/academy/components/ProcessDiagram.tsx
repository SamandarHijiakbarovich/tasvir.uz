/** Jarayon sxemasining jonli ko‘rinishi: bosqichlar strelkalar bilan ketma-ket chiziladi. */
export function ProcessDiagram({ items }: { items: string[] }) {
  const filled = items.map((i) => i.trim()).filter(Boolean);

  if (filled.length === 0) {
    return <p className="tv-diagram-empty">Bosqich nomlarini yozing, sxema shu yerda paydo bo‘ladi.</p>;
  }

  return (
    <ol className="tv-diagram" role="img" aria-label={`Jarayon sxemasi: ${filled.join(", keyin ")}`}>
      {filled.map((item, index) => (
        <li key={`${index}-${item}`} className={index === 0 ? "first" : undefined}>
          <span className="tv-diagram-num" aria-hidden="true">
            {index + 1}
          </span>
          {item}
        </li>
      ))}
    </ol>
  );
}
