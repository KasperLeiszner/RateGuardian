using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RateGuardian.Storage;

namespace RateGuardian.Middleware;

public class IpWhitelistMiddleware : IMiddleware
{
    private readonly IInMemStorage _inMemStorage;
    private const int RequestLimit = 5;
    private const int IntervalLimit = 5000;

    public IpWhitelistMiddleware(IInMemStorage inMemStorage)
    {
        _inMemStorage = inMemStorage;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var ipAddress = context.Connection.RemoteIpAddress;

        _inMemStorage.TrackedRequests.TryAdd(ipAddress, new List<DateTimeOffset>());
        
        var history = _inMemStorage.TrackedRequests[ipAddress].Where(x => x > DateTimeOffset.Now.Subtract(TimeSpan.FromMilliseconds(IntervalLimit))).ToList();

        if (history.Count > RequestLimit)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded.");
            return;
        }

        _inMemStorage.TrackedRequests[ipAddress].Add(DateTimeOffset.Now);
        
        await next.Invoke(context);
    }
}