namespace DevKit.ExecutionEngine.Redis;

/// <summary>
/// Interfaz que define los métodos para interactuar con el servicio de caché Redis.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtiene un valor del caché o lo establece si no existe.
    /// La expresión lambda representa la operación asíncrona que produce el valor.
    /// </summary>
    /// <typeparam name="T">El tipo del valor a cachear.</typeparam>
    /// <param name="expression">Expresión que define la tarea asíncrona para obtener el valor.</param>
    /// <returns>El valor cacheado o el resultado de la expresión.</returns>
    Task<T> GetOrSetAsync<T>(Expression<Func<Task<T>>> expression);

    /// <summary>
    /// Invalida las entradas de caché correspondientes a las expresiones proporcionadas.
    /// </summary>
    /// <param name="expressions">Expresiones que representan las claves de caché a invalidar.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task InvalidateCacheAsync(params Expression[] expressions);
}
