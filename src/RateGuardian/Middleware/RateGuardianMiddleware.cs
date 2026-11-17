using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RateGuardian.Storage;

namespace RateGuardian.Middleware;

public class RateGuardianMiddleware(IInMemStorage inMemStorage) : IMiddleware
{
    private const int RequestLimit = 5;
    private const int IntervalLimit = 5000;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var ipAddress = context.Connection.RemoteIpAddress ?? IPAddress.None;

        inMemStorage.TrackedRequests.TryAdd(ipAddress, []);

        var history = inMemStorage.TrackedRequests[ipAddress].Where(x => x > DateTimeOffset.Now.Subtract(TimeSpan.FromMilliseconds(IntervalLimit))).ToList();

        if (history.Count > RequestLimit)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded.");
            return;
        }

        inMemStorage.TrackedRequests[ipAddress].Add(DateTimeOffset.Now);

        await next.Invoke(context);
    }
}
