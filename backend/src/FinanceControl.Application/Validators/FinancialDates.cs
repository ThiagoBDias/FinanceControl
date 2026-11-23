using FinanceControl.Domain.Exceptions;
namespace FinanceControl.Application.Validators;

public static class FinancialDates
{
    private static readonly TimeZoneInfo Brazil = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    public static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Brazil));

    public static void ValidatePaymentDate(DateOnly date)
    {
        if (date.Year < 1900 || date > Today)
            throw new OperationRejectedException("Informe uma data de pagamento válida, até hoje.");
    }
}
