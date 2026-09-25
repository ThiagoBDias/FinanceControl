namespace FinanceControl.Application.Interfaces;

/// <summary>
/// Confirma atomicamente todas as mutacoes feitas na unidade de trabalho
/// atual (contas registradas/alteradas, TransferGroup/Invoice adicionados).
/// Necessario sempre que uma operacao toca mais de um aggregate - em
/// especial CreateTransfer (2 Accounts + 1 TransferGroup)/// (2 Accounts + 1 TransferGroup + 1 Invoice).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
