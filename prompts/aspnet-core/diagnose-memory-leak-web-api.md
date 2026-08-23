# Diagnose a Suspected Memory Leak in a Long-Running Web API

**Category:** ASP.NET Core
**Use when:** memory usage climbs steadily under load and doesn't recover after GC.

## Prompt

Analyze the service for the usual ASP.NET Core memory-leak culprits before touching any code: static or singleton-scoped fields/collections that grow unbounded (caches without eviction, event handler subscriptions that are never unsubscribed, static `List<T>`/`Dictionary<T>` accumulating entries per request); `HttpClient` instances created per-request instead of via `IHttpClientFactory` (socket exhaustion presents similarly to a leak); `IDisposable` resources (`DbContext`, streams, `SqlConnection`, `HttpResponseMessage`) not wrapped in `using`/`await using` or not disposed on exception paths; scoped/transient services incorrectly captured by a singleton (captive dependency) keeping request-scoped data alive far longer than a single request; and large object heap fragmentation from big buffers/arrays allocated per request without pooling (`ArrayPool<T>`/`RecyclableMemoryStream` where the project already uses them).

Report findings before proposing a fix: identify the specific suspect(s) with file/line references, and for each, explain the mechanism by which it retains memory (not just "this looks suspicious") — e.g., "this static `ConcurrentDictionary` is keyed by request ID and never removes entries" is actionable; "this class holds a lot of data" is not. If the leak can't be pinned down from static analysis alone, propose a diagnostic plan: capturing a memory dump under load (`dotnet-dump`/`dotnet-gcdump`) at two points in time and diffing retained object counts/types, or adding temporary instrumentation (`GC.GetTotalMemory`, `EventCounters`) to narrow down which subsystem is growing, and ask me for access to the environment/dump before assuming you can reproduce it locally.

Once the root cause is confirmed (not just suspected) and I approve a fix:
- Make the smallest correct change — add proper disposal, bound the growing collection with eviction/expiry, fix the captive-dependency lifetime mismatch, or switch to `IHttpClientFactory`.
- Do not restructure unrelated code while fixing this.

Write or update a test that reproduces the retention pattern where feasible (e.g., asserting a cache evicts entries past its bound, or that a disposable is disposed via a spy/mock), and describe how you'd verify the fix under real load (dump diff before/after) since a unit test alone often can't prove a leak is gone. Confirm with me before running any load test or memory dump capture against a production or shared environment.
