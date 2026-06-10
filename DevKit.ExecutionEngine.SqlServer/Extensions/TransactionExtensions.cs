namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Proporciona métodos de extensión para manejo de transacciones con control de concurrencia.
/// </summary>
public static class TransactionExtensions
{
    /// <summary>
    /// Ejecuta una operación dentro de una transacción con control de concurrencia.
    /// </summary>
    /// <typeparam name="T">Tipo de resultado.</typeparam>
    /// <param name="provider">Instancia del SQL Server Provider.</param>
    /// <param name="operation">Operación a ejecutar dentro de la transacción.</param>
    /// <param name="isolationLevel">Nivel de aislamiento de la transacción.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    public static async Task<T> ExecuteInTransactionAsync<T>(
        this SQLServerProvider provider,
        Func<SqlTransaction, Task<T>> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        await provider.TransactionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (provider.Connection.State == ConnectionState.Closed)
            {
                await provider.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using SqlTransaction transaction = provider.Connection.BeginTransaction(isolationLevel);
            provider.Transaction = transaction;

            try
            {
                T result = await operation(transaction).ConfigureAwait(false);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                provider.Transaction = null;
            }
        }
        finally
        {
            provider.TransactionSemaphore.Release();
        }
    }

    /// <summary>
    /// Ejecuta una operación dentro de una transacción con control de concurrencia (síncrono).
    /// </summary>
    /// <typeparam name="T">Tipo de resultado.</typeparam>
    /// <param name="provider">Instancia del SQL Server Provider.</param>
    /// <param name="operation">Operación a ejecutar dentro de la transacción.</param>
    /// <param name="isolationLevel">Nivel de aislamiento de la transacción.</param>
    /// <returns>Resultado de la operación.</returns>
    public static T ExecuteInTransaction<T>(
        this SQLServerProvider provider,
        Func<SqlTransaction, T> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        provider.TransactionSemaphore.Wait();
        try
        {
            if (provider.Connection.State == ConnectionState.Closed)
            {
                provider.Connection.Open();
            }

            using SqlTransaction transaction = provider.Connection.BeginTransaction(isolationLevel);
            provider.Transaction = transaction;

            try
            {
                T result = operation(transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                provider.Transaction = null;
            }
        }
        finally
        {
            provider.TransactionSemaphore.Release();
        }
    }

    /// <summary>
    /// Ejecuta múltiples operaciones en paralelo dentro de diferentes transacciones con control de concurrencia.
    /// </summary>
    /// <typeparam name="T">Tipo de resultado.</typeparam>
    /// <param name="provider">Instancia del SQL Server Provider.</param>
    /// <param name="operations">Operaciones a ejecutar en paralelo.</param>
    /// <param name="maxConcurrency">Máximo número de operaciones concurrentes.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultados de las operaciones.</returns>
    public static async Task<T[]> ExecuteInParallelAsync<T>(
        this SQLServerProvider provider,
        IEnumerable<Func<SqlTransaction, Task<T>>> operations,
        int maxConcurrency = 3,
        CancellationToken cancellationToken = default)
    {
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        IEnumerable<Task<T>> tasks = operations.Select(async operation =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await provider.ExecuteInTransactionAsync(operation, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        });

        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
