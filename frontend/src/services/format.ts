const brazil = "America/Sao_Paulo";
const calendarDate = new Intl.DateTimeFormat("en-CA", {
  timeZone: brazil, year: "numeric", month: "2-digit", day: "2-digit",
});
const currency = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

export function today() {
  const parts = calendarDate.formatToParts(new Date());
  return ["year", "month", "day"].map((key) => parts.find(({ type }) => type === key)?.value).join("-");
}
export const money = (amount: number) => currency.format(amount);
export const dateLabel = (value: string) => value.slice(0, 10).split("-").reverse().join("/");
export const isCard = (type: string | number) => String(type) === "4" || type === "CreditCard";
