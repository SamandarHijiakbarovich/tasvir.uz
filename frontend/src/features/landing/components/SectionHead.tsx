type Props = { id: string; label: string; title: string; lead?: string };

export function SectionHead({ id, label, title, lead }: Props) {
  return (
    <div className="tv-head tv-reveal">
      <span className="tv-label">{label}</span>
      <h2 className="tv-h2" id={id}>
        {title}
      </h2>
      {lead && <p className="tv-lead">{lead}</p>}
    </div>
  );
}
