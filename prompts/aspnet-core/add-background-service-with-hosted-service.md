# Add a BackgroundService for Periodic or Queued Work

**Category:** ASP.NET Core
**Use when:** a task needs to run on a schedule or drain a queue in-process.

## Prompt

Analyze the work I need running in the background: is it periodic on a fixed interval, triggered by items arriving on a queue (in-memory `Channel<T>`, or an external queue like Service Bus/RabbitMQ), and what it depends on (database, external API, other scoped services). Check whether any `BackgroundService`/`IHostedService` implementations already exist in this project whose patterns (logging, error handling, DI scope creation) should be followed for consistency.

Propose the design before implementing: whether this is a `BackgroundService` with a `PeriodicTimer`-driven loop, or a queue-processing worker consuming from a `Channel<T>`/external broker; how it creates a DI scope per unit of work via `IServiceScopeFactory` (a `BackgroundService` is a singleton, so it must never hold a scoped `DbContext` or scoped service directly — every unit of work needs its own scope); the failure-handling strategy per item/tick (log and continue vs. retry with backoff vs. dead-letter after N attempts); and how the service responds to `CancellationToken` during shutdown — no work should be left in an inconsistent state.

Once approved, implement:
- Inherit `BackgroundService` and implement `ExecuteAsync(CancellationToken stoppingToken)`, respecting `stoppingToken` in every await and loop condition so the host can shut it down promptly.
- Create a new DI scope (`IServiceScopeFactory.CreateScope()`) per iteration/item for any scoped dependency; never inject a scoped service directly into the hosted service's constructor.
- Wrap each unit of work in its own try/catch so one failure doesn't crash the entire background loop (an unhandled exception in `ExecuteAsync` stops the host by default) — log the failure with enough context to diagnose it, and never crash the whole worker on a single bad item unless that's genuinely the desired behavior.
- Register it via `AddHostedService<T>()`.
- Consider using `IHostApplicationLifetime` if the service needs to react to application stopping/stopped events specifically.

Write or update tests exercising the core work logic extracted into a testable method/service (not the `ExecuteAsync` loop itself, which is hard to unit test directly), covering the success, transient-failure-retry, and permanent-failure paths. Confirm with me before changing the polling interval or concurrency of any background service already running in production.
