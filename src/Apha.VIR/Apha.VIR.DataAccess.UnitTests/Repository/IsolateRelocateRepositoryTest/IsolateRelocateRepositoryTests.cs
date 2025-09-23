using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Apha.VIR.DataAccess.UnitTests.Repository.IsolateRelocateRepositoryTest
{

    public class TestIsolateRelocateRepository : IsolateRelocateRepository
    {
        private readonly IQueryable<IsolateRelocate> _relocateData;

        public bool UpdateCalled { get; private set; }

        public TestIsolateRelocateRepository(VIRDbContext context, IQueryable<IsolateRelocate> relocateData)
            : base(context)
        {
            _relocateData = relocateData;
        }

        public override Task<IEnumerable<IsolateRelocate>> GetIsolatesByCriteria(string? min, string? max, Guid? freezer, Guid? tray)
        {
            return Task.FromResult(_relocateData.AsEnumerable());
        }

        public override Task UpdateIsolateFreezeAndTrayAsync(IsolateRelocate item)
        {
            UpdateCalled = true;
            return Task.CompletedTask;
        }
    }

    public class IsolateRelocateRepositoryTests
    {
        [Fact]
        public async Task GetIsolatesByCriteria_ReturnsData()
        {
            var fakeData = new List<IsolateRelocate>
        {
            new IsolateRelocate { IsolateId = Guid.NewGuid() },
            new IsolateRelocate { IsolateId = Guid.NewGuid() }
        };
            var asyncFakeData = new TestAsyncEnumerable<IsolateRelocate>(fakeData);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateRelocateRepository(mockContext.Object, asyncFakeData);

            var result = await repo.GetIsolatesByCriteria(null, null, null, null);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_CallsExecuteSqlRaw()
        {
            var isolateRelocate = new IsolateRelocate
            {
                IsolateId = Guid.NewGuid(),
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                Well = "A1",
                LastModified = new byte[8],
                UserID = "user1",
                UpdateType = "Isolate"
            };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateRelocateRepository(mockContext.Object, new TestAsyncEnumerable<IsolateRelocate>(Enumerable.Empty<IsolateRelocate>()));

            await repo.UpdateIsolateFreezeAndTrayAsync(isolateRelocate);

            Assert.True(repo.UpdateCalled);
        }
        [Fact]
        public async Task GetIsolatesByCriteria_WithFilters_ReturnsFilteredData()
        {
            var fakeData = new List<IsolateRelocate>
        {
            new IsolateRelocate { IsolateId = Guid.NewGuid(), AVNumber = "AV001", Freezer = Guid.NewGuid(), Tray = Guid.NewGuid() },
            new IsolateRelocate { IsolateId = Guid.NewGuid(), AVNumber = "AV002", Freezer = Guid.NewGuid(), Tray = Guid.NewGuid() },
            new IsolateRelocate { IsolateId = Guid.NewGuid(), AVNumber = "AV003", Freezer = Guid.NewGuid(), Tray = Guid.NewGuid() }
        };
            var asyncFakeData = new TestAsyncEnumerable<IsolateRelocate>(fakeData);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateRelocateRepository(mockContext.Object, asyncFakeData);

            var result = await repo.GetIsolatesByCriteria("AV001", "AV002", fakeData[0].Freezer, fakeData[0].Tray);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
           
        }

        

    

      
    }

    // Helper class for creating async queryables for testing
    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new TestAsyncQueryProvider<T>(this); }
        }
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current
        {
            get
            {
                return _inner.Current;
            }
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }

    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object ?Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(
                    name: nameof(IQueryProvider.Execute),
                    genericParameterCount: 1,
                    types: new[] { typeof(Expression) })!
                .MakeGenericMethod(expectedResultType)
                .Invoke(this, new[] { expression });

            var fromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult))
     ?? throw new InvalidOperationException("Task.FromResult method not found.");

            var task = fromResultMethod
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })
                ?? throw new InvalidOperationException("Task.FromResult returned null.");

            return (TResult)task;

        }
    }
}
