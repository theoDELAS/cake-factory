namespace CakeMachine.Utils;

internal class ConcatAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T>[] _elements;

    public ConcatAsyncEnumerable(params IAsyncEnumerable<T>[] elements)
    {
        _elements = elements;
    }

    /// <inheritdoc />
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new ())
        => new Enumerator<T>(_elements.Select(e => e.GetAsyncEnumerator(cancellationToken)).ToArray(), cancellationToken);

    private class Enumerator<T2> : IAsyncEnumerator<T2>
    {
        private readonly CancellationToken _token;
        private readonly IDictionary<IAsyncEnumerator<T2>, bool> _enumerators;
        private T2? _current;

        public Enumerator(IAsyncEnumerator<T2>[] elements, CancellationToken token)
        {
            _token = token;
            _enumerators = elements.ToDictionary(e => e, _ => false);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            foreach (var asyncEnumerator in _enumerators.Keys)
            {
                await asyncEnumerator.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            _token.ThrowIfCancellationRequested();

            var alreadyPulled = _enumerators.Where(e => e.Value).ToArray();
            if(alreadyPulled.Any())
            {
                var pulled = alreadyPulled.First().Key;

                _current = pulled.Current;
                _enumerators[pulled] = false;
                return true;
            }

            foreach (var enumerator in _enumerators.Keys)
            {
                var hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                _enumerators[enumerator] = hasNext;
            }

            return _enumerators.Any(e => e.Value) && await MoveNextAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public T2 Current => _current!;
    }
}