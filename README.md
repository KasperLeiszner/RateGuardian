# RateGuardian

A lightweight, per-IP rate limiting middleware for ASP.NET Core.

RateGuardian tracks requests per client IP address using a sliding window and rejects clients that exceed the limit with `429 Too Many Requests`.

> **Status: early prototype.** RateGuardian is a learning project and is not yet production-ready. See [Known limitations](#known-limitations) and the [Roadmap](#roadmap).

## How it works

1. Each incoming request is keyed by `HttpContext.Connection.RemoteIpAddress`.
2. RateGuardian looks at that IP's request timestamps within the current window.
3. If the IP is over the limit, the request is short-circuited with `429` and the body `Rate limit exceeded.`
4. Otherwise the request is recorded and passed on to the next middleware.

Request history is held in memory in a singleton store, so limits apply per application instance.

| Setting | Current value |
|---|---|
| Algorithm | Sliding window log |
| Partition key | Client IP address |
| Window | 5 seconds |
| Limit | 5 requests per window |
| Storage | In-memory |

Limits are currently hard-coded in `IpWhitelistMiddleware`.

## Getting started

RateGuardian is not published to NuGet yet. Reference the project directly:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/RateGuardian/RateGuardian.csproj" />
</ItemGroup>
```

Register the services and add the middleware in `Program.cs`:

```csharp
using RateGuardian;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateGuardian();

var app = builder.Build();

app.UseRateGuardian();

app.MapGet("/", () => "Hello from behind the guard");

app.Run();
```

Add `UseRateGuardian()` early in the pipeline so rejected requests skip the rest of the app.

## Try it

Run the sample project:

```bash
dotnet run --project RateGuardian/SampleProject
```

Then send a burst of requests (use the port shown in the console output):

```bash
for i in $(seq 1 10); do curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5000/; done
```

The first requests return `200`; once the limit is hit, you'll see `429` until the window passes.

## Project structure

```
RateGuardian/
├── RateGuardian/             # The library
│   ├── InstallationExtension.cs   # AddRateGuardian() / UseRateGuardian()
│   ├── Middleware/                # Rate limiting middleware
│   └── Storage/                   # In-memory request store
├── SampleProject/            # Minimal API demonstrating usage
└── UnitTests/                # Test project (no tests yet)
```

## Requirements

- .NET 7 runtime (or a newer one with roll-forward, see below)

.NET 7 is out of support. If you only have a newer runtime installed, run the sample with:

```bash
DOTNET_ROLL_FORWARD=Major dotnet run --project RateGuardian/SampleProject
```

## Known limitations

Being honest about where the prototype falls short:

- **Not thread-safe per client.** Concurrent requests from the same IP share a `List<DateTimeOffset>`, which can throw or let extra requests through.
- **Unbounded memory.** Old timestamps and idle IPs are never evicted.
- **Off-by-one.** The check `Count > RequestLimit` allows 6 requests per window instead of 5.
- **Null IP addresses** (e.g. `TestServer`, Unix sockets) cause an exception.
- **Behind a reverse proxy** all clients share the proxy's IP unless `UseForwardedHeaders()` is configured.
- **Single instance only.** In-memory storage doesn't share limits across multiple app instances.
- **No configuration.** Limits, window and partition key are hard-coded.
- **No `Retry-After` header** on rejected responses.

## Roadmap

- [ ] Upgrade to .NET 10, add tests and CI
- [ ] Fix concurrency, the off-by-one and null-IP handling
- [ ] Evict expired timestamps and idle clients
- [ ] Configuration via the options pattern and `appsettings.json`
- [ ] Pluggable partition keys (IP, header, claim)
- [ ] Multiple algorithms: fixed window, sliding window counter, token bucket
- [ ] Named per-endpoint policies (`.RequireRateGuard("strict")`)
- [ ] `Retry-After` and `RateLimit` response headers, ProblemDetails body
- [ ] Metrics via `System.Diagnostics.Metrics` / OpenTelemetry
- [ ] Redis-backed distributed store
- [ ] Publish to NuGet

## Alternatives

For production workloads today, consider ASP.NET Core's built-in rate limiting middleware ([`Microsoft.AspNetCore.RateLimiting`](https://learn.microsoft.com/aspnet/core/performance/rate-limit)).
