type Props = { label: string; value: string; icon: string; tone?: "green" | "red" | "purple" | "blue" };
export default function StatCard({ label, value, icon, tone = "purple" }: Props) {
  return <div className="stat-card"><div className={`stat-icon ${tone}`}>{icon}</div><div><span>{label}</span><strong>{value}</strong></div></div>;
}
