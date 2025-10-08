using System.Collections.Concurrent;

namespace GeneratingMiddlewareWithCopilot.Services;

/// <summary>
/// Thread-safe service that tracks how many times a given route (by template or path) was called.
/// Backed by <see cref="ConcurrentDictionary{TKey, TValue}"/> to allow updates from multiple requests concurrently.
/// Register as a singleton so all requests share the same counts.
/// </summary>
public class RouteCallCounter
{
    private readonly ConcurrentDictionary<string, long> _counts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Increments the count for the provided route key in a lock-free manner.
    /// </summary>
    /// <param name="routeKey">Route template or path used as the aggregation key.</param>
    public void Increment(string routeKey)
    {
        _counts.AddOrUpdate(routeKey, 1, static (_, current) => current + 1);
    }

    /// <summary>
    /// Returns a snapshot of the current counts. A copy is returned to avoid exposing internal state.
    /// </summary>
    public IReadOnlyDictionary<string, long> GetCounts()
    {
        // Return a snapshot to avoid exposing the internal concurrent dictionary directly
        return new Dictionary<string, long>(_counts);
    }
}
