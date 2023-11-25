using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Expressions;

namespace CatFact.Applications.Interfaces
{
    public interface IMemoizationService
    {
        Task<T> MemoizeAsync<T>(DistributedCacheEntryOptions cacheOption, Expression<Func<Task<T>>> funcExpr);
    }
}