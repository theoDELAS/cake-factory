namespace CakeMachine.Utils;

internal class EnumerateCompletedTasks<T> : IAsyncEnumerable<T>
{
    private readonly IEnumerable<Task<T>> _tasks;

    public EnumerateCompletedTasks(IEnumerable<Task<T>> tasks)
    {
        _tasks = tasks;
    }

    /// <inheritdoc />
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new ()) 
        => new EnumeratorOfTasks<T>(_tasks, cancellationToken);

    private class EnumeratorOfTasks<TEnumerator> : IAsyncEnumerator<TEnumerator>
    {
        private readonly HashSet<Task<TEnumerator>> _tasks;
        private readonly CancellationToken _token;
        private TEnumerator? _current;

        public EnumeratorOfTasks(IEnumerable<Task<TEnumerator>> tasks, CancellationToken token)
        {
            _tasks = new HashSet<Task<TEnumerator>>(tasks);
            _token = token;
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            _token.ThrowIfCancellationRequested();
            if (!_tasks.Any()) return false;

            var task = await Task.WhenAny(_tasks).ConfigureAwait(false);
            _tasks.Remove(task);
            _current = await task.ConfigureAwait(false);

            return true;
        }

        /// <inheritdoc />
        public TEnumerator Current => _current ?? throw new InvalidOperationException();
    }
}

internal static class EnumerateCompletedTasksExtensions
{
    public static IAsyncEnumerable<T> EnumerateCompleted<T>(this IEnumerable<Task<T>> tasks)
        => new EnumerateCompletedTasks<T>(tasks);
}