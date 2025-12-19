using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FFXIVVenues.DomainData.Entities.Venues;
using FFXIVVenues.VenueModels.Observability;

namespace FFXIVVenues.ApiGateway.Observability;

public class ChangeBroker : IChangeBroker
{
    private readonly List<(Observer Observer, InvocationKind invocationKind)> _observers = new();
    private readonly ConcurrentDictionary<string, Timer> _queuedOperationTimers = new();
    private readonly ConcurrentDictionary<string, (ObservableOperation[] Operations, Venue Venue)> _queuedOperations = new();

    public Action Observe(Observer observer, InvocationKind invocationKind)
    {
        _observers.Add((observer, invocationKind));
        return () =>
        {
            _observers.Remove((observer, invocationKind));
        };
    }

    public void Queue(ObservableOperation operation, Venue venue)
    {
        foreach (var (observer, invocationKind) in _observers)
        {
            if (!observer.Matches(operation, venue)) continue;

            if (invocationKind == InvocationKind.Immediate)
            {
                observer.ObserverAction(operation, venue);
                continue;
            }

            var key = venue.Id;
            if (_queuedOperationTimers.TryGetValue(key, out var oldTimer))
            {
                oldTimer.Dispose();
            }

            var newTimer = new Timer(
                _ => OnQueuedOperationElapsed(key),
                null,
                TimeSpan.FromSeconds(5),
                Timeout.InfiniteTimeSpan);

            _queuedOperationTimers.AddOrUpdate(key, newTimer, (_, _) => newTimer);
            _queuedOperations.AddOrUpdate(key, (new[] { operation }, venue),
                (s, details) => 
                    (details.Operations.Union(new[] { operation }).ToArray(), details.Venue));
        }
    }

    private void OnQueuedOperationElapsed(string key)
    {
        if (_queuedOperationTimers.TryRemove(key, out var timer))
        {
            timer.Dispose();
        }

        var found = _queuedOperations.TryGetValue(key, out var details);
        if (!found)
            return;
        
        var (operations, venue) = details;

        foreach (var (observer, invocationKind) in _observers)
        {
            if (invocationKind != InvocationKind.Delayed) continue;
            foreach (var operation in operations)
            {
                if (!observer.Matches(operation, venue)) continue;
                observer.ObserverAction(operation, venue);
            }
        }
    }

}
