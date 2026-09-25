using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace FinanceControl.Infrastructure.Persistence.Migrations;

using FinanceControl.Infrastructure.Persistence;
/// <summary>
/// Initial schema for the FinanceControl MVP.
///
/// This migration is intentionally SQL-backed because the development environment
/// used to prepare the repository does not contain the dotnet SDK/dotnet-ef tool.
/// The SQL mirrors the EF Core model configured in Persistence/Configurations.
/// </summary>
[DbContext(typeof(FinanceControlDbContext))]
[Migration("20260924180000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "AspNetRoles" (
                "Id" uuid NOT NULL,
                "Name" character varying(256),
                "NormalizedName" character varying(256),
                "ConcurrencyStamp" text,
                CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex"
                ON "AspNetRoles" ("NormalizedName")
                WHERE "NormalizedName" IS NOT NULL;

            CREATE TABLE IF NOT EXISTS "AspNetUsers" (
                "Id" uuid NOT NULL,
                "UserName" character varying(256),
                "NormalizedUserName" character varying(256),
                "Email" character varying(256),
                "NormalizedEmail" character varying(256),
                "EmailConfirmed" boolean NOT NULL,
                "PasswordHash" text,
                "SecurityStamp" text,
                "ConcurrencyStamp" text,
                "PhoneNumber" text,
                "PhoneNumberConfirmed" boolean NOT NULL,
                "TwoFactorEnabled" boolean NOT NULL,
                "LockoutEnd" timestamp with time zone,
                "LockoutEnabled" boolean NOT NULL,
                "AccessFailedCount" integer NOT NULL,
                "Name" text NOT NULL,
                CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex"
                ON "AspNetUsers" ("NormalizedUserName")
                WHERE "NormalizedUserName" IS NOT NULL;
            CREATE INDEX IF NOT EXISTS "EmailIndex"
                ON "AspNetUsers" ("NormalizedEmail");

            CREATE TABLE IF NOT EXISTS "Users" (
                "Id" uuid NOT NULL,
                "Name" character varying(200) NOT NULL,
                "Email" character varying(320) NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");

            CREATE TABLE IF NOT EXISTS "Accounts" (
                "Id" uuid NOT NULL,
                "OwnerScopeId" uuid NOT NULL,
                "Name" character varying(200) NOT NULL,
                "Type" character varying(20) NOT NULL,
                "InitialBalance" numeric(18,2) NOT NULL,
                "CachedBalance" numeric(18,2) NOT NULL,
                "CachedBalanceUpdatedAt" timestamp with time zone NOT NULL,
                "IsActive" boolean NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id")
            );
            CREATE INDEX IF NOT EXISTS "IX_Accounts_OwnerScopeId" ON "Accounts" ("OwnerScopeId");

            CREATE TABLE IF NOT EXISTS "CreditCardSettings" (
                "AccountId" uuid NOT NULL,
                "CreditLimit" numeric(18,2) NOT NULL,
                "ClosingDay" integer NOT NULL,
                "DueDay" integer NOT NULL,
                CONSTRAINT "PK_CreditCardSettings" PRIMARY KEY ("AccountId"),
                CONSTRAINT "FK_CreditCardSettings_Accounts_AccountId"
                    FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS "Categories" (
                "Id" uuid NOT NULL,
                "OwnerScopeId" uuid NOT NULL,
                "Name" character varying(150) NOT NULL,
                "Type" character varying(20) NOT NULL,
                "IsActive" boolean NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
            );
            CREATE INDEX IF NOT EXISTS "IX_Categories_OwnerScopeId" ON "Categories" ("OwnerScopeId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Categories_OwnerScopeId_Name"
                ON "Categories" ("OwnerScopeId", "Name");

            CREATE TABLE IF NOT EXISTS "TransferGroups" (
                "Id" uuid NOT NULL,
                "OwnerScopeId" uuid NOT NULL,
                "SourceAccountId" uuid NOT NULL,
                "DestinationAccountId" uuid NOT NULL,
                "Amount" numeric(18,2) NOT NULL,
                "TransferDate" date NOT NULL,
                "Description" character varying(500),
                "CreatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_TransferGroups" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_TransferGroups_Accounts_SourceAccountId"
                    FOREIGN KEY ("SourceAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_TransferGroups_Accounts_DestinationAccountId"
                    FOREIGN KEY ("DestinationAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
            );
            CREATE INDEX IF NOT EXISTS "IX_TransferGroups_OwnerScopeId" ON "TransferGroups" ("OwnerScopeId");
            CREATE INDEX IF NOT EXISTS "IX_TransferGroups_SourceAccountId" ON "TransferGroups" ("SourceAccountId");
            CREATE INDEX IF NOT EXISTS "IX_TransferGroups_DestinationAccountId" ON "TransferGroups" ("DestinationAccountId");

            CREATE TABLE IF NOT EXISTS "Invoices" (
                "Id" uuid NOT NULL,
                "OwnerScopeId" uuid NOT NULL,
                "CreditCardAccountId" uuid NOT NULL,
                "PeriodYear" integer NOT NULL,
                "PeriodMonth" integer NOT NULL,
                "ClosingDate" date NOT NULL,
                "DueDate" date NOT NULL,
                "Status" character varying(20) NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Invoices" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_Invoices_Accounts_CreditCardAccountId"
                    FOREIGN KEY ("CreditCardAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
            );
            CREATE INDEX IF NOT EXISTS "IX_Invoices_OwnerScopeId" ON "Invoices" ("OwnerScopeId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Invoices_CreditCardAccountId_PeriodYear_PeriodMonth"
                ON "Invoices" ("CreditCardAccountId", "PeriodYear", "PeriodMonth");

            CREATE TABLE IF NOT EXISTS "Transactions" (
                "Id" uuid NOT NULL,
                "OwnerScopeId" uuid NOT NULL,
                "AccountId" uuid NOT NULL,
                "Type" character varying(20) NOT NULL,
                "Amount" numeric(18,2) NOT NULL,
                "Description" character varying(500) NOT NULL,
                "CategoryId" uuid,
                "InvoiceId" uuid,
                "TransferGroupId" uuid,
                "CompetenceDate" date NOT NULL,
                "DueDate" date,
                "PaymentDate" date,
                "IsDeleted" boolean NOT NULL,
                "DeletedAt" timestamp with time zone,
                "Provider" character varying(100),
                "ExternalAccountId" character varying(200),
                "ExternalTransactionId" character varying(200),
                "DedupHash" character varying(128),
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Transactions" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_Transactions_Accounts_AccountId"
                    FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_Transactions_Categories_CategoryId"
                    FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_Transactions_Invoices_InvoiceId"
                    FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_Transactions_TransferGroups_TransferGroupId"
                    FOREIGN KEY ("TransferGroupId") REFERENCES "TransferGroups" ("Id") ON DELETE RESTRICT
            );
            CREATE INDEX IF NOT EXISTS "IX_Transactions_OwnerScopeId" ON "Transactions" ("OwnerScopeId");
            CREATE INDEX IF NOT EXISTS "IX_Transactions_AccountId_CompetenceDate"
                ON "Transactions" ("AccountId", "CompetenceDate");
            CREATE INDEX IF NOT EXISTS "IX_Transactions_InvoiceId" ON "Transactions" ("InvoiceId");
            CREATE INDEX IF NOT EXISTS "IX_Transactions_TransferGroupId" ON "Transactions" ("TransferGroupId");
            CREATE INDEX IF NOT EXISTS "IX_Transactions_DedupHash" ON "Transactions" ("DedupHash");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Transactions_Provider_ExternalAccountId_ExternalTransactionId"
                ON "Transactions" ("Provider", "ExternalAccountId", "ExternalTransactionId")
                WHERE "ExternalTransactionId" IS NOT NULL;

            CREATE TABLE IF NOT EXISTS "Budgets" (
                "Id" uuid NOT NULL,
                "OwnerScopeId" uuid NOT NULL,
                "CategoryId" uuid NOT NULL,
                "Year" integer NOT NULL,
                "Month" integer NOT NULL,
                "LimitAmount" numeric(18,2) NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Budgets" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_Budgets_Categories_CategoryId"
                    FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Budgets_OwnerScopeId_CategoryId_Year_Month"
                ON "Budgets" ("OwnerScopeId", "CategoryId", "Year", "Month");
            CREATE INDEX IF NOT EXISTS "IX_Budgets_OwnerScopeId_Year_Month"
                ON "Budgets" ("OwnerScopeId", "Year", "Month");

            CREATE TABLE IF NOT EXISTS "AuditLogEntries" (
                "Id" uuid NOT NULL,
                "OwnerScopeId" uuid NOT NULL,
                "EntityName" character varying(100) NOT NULL,
                "EntityId" uuid NOT NULL,
                "Action" character varying(20) NOT NULL,
                "ChangesJson" jsonb NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_AuditLogEntries" PRIMARY KEY ("Id")
            );
            CREATE INDEX IF NOT EXISTS "IX_AuditLogEntries_OwnerScopeId" ON "AuditLogEntries" ("OwnerScopeId");
            CREATE INDEX IF NOT EXISTS "IX_AuditLogEntries_EntityName_EntityId"
                ON "AuditLogEntries" ("EntityName", "EntityId");

            CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
                "Id" integer GENERATED BY DEFAULT AS IDENTITY,
                "RoleId" uuid NOT NULL,
                "ClaimType" text,
                "ClaimValue" text,
                CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId"
                    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

            CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
                "Id" integer GENERATED BY DEFAULT AS IDENTITY,
                "UserId" uuid NOT NULL,
                "ClaimType" text,
                "ClaimValue" text,
                CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

            CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
                "LoginProvider" character varying(128) NOT NULL,
                "ProviderKey" character varying(128) NOT NULL,
                "ProviderDisplayName" text,
                "UserId" uuid NOT NULL,
                CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
                CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

            CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
                "UserId" uuid NOT NULL,
                "RoleId" uuid NOT NULL,
                CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
                CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId"
                    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

            CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
                "UserId" uuid NOT NULL,
                "LoginProvider" character varying(128) NOT NULL,
                "Name" character varying(128) NOT NULL,
                "Value" text,
                CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
                CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );

            -- AuditLogEntries is application-append-only by design. The actual
            -- REVOKE must be executed for the concrete PostgreSQL application role
            -- used in deployment; that role is environment-specific.
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS "AuditLogEntries";
            DROP TABLE IF EXISTS "Budgets";
            DROP TABLE IF EXISTS "Transactions";
            DROP TABLE IF EXISTS "Invoices";
            DROP TABLE IF EXISTS "TransferGroups";
            DROP TABLE IF EXISTS "CreditCardSettings";
            DROP TABLE IF EXISTS "Categories";
            DROP TABLE IF EXISTS "Accounts";
            DROP TABLE IF EXISTS "Users";
            DROP TABLE IF EXISTS "AspNetUserTokens";
            DROP TABLE IF EXISTS "AspNetUserRoles";
            DROP TABLE IF EXISTS "AspNetUserLogins";
            DROP TABLE IF EXISTS "AspNetUserClaims";
            DROP TABLE IF EXISTS "AspNetRoleClaims";
            DROP TABLE IF EXISTS "AspNetUsers";
            DROP TABLE IF EXISTS "AspNetRoles";
            """);
    }
}
