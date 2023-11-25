using CatFact.Applications.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace CatFact.Infrastructures.Services
{
    public class MemoizationService : IMemoizationService
    {
        private readonly IDistributedCache _cache;
        public MemoizationService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<T> MemoizeAsync<T>(DistributedCacheEntryOptions cacheOption, Expression<Func<Task<T>>> funcExpr)
        {
            var methodCallExpression = funcExpr.Body as MethodCallExpression;

            if (methodCallExpression == null)
            {
                throw new ArgumentException("Expression must be a method call.");
            }

            var methodName = methodCallExpression.Method.Name; // Get the method name
            var arguments = methodCallExpression.Arguments.Select(arg => Expression.Lambda(arg).Compile().DynamicInvoke()).ToArray();
            var parameters = methodCallExpression.Method.GetParameters();
            var cacheKey = GenerateCacheKey(methodName, arguments, parameters);

            var cachedResult = _cache.GetString(cacheKey);

            if (cachedResult != null)
            {
                return Deserialize<T>(cachedResult);
            }
            else
            {
                var func = funcExpr.Compile();
                var result = await func();
                var data = Serialize<T>(result);
                await _cache.SetStringAsync(cacheKey, data, cacheOption);
                return result;
            }
        }

        private string GenerateCacheKey(string methodName, object[] arguments, ParameterInfo[] parameters)
        {
            var keyBuilder = new StringBuilder(methodName);
            keyBuilder.Append("_");

            var paramsArgsStringList = new List<string>();

            for (int i = 0; i < arguments.Length; i++)
            {
                var parameterName = parameters[i].Name; // Get the parameter name
                var argumentValue = arguments[i];

                paramsArgsStringList.Add($"{parameterName}-{argumentValue}");
            }
            keyBuilder.Append(string.Join("-", paramsArgsStringList));

            return keyBuilder.ToString();
        }

        private string Serialize<T>(T obj)
        {
            if (obj == null)
            {
                return null;
            }

            try
            {
                // Using System.Text.Json for serialization
                var serialized = JsonSerializer.Serialize(obj);
                return serialized;
            }
            catch (Exception ex)
            {
                // Handle serialization failure
                throw new InvalidOperationException("Serialization failed.", ex);
            }
        }

        private T Deserialize<T>(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return default;
            }

            try
            {
                // Using System.Text.Json for deserialization
                var deserialized = JsonSerializer.Deserialize<T>(data);
                return deserialized;
            }
            catch (Exception ex)
            {
                // Handle deserialization failure
                throw new InvalidOperationException("Deserialization failed.", ex);
            }
        }
    }
}
