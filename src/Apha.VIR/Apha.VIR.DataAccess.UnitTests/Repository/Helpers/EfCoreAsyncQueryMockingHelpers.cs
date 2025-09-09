using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace Apha.VIR.DataAccess.UnitTests.Repository.Helpers
{
    public class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) =>
            new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression) => inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments().FirstOrDefault() ?? throw new InvalidOperationException("TResult must be a generic Task<T>.");
            var executeMethod = typeof(IQueryProvider).GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: [typeof(Expression)]) ?? throw new InvalidOperationException("Failed to find IQueryProvider.Execute<T> method.");
            var genericExecuteMethod = executeMethod.MakeGenericMethod(expectedResultType);

            var executionResult = genericExecuteMethod.Invoke(inner, [expression]) ?? throw new InvalidOperationException("Execution returned null.");
            var fromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult)) ?? throw new InvalidOperationException("Failed to find Task.FromResult method.");
            var genericFromResultMethod = fromResultMethod.MakeGenericMethod(expectedResultType);

            var taskResult = genericFromResultMethod.Invoke(null, [executionResult]);
            return taskResult == null ? throw new InvalidOperationException("Task.FromResult returned null.") : (TResult)taskResult;
        }
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    public class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner = inner;

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());

    }
}
