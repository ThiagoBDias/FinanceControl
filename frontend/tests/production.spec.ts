import { readFileSync } from "node:fs";
import { expect, test } from "@playwright/test";

test("production enforces restricted enrollment and secure session headers", async ({ request }) => {
  const file = process.env.FINANCECONTROL_PRODUCTION_FIXTURE;
  test.skip(!file, "Use the production verification procedure with a synthetic existing test user.");
  const { email } = JSON.parse(readFileSync(file!, "utf8"));
  const headers = { "X-FinanceControl-Client": "browser" };

  const denied = await request.post("/api/auth/register", {
    headers, data: { name: "Sem convite", email: "no-invite@example.test", password: "Synthetic-only-42" },
  });
  expect(denied.status()).toBe(403);
  const anonymous = await request.get("/api/accounts");
  expect(anonymous.status()).toBe(401);
  expect(anonymous.headers()["cache-control"]).toBe("no-store");
  const shell = await request.get("/dashboard");
  expect(shell.ok()).toBeTruthy();
  expect(shell.headers()["content-security-policy"]).toContain("frame-ancestors 'none'");
  const session = await request.post("/api/auth/login", { headers, data: { email, password: "Synthetic-only-42" } });
  expect(session.status(), await session.text()).toBe(204);
  const cookie = session.headers()["set-cookie"];
  expect(cookie).toMatch(/^__Host-financecontrol_session=/);
  expect(cookie).toMatch(/; secure(?:;|$)/i);
  expect(cookie).toMatch(/; samesite=strict(?:;|$)/i);
  expect(cookie).toMatch(/; httponly(?:;|$)/i);
  const browserSession = await request.get("/api/auth/me");
  expect(browserSession.ok(), await browserSession.text()).toBeTruthy();
  const logout = await request.post("/api/auth/logout", { headers });
  expect(logout.status()).toBe(204);
  expect((await request.get("/api/auth/me")).status()).toBe(401);
});
