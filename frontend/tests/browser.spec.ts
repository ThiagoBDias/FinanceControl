import { randomUUID } from "node:crypto";
import { expect, test } from "@playwright/test";

for (const viewport of [{ width: 1440, height: 900 }, { width: 390, height: 844 }]) {
  test(`uses the finance interface at ${viewport.width}px`, async ({ page, context, request }) => {
    await page.setViewportSize(viewport);
    const suffix = randomUUID();
    const email = `browser-${suffix}@example.test`;
    const password = "Synthetic-only-42";
    const registration = await request.post("/api/auth/register", { data: { email, password, name: "Pessoa de teste" } });
    expect(registration.status(), await registration.text()).toBe(201);
    await page.goto("/dashboard");
    await expect(page).toHaveURL(/\/login$/);
    await page.getByLabel("E-mail").fill(email);
    await page.getByLabel("Senha", { exact: true }).fill(password);
    await page.getByRole("button", { name: "Entrar" }).click();
    await expect(page.getByRole("heading", { name: "Evolução financeira" })).toBeVisible();

    const session = (await context.cookies()).find((cookie) => cookie.name.includes("financecontrol_session"));
    expect(session?.httpOnly).toBe(true);
    expect(session?.sameSite).toBe("Strict");
    expect(await page.evaluate(() => localStorage.getItem("financecontrol_token"))).toBeNull();
    const cookieWrite = await context.request.post("/api/accounts", { data: { name: "Teste CSRF", initialBalance: 0, type: 1 } });
    expect(cookieWrite.status()).toBe(403);

    await page.getByRole("link", { name: "Contas", exact: true }).click();
    await page.getByRole("heading", { name: "Nenhuma conta" }).waitFor();
    await page.getByRole("button", { name: /Nova conta/ }).first().click();
    const dialog = page.getByRole("dialog");
    await dialog.getByLabel("Nome").fill("Conta de teste");
    await dialog.getByLabel("Saldo inicial").fill("250");
    await dialog.getByRole("button", { name: "Salvar" }).click();
    await expect(page.getByRole("status")).toContainText("Conta criada");
    await expect(dialog).toBeHidden();
    await page.getByRole("link", { name: "Histórico" }).click();

    await page.getByRole("button", { name: "Novo lançamento" }).click();
    await dialog.getByLabel("Descrição").fill("Compra local");
    await dialog.getByLabel("Valor", { exact: true }).fill("20.25");
    await dialog.getByRole("button", { name: "Salvar" }).click();
    await expect(page.getByRole("cell", { name: "Compra local" })).toBeVisible();
    await page.getByRole("link", { name: "Cartões" }).click();
    await page.getByRole("button", { name: /Novo cartão/ }).first().click();
    await dialog.getByLabel("Nome").fill("Cartão local");
    await dialog.getByLabel("Limite", { exact: true }).fill("500");
    await dialog.getByLabel("Dia de fechamento").fill("31");
    await dialog.getByLabel("Dia de vencimento").fill("10");
    await dialog.getByRole("button", { name: "Salvar" }).click();
    await expect(page.getByText("Cartão local", { exact: true })).toBeVisible();

    await page.getByRole("link", { name: "Lançamentos" }).click();
    await page.getByRole("button", { name: "Novo lançamento" }).click();
    await dialog.getByRole("combobox", { name: "Conta", exact: true }).selectOption({ label: "Cartão local" });
    await dialog.getByLabel("Descrição").fill("Compra no cartão");
    await dialog.getByLabel("Valor", { exact: true }).fill("80");
    await dialog.getByLabel("Parcelas").fill("2");
    await dialog.getByRole("button", { name: "Salvar" }).click();
    await expect(page.getByText("Parcela 2 de 2")).toBeVisible();
    await page.getByRole("link", { name: "Faturas" }).click();
    const invoices = page.getByRole("row").filter({ hasText: "Cartão local" });
    await expect(invoices).toHaveCount(2);
    await invoices.last().getByRole("button", { name: "Fechar fatura" }).click();
    await dialog.getByRole("button", { name: "Confirmar fechamento" }).click();
    await invoices.last().getByRole("button", { name: "Pagar fatura" }).click();
    await dialog.getByLabel("Conta de pagamento").selectOption({ label: "Conta de teste" });
    await dialog.getByLabel("Valor do pagamento").fill("10");
    await dialog.getByRole("button", { name: "Registrar pagamento" }).click();
    await expect(invoices.last()).toContainText("Pagamento parcial");

    await page.getByRole("link", { name: "Dívidas" }).click();
    await page.getByRole("button", { name: "Nova dívida" }).click();
    await dialog.getByLabel("Nome").fill("Financiamento de teste");
    await dialog.getByLabel("Total a pagar, incluindo juros").fill("75");
    await dialog.getByLabel("Quantidade de parcelas").fill("3");
    await dialog.getByRole("button", { name: "Salvar" }).click();
    await expect(page.getByText("Financiamento de teste")).toBeVisible();
    await page.reload();
    await expect(page.getByText("Financiamento de teste")).toBeVisible();

    for (const nav of ["Dashboard", "Contas", "Faturas", "Dívidas"]) {
      await page.getByRole("link", { name: nav, exact: true }).click();
      await expect(page.getByRole("heading", { name: nav, exact: true }).first()).toBeVisible();
      expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(viewport.width);
    }

    await page.getByRole("button", { name: /Sair/ }).click();
    await expect(page).toHaveURL(/\/login$/);
    expect((await context.request.get("/api/accounts")).status()).toBe(401);
  });
}
