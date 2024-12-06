﻿// Description: Entity Framework Bulk Operations & Utilities (EF Bulk SaveChanges, Insert, Update, Delete, Merge | LINQ Query Cache, Deferred, Filter, IncludeFilter, IncludeOptimize | Audit)
// Website & Documentation: https://github.com/zzzprojects/Entity-Framework-Plus
// Forum & Issues: https://github.com/zzzprojects/EntityFramework-Plus/issues
// License: https://github.com/zzzprojects/EntityFramework-Plus/blob/master/LICENSE
// More projects: http://www.zzzprojects.com/
// Copyright © ZZZ Projects Inc. 2014 - 2016. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if EF5
using System.Data.Objects;

#elif EF6
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

#elif EFCORE
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.Internal;

#endif

namespace Z.EntityFramework.Plus
{
    /// <summary>Class for query future value.</summary>
    /// <typeparam name="T">The type of elements of the query.</typeparam>
#if QUERY_INCLUDEOPTIMIZED
    internal class QueryFutureEnumerable<T> : BaseQueryFuture, IEnumerable<T>
#else
    public class QueryFutureEnumerable<T> : BaseQueryFuture, IEnumerable<T>
#endif
    {

        /// <summary>The result of the query future.</summary>
        private IEnumerable<T> _result;

        /// <summary>Constructor.</summary>
        /// <param name="ownerBatch">The batch that owns this item.</param>
        /// <param name="query">
        ///     The query to defer the execution and to add in the batch of future
        ///     queries.
        /// </param>
#if EF5 || EF6
        public QueryFutureEnumerable(QueryFutureBatch ownerBatch, ObjectQuery<T> query)
#elif EFCORE
        public QueryFutureEnumerable(QueryFutureBatch ownerBatch, IQueryable query)
#endif
        {
            OwnerBatch = ownerBatch;
            Query = query;
        }

        /// <summary>Gets the enumerator of the query future.</summary>
        /// <returns>The enumerator of the query future.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> values;

            if (!HasValue)
            {
                OwnerBatch.ExecuteQueries();
            }

            if (_result == null)
            {
                values = new List<T>().GetEnumerator();
            }
            else
            {
                values = _result.GetEnumerator(); 
            }

#if EFCORE_3X
            if (IsIncludeOptimizedNullCollectionNeeded)
			{ 
                QueryIncludeOptimizedNullCollection.NullCollectionToEmpty(_result, Childs);
            }
#endif

            return values; 
        }


        /// <summary>Gets the enumerator of the query future.</summary>
        /// <returns>The enumerator of the query future.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#if NET45 
        public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
	        cancellationToken.ThrowIfCancellationRequested();
			if (!HasValue)
            {
#if EF6
                if (Query.Context.IsInMemoryEffortQueryContext())
                {
                    OwnerBatch.ExecuteQueries();
                }
                else
                {
                    await OwnerBatch.ExecuteQueriesAsync(cancellationToken).ConfigureAwait(false);
                }
#else
                await OwnerBatch.ExecuteQueriesAsync(cancellationToken).ConfigureAwait(false);
#endif
			}

			if (_result == null)
            {
                return new List<T>();
            }

            using (var enumerator = _result.GetEnumerator())
            {
                var list = new List<T>();
                while (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }
                return list;
            }
        }

#if NET45
		public async Task<T[]> ToArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
	        cancellationToken.ThrowIfCancellationRequested();
			if (!HasValue)
            {
#if EF6
                if (Query.Context.IsInMemoryEffortQueryContext())
                {
                    OwnerBatch.ExecuteQueries();
                }
                else
                {
                    await OwnerBatch.ExecuteQueriesAsync(cancellationToken).ConfigureAwait(false);
                }
#else
                await OwnerBatch.ExecuteQueriesAsync(cancellationToken ).ConfigureAwait(false);
#endif

			}

			if (_result == null)
            {
                return new T[0];
            }

            using (var enumerator = _result.GetEnumerator())
            {
                var list = new List<T>();
                while (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }
                return list.ToArray();
            }
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            if (!HasValue)
            {
                await OwnerBatch.ExecuteQueriesAsync().ConfigureAwait(false);
            }

            if (_result == null)
            {
                return default(T);
            }

            using (var enumerator = _result.GetEnumerator())
            {
                enumerator.MoveNext();
                return enumerator.Current;
            }
        }
#endif
#endif

		/// <summary>Sets the result of the query deferred.</summary>
		/// <param name="reader">The reader returned from the query execution.</param>
		public override void SetResult(DbDataReader reader)
        {
            if (reader.GetType().FullName.Contains("Oracle"))
            {
                var reader2 = new QueryFutureOracleDbReader(reader);
                reader = reader2;
            }
            
            var enumerator = GetQueryEnumerator<T>(reader);

            using (enumerator)
            {
                SetResult(enumerator);
            }  
        }

        public void SetResult(IEnumerator<T> enumerator)
        {
            // Enumerate on all items
            var list = new List<T>();
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
            }
            _result = list;

            HasValue = true;
        } 

#if EFCORE
		public override void ExecuteInMemory()
        {
            HasValue = true;
            _result = ((IQueryable<T>) Query).ToList();
        }
#endif
        public override void GetResultDirectly()
        {
            var query = ((IQueryable<T>)Query);

            GetResultDirectly(query);
        }


#if   NET45
		public override Task GetResultDirectlyAsync(CancellationToken cancellationToken)
	    {
		    cancellationToken.ThrowIfCancellationRequested();
 
		    var query = ((IQueryable<T>)Query);
			GetResultDirectly(query);

	        return Task.FromResult(0);
	    }
#endif

		internal void GetResultDirectly(IQueryable<T> query)
        {
            using (var enumerator = query.GetEnumerator())
            {
                SetResult(enumerator);
            }
        } 
	}
}