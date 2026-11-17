using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace RateGuardian.Storage;

public interface IInMemStorage
{
    ConcurrentDictionary<IPAddress, List<DateTimeOffset>> TrackedRequests { get; }
}