using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceControl.Infrastructure.Persistence.Migrations;

public partial class LedgerPaymentsAndDebts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE "Invoices"
                ADD COLUMN "PaidAmount" numeric(18,2) NOT NULL DEFAULT 0,
                ADD COLUMN "OpeningAmount" numeric(18,2) NOT NULL DEFAULT 0;
            ALTER TABLE "Transactions"
                ADD COLUMN "IsScheduled" boolean NOT NULL DEFAULT false,
                ADD COLUMN "PurchaseId" uuid,
                ADD COLUMN "InstallmentNumber" integer,
                ADD COLUMN "InstallmentCount" integer;
            ALTER TABLE "TransferGroups"
                ADD COLUMN "InvoiceId" uuid REFERENCES "Invoices" ("Id") ON DELETE RESTRICT;
            CREATE INDEX "IX_TransferGroups_InvoiceId" ON "TransferGroups" ("InvoiceId");
            CREATE INDEX IF NOT EXISTS "IX_Transactions_CategoryId" ON "Transactions" ("CategoryId");

            CREATE TABLE "FinancialRequest" (
                "OwnerScopeId" uuid NOT NULL,
                "RequestId" uuid NOT NULL,
                "Fingerprint" varchar(64) NOT NULL,
                "StatusCode" integer NOT NULL,
                "ContentType" text,
                "Location" text,
                "Response" bytea NOT NULL,
                PRIMARY KEY ("OwnerScopeId", "RequestId")
            );

            CREATE TABLE "Debts" (
                "Id" uuid PRIMARY KEY,
                "OwnerScopeId" uuid NOT NULL,
                "Name" varchar(100) NOT NULL,
                "TotalAmount" numeric(18,2) NOT NULL,
                "CategoryId" uuid REFERENCES "Categories" ("Id") ON DELETE RESTRICT
            );
            CREATE INDEX "IX_Debts_CategoryId" ON "Debts" ("CategoryId");
            CREATE INDEX "IX_Debts_OwnerScopeId" ON "Debts" ("OwnerScopeId");

            CREATE TABLE "DebtInstallment" (
                "DebtId" uuid NOT NULL REFERENCES "Debts" ("Id") ON DELETE RESTRICT,
                "Number" integer NOT NULL,
                "DueDate" date NOT NULL,
                "Amount" numeric(18,2) NOT NULL,
                "PaymentDate" date,
                "TransactionId" uuid REFERENCES "Transactions" ("Id") ON DELETE RESTRICT,
                PRIMARY KEY ("DebtId", "Number")
            );
            CREATE UNIQUE INDEX "IX_DebtInstallment_TransactionId" ON "DebtInstallment" ("TransactionId");

            INSERT INTO "Invoices" ("Id", "OwnerScopeId", "CreditCardAccountId", "PeriodYear", "PeriodMonth",
                "ClosingDate", "DueDate", "Status", "CreatedAt", "UpdatedAt", "OpeningAmount")
            SELECT gen_random_uuid(), "OwnerScopeId", "Id", 1, 1, "CreatedAt"::date, "CreatedAt"::date,
                'Closed', NOW(), NOW(), "InitialBalance"
            FROM "Accounts"
            WHERE "Type" = 'CreditCard' AND "InitialBalance" > 0;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Restaure um backup revisado para preservar a conciliação financeira.");
}
