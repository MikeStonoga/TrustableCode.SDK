namespace TrustableCode.SDK.BusinessModeling.Transactions;

/// <summary>
/// Runs application work inside a business transaction so state changes and outbox persistence can commit atomically.
/// </summary>
public interface IBusinessTransactionRunner
{
    /// <summary>
    /// Executes the provided work inside a transaction boundary.
    /// </summary>
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
