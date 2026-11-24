import { defineConfig } from "@playwright/test";

export default defineConfig({
  testDir: "./tests",
  workers: 1,
  timeout: 45_000,
  use: {
    baseURL: process.env.FINANCECONTROL_E2E_URL || "http://localhost:8080",
    launchOptions: {
      executablePath: process.env.FINANCECONTROL_CHROMIUM || undefined,
    },
  },
});
