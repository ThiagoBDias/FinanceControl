export default function EmptyState({ title, description }: { title: string; description: string }) {
  return <div className="empty-state"><div className="empty-icon">○</div><h3>{title}</h3><p>{description}</p></div>;
}
