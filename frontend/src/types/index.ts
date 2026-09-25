export type Account = {
  id: string;
  name: string;
  type: string;
  initialBalance?: number;
  cachedBalance?: number;
  balance?: number;
  isActive?: boolean;
  creditCardSettings?: {
    creditLimit: number;
    closingDay: number;
    dueDay: number;
  } | null;
};

export type Category = {
  id: string;
  name: string;
  type: string;
  isActive: boolean;
};

export type Transaction = {
  id: string;
  accountId: string;
  categoryId?: string | null;
  invoiceId?: string | null;
  transferGroupId?: string | null;
  amount: number;
  type: string;
  paymentDate: string;
  description?: string | null;
  isDeleted?: boolean;
};

export type Invoice = {
  id: string;
  creditCardAccountId: string;
  periodYear: number;
  periodMonth: number;
  closingDate: string;
  dueDate: string;
  status: string;
  paidAmount: number;
  totalAmount?: number;
};

export type Budget = {
  id: string;
  categoryId: string;
  year: number;
  month: number;
  limit?: number;
  amount?: number;
  spent?: number;
  remaining?: number;
  exceeded?: number;
};

export type DashboardData = {
  balance?: number;
  totalBalance?: number;
  income?: number;
  totalIncome?: number;
  expenses?: number;
  totalExpenses?: number;
  net?: number;
  result?: number;
  accounts?: Account[];
  recentTransactions?: Transaction[];
  transactions?: Transaction[];
  expensesByCategory?: Array<{ name: string; amount: number }>;
};

export type AuthResponse = {
  token?: string;
  accessToken?: string;
  refreshToken?: string;
  user?: unknown;
};

export type Paged<T> = T[] | { items?: T[]; data?: T[]; totalCount?: number };
