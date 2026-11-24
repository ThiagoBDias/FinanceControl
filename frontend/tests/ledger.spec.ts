import { randomUUID } from "node:crypto";
import { readFileSync, writeFileSync } from "node:fs";
import { expect, test, type APIRequestContext } from "@playwright/test";
import type { Debt, Invoice, Transaction } from "../src/types";

const password = "Synthetic-only-42";
const date = "2026-02-18";
const testRunFile = process.env.FINANCECONTROL_PERSISTENCE_FILE;

async function createUser(request: APIRequestContext, email: string) {
  const response = await request.post("/api/auth/register", { data: { email, password, name: "Pessoa de teste" } });
  expect(response.status(), await response.text()).toBe(201);
  return (await response.json()).accessToken as string;
}

test("reconciles the PostgreSQL ledger and isolates users", async ({ request }) => {
  const email = `ledger-${randomUUID()}@example.test`;
  const token = await createUser(request, email);
  const other = await createUser(request, `other-${randomUUID()}@example.test`);
  const auth = { Authorization: `Bearer ${token}` };
  const outsider = { Authorization: `Bearer ${other}` };
  const anonymous = await request.get("/api/accounts");
  expect(anonymous.status()).toBe(401);

  async function get(path: string) {
    const response = await request.get(`/api${path}`, { headers: auth });
    expect(response.status(), await response.text()).toBe(200);
    return response.json();
  }

  async function post(path: string, data?: object, id = randomUUID(), status = 201) {
    const response = await request.post(`/api${path}`, { data, headers: { ...auth, "Idempotency-Key": id } });
    expect(response.status(), await response.text()).toBe(status);
    return response.json();
  }

  const source = await post("/accounts", { name: "Conta de origem", type: 1, initialBalance: 1000 });
  const target = await post("/accounts", { name: "Reserva", type: 2, initialBalance: 0 });
  const update = await request.put(`/api/accounts/${target.id}`, { headers: auth, data: { name: "Reserva alterada" } });
  expect(update.ok(), await update.text()).toBeTruthy();
  const category = await post("/categories", { name: "Mercado", type: 2 });
  await post("/budgets", { categoryId: category.id, year: 2026, month: 2, limitAmount: 150 }, randomUUID(), 200);
  await post("/transactions/income", { accountId: source.id, amount: 250, description: "Receita", competenceDate: date });
  await post("/transactions/expense", { accountId: source.id, amount: 100, description: "Mercado", competenceDate: date, categoryId: category.id });
  const bill = await post("/transactions/expense", {
    accountId: source.id, amount: 35, description: "Conta programada", competenceDate: date, dueDate: date, scheduled: true,
  });
  expect(bill.isPending).toBe(true);
  expect((await get(`/accounts/${source.id}`)).calculatedBalance).toBe(1150);
  expect((await get("/dashboard?year=2026&month=2")).upcoming).toEqual(
    expect.arrayContaining([expect.objectContaining({ name: "Conta programada", amount: 35 })]),
  );
  await post(`/transactions/${bill.id}/settle`, { paymentDate: date }, randomUUID(), 200);

  const transferBody = { sourceAccountId: source.id, destinationAccountId: target.id, transferDate: date, amount: 50 };
  const transferKey = randomUUID();
  const [transfer, replay] = await Promise.all([
    post("/transfers", transferBody, transferKey), post("/transfers", transferBody, transferKey),
  ]);
  expect(replay.id).toBe(transfer.id);
  await post("/transfers", { ...transferBody, amount: 55 }, transferKey, 409);

  const card = await post("/credit-cards", { name: "Cartão teste", creditLimit: 1000, closingDay: 20, dueDay: 5, initialBalance: 12.34 });
  const purchaseBody = { accountId: card.id, amount: "100.00", description: "Compra parcelada",
    competenceDate: date, categoryId: category.id, installmentCount: 3 };
  const purchaseKey = randomUUID();
  const purchase = await post("/transactions/expense", purchaseBody, purchaseKey);
  expect((await post("/transactions/expense", purchaseBody, purchaseKey)).id).toBe(purchase.id);
  await post("/transactions/expense", { ...purchaseBody, amount: "0.001" }, randomUUID(), 400);
  const installments: Transaction[] = await get(`/transactions?accountId=${card.id}`);
  installments.sort((a, b) => (a.installmentNumber ?? 0) - (b.installmentNumber ?? 0));
  expect(installments.map((item) => item.amount)).toEqual([33.34, 33.33, 33.33]);
  expect(installments.map((item) => item.competenceDate)).toEqual(["2026-02-18", "2026-03-18", "2026-04-18"]);
  const invoices: Invoice[] = await get(`/invoices?creditCardAccountId=${card.id}`);
  const firstInvoice = invoices.find((invoice) => invoice.id === purchase.invoiceId);
  const opening = invoices.find((invoice) => invoice.openingAmount > 0);
  if (!firstInvoice || !opening) throw new Error("Faturas esperadas não encontradas.");
  expect(firstInvoice.totalAmount).toBe(33.34);
  expect(firstInvoice.dueDate).toBe("2026-03-05");
  const pay = (invoiceId: string, amount: number, key = randomUUID(), status = 200) =>
    post(`/invoices/${invoiceId}/payments`, { accountId: source.id, amount, paymentDate: date }, key, status);

  await post(`/invoices/${firstInvoice.id}/close`, undefined, randomUUID(), 200);
  await post("/transactions/expense", { ...purchaseBody, installmentCount: 1 }, randomUUID(), 400);
  const payKey = randomUUID();
  const partial = await pay(firstInvoice.id, 10, payKey);
  expect(partial.pendingAmount).toBe(23.34);
  expect((await pay(firstInvoice.id, 10, payKey)).paidAmount).toBe(10);
  await pay(firstInvoice.id, 23.35, randomUUID(), 400);
  const concurrentPayments = await Promise.all([1, 2].map(() => request.post(`/api/invoices/${firstInvoice.id}/payments`, {
    headers: { ...auth, "Idempotency-Key": randomUUID() },
    data: { accountId: source.id, amount: 23.34, paymentDate: date },
  })));
  expect(concurrentPayments.map((result) => result.status()).sort()).toEqual([200, 400]);
  expect((await get(`/invoices/${firstInvoice.id}`)).pendingAmount).toBe(0);
  expect((await pay(opening.id, 12.34)).pendingAmount).toBe(0);

  const debt = await post("/debts", { name: "Financiamento", totalAmount: 100, installmentCount: 3, firstDueDate: date, categoryId: category.id });
  const paid = await post(`/debts/${debt.id}/installments/1/pay`, { accountId: source.id, paymentDate: date }, randomUUID(), 200);
  expect(paid.pendingAmount).toBe(66.66);
  await post(`/debts/${debt.id}/installments/1/pay`, { accountId: source.id, paymentDate: date }, randomUUID(), 400);

  const future = new Date();
  future.setUTCDate(future.getUTCDate() + 5);
  const dueSoon = future.toISOString().slice(0, 10);
  await post("/transactions/expense", {
    accountId: source.id, amount: "20.00", description: "Conta do próximo vencimento",
    competenceDate: dueSoon, dueDate: dueSoon, scheduled: true,
  });
  const summary = await get("/dashboard?year=2026&month=2");
  expect(summary.upcoming).toEqual(expect.arrayContaining([
    expect.objectContaining({ name: "Conta do próximo vencimento", amount: 20 }),
  ]));
  expect(summary.projectedCommitments).toEqual(expect.arrayContaining([
    expect.objectContaining({ year: future.getUTCFullYear(), month: future.getUTCMonth() + 1, amount: expect.any(Number) }),
  ]));
  const forecast = summary.projectedCommitments.find((item: { year: number; month: number }) =>
    item.year === future.getUTCFullYear() && item.month === future.getUTCMonth() + 1);
  expect(forecast.amount).toBeGreaterThanOrEqual(20);
  expect(summary.income).toBe(250);
  expect(summary.expenses).toBe(201.68);
  expect(summary.result).toBe(48.32);
  expect(summary.cashBalance).toBe(1035.98);
  expect(summary.netBalance).toBe(969.32);
  expect(summary.creditCards[0].availableAmount).toBe(933.34);
  expect(summary.budgets[0].spentAmount).toBe(166.68);
  expect(summary.budgets[0].isOverLimit).toBe(true);
  expect((await get(`/accounts/${source.id}`)).calculatedBalance).toBe(985.98);
  expect((await get(`/accounts/${target.id}`)).calculatedBalance).toBe(50);

  const foreignAccount = await request.get(`/api/accounts/${source.id}`, { headers: outsider });
  const foreignInvoice = await request.get(`/api/invoices/${firstInvoice.id}`, { headers: outsider });
  const foreignWrite = await request.post(`/api/debts/${debt.id}/installments/2/pay`, {
    headers: { ...outsider, "Idempotency-Key": randomUUID() }, data: { accountId: source.id, paymentDate: date },
  });
  expect(foreignAccount.status()).toBe(404);
  expect(foreignInvoice.status()).toBe(404);
  expect(foreignWrite.status()).toBe(404);
  expect(await (await request.get("/api/transactions", { headers: outsider })).json()).toEqual([]);

  if (testRunFile) writeFileSync(testRunFile, JSON.stringify({ email, sourceId: source.id, invoiceId: firstInvoice.id, debtId: debt.id }));
});

test("ledger survives a PostgreSQL and API restart", async ({ request }) => {
  test.skip(!testRunFile, "Use the restart procedure with FINANCECONTROL_PERSISTENCE_FILE.");
  const fixture = JSON.parse(readFileSync(testRunFile!, "utf8"));
  const response = await request.post("/api/auth/login", { data: { email: fixture.email, password } });
  expect(response.status(), await response.text()).toBe(200);
  const headers = { Authorization: `Bearer ${(await response.json()).accessToken}` };
  const source = await request.get(`/api/accounts/${fixture.sourceId}`, { headers });
  const invoice = await request.get(`/api/invoices/${fixture.invoiceId}`, { headers });
  const debts = await request.get("/api/debts", { headers });
  expect((await source.json()).calculatedBalance).toBe(985.98);
  expect((await invoice.json()).pendingAmount).toBe(0);
  const balances: Debt[] = await debts.json();
  expect(balances.find((debt) => debt.id === fixture.debtId)?.pendingAmount).toBe(66.66);
});
