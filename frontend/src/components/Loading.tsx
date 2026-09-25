export default function Loading({ text = "Carregando..." }: { text?: string }) {
  return <div className="loading"><span className="spinner" />{text}</div>;
}
