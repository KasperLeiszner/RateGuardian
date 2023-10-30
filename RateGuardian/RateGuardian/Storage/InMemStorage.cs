using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace RateGuardian.Storage;

public class InMemStorage : IInMemStorage
{
    public ConcurrentDictionary<IPAddress, List<DateTimeOffset>> TrackedRequests { get; } = new();
}