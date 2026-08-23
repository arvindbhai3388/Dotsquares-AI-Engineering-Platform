# Add Graceful Shutdown Handling to a Hosted Service

**Category:** ASP.NET Core
**Use when:** a worker gets killed mid-task during deploys or scale-downs.

## Prompt

Analyze the hosted service/worker I specify: what unit of work it processes (a queue item, a batch job, a long-running scan), how long a single unit typically takes, whether partial completion of a unit leaves data in an inconsistent state, and the current shutdown behavior — does it check `CancellationToken`/`stoppingToken` at all today, or does it get hard-killed by the host/orchestrator mid-operation.

Propose the shutdown design before implementing: how `IHostApplicationLifetime.ApplicationStopping` or the `stoppingToken` passed to `ExecuteAsync` should be observed — at what granularity (between items in a loop is usually sufficient; mid-item cancellation is only safe if the unit of work is itself cancellable/transactional), what "graceful" means for the current in-flight unit (finish it and then stop taking new work, versus abort cleanly and let the item be reprocessed/dead-lettered later), and the shutdown timeout budget (confirm the value configured via `HostOptions.ShutdownTimeout` or the orchestrator's grace period, so the worker's cleanup logic fits inside it rather than getting force-killed anyway).

Once approved, implement:
- Check the cancellation token at safe checkpoints (start/end of each item, not mid-database-write) and stop pulling new work once cancellation is requested.
- If the current item must finish before stopping, bound that with a timeout so shutdown doesn't hang indefinitely past the host's grace period.
- Implement `StopAsync` override only if custom cleanup beyond token-based cancellation is needed (flushing buffers, closing connections), and keep it fast.
- Ensure any resources acquired (locks, leases, queue message visibility timeouts) are released or safely handed back if a unit is aborted mid-flight, so it doesn't get stuck or duplicated incorrectly.
- Log shutdown start, items still in flight, and completion, so a deploy-time restart is diagnosable from logs.

Write or update tests simulating cancellation mid-loop (trigger the token between iterations) confirming: no new work is picked up after cancellation, an in-flight item either completes or is safely rolled back/requeued per the agreed behavior, and no resource leak occurs. Confirm with me on the exact shutdown timeout budget and requeue-versus-drop behavior before changing a worker already running in production.
