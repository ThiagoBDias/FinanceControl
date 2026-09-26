export type Account = {
  id: string;
  name: string;
  type: number | string;
  initialBalance: number;
  calculatedBalance: number;
  isActive: boolean;
  ownerScopeId: string;
  creditCardSettings?: {
    creditLimit: number;
    closingDay: number;
    dueDay: number;
  } | null;
};

export type Category = {
  id: string;
  name: string;
  type: number | string;
  isActive: boolean;
};

export type Transaction = {
  id: string;
  accountId: string;
  categoryId?: string | null;
  invoiceId?: string | null;
  transferGroupId?: string | null;
  amount: number;
  type: number | string;
  competenceDate: string;
  paymentDate: string | null;
  dueDate: string | null;
  description: string;
};

export type Invoice = {
  id: string;
  creditCardAccountId: string;
  periodYear: number;
  periodMonth: number;
  closingDate: string;
  dueDate: string;
  status: number | string;
  totalAmount: number;
};

export type Budget = {
  id: string;
  categoryId: string;
  year: number;
  month: number;
  categoryName: string;
  limitAmount: number;
  spentAmount: number;
  remainingAmount: number;
  isOverLimit: boolean;
};

export type DashboardData = {
  year: number;
  month: number;
  netBalance: number;
  income: number;
  expenses: number;
  result: number;
  expensesByCategory: Array<{ categoryId: string | null; categoryName: string; amount: number }>;
  accounts: Array<{ accountId: string; name: string; type: string; balance: number; isLiability: boolean }>;
  creditCards: Array<{ accountId: string; name: string; creditLimit: number; usedAmount: number; availableAmount: number }>;
  invoices: Array<{ id: string; creditCardAccountId: string; periodYear: number; periodMonth: number; status: string; totalAmount: number; dueDate: string }>;
  budgets: Array<{ categoryId: string; categoryName: string; limitAmount: number; spentAmount: number; remainingAmount: number; isOverLimit: boolean }>;
  monthlyEvolution: Array<{ year: number; month: number; income: number; expenses: number; result: number }>;
};

export type AuthResponse = {
  accessToken: string;
  userId: string;
  name: string;
  email: string;
  expiresAtUtc: string;
};

export type Paged<T> = T[] | { items?: T[]; data?: T[]; totalCount?: number };
