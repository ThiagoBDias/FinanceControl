SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN (
    'Users','Accounts','CreditCardSettings','Categories','Transactions',
    'TransferGroups','Invoices','Budgets','AuditLogEntries',
    'AspNetUsers','AspNetRoles'
  )
ORDER BY table_name;
