namespace DevKit.ExecutionEngine.SQLServer.Implementations;

internal static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<TSource>> Chunk<TSource>(this IEnumerable<TSource> source, int size)
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