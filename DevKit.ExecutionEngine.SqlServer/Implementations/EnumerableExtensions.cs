namespace DevKit.ExecutionEngine.SQLServer.Implementations;

internal static class EnumerableExtensions
{
    /// <param name="source">Colección que se desea particionar.</param>
    /// <typeparam name="TSource">Tipo de elemento contenido en la secuencia.</typeparam>
    extension<TSource>(IEnumerable<TSource> source)
    {
        /// <summary>
        /// Divide la secuencia en fragmentos del tamaño indicado.
        /// </summary>
        /// <param name="size">Cantidad máxima de elementos por fragmento.</param>
        /// <returns>Colección de fragmentos con los elementos originales.</returns>
        public IEnumerable<IEnumerable<TSource>> Chunk(int size)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than 0");
            }

            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return ChunkSequence(enumerator, size);
                }
            }
        }
    }

    private static IEnumerable<TSource> ChunkSequence<TSource>(IEnumerator<TSource> enumerator, int size)
    {
        int count = 0;
        do
        {
            yield return enumerator.Current;
            count++;
        }
        while (count < size && enumerator.MoveNext());
    }
}