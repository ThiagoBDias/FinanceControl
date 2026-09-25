namespace FinanceControl.Application.UseCases.Transactions;

/// <summary>
/// Resolve a qual fatura (periodo ano/mes) uma compra no cartao pertence,
/// a partir da data de competencia e do dia de fechamento configurado em
/// CreditCardSettings.
///
/// Decisao explicita desta etapa (Application, nao Domain): se a data cair
/// no ou antes do dia de fechamento, pertence a fatura do mes corrente;
/// depois do fechamento, vai para o mes seguinte. O vencimento e resolvido
/// de forma similar a partir do DueDay. Esta e uma simplificacao razoavel
/// para o MVP - regras reais variam por banco/produto e podem ser
/// refinadas depois sem impacto no Domain (Invoice/Account nao conhecem
/// esta logica).
/// </summary>
internal static class InvoicePeriodResolver
{
    public static (int Year, int Month) ResolvePeriod(DateOnly transactionDate, int closingDay)
    {
        if (transactionDate.Day <= closingDay)
            return (transactionDate.Year, transactionDate.Month);

        var nextMonth = transactionDate.AddMonths(1);
        return (nextMonth.Year, nextMonth.Month);
    }

    public static DateOnly ResolveClosingDate(int year, int month, int closingDay)
    {
        var day = Math.Min(closingDay, DateTime.DaysInMonth(year, month));
        return new DateOnly(year, month, day);
    }

    public static DateOnly ResolveDueDate(DateOnly closingDate, int dueDay)
    {
        var fallsInSameMonth = dueDay > closingDate.Day;
        var targetMonth = fallsInSameMonth ? closingDate : closingDate.AddMonths(1);
        var day = Math.Min(dueDay, DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month));
        return new DateOnly(targetMonth.Year, targetMonth.Month, day);
    }
}
