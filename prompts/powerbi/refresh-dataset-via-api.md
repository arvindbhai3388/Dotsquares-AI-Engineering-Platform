# Trigger and Monitor Dataset Refresh via REST API

**Category:** Power BI
**Use when:** Report data needs to be kept current on a schedule beyond the built-in scheduled refresh limits.

## Prompt

Implement a .NET background job that triggers a Power BI dataset refresh via the REST API and monitors it to completion, for cases where the built-in scheduled refresh (limited to 8/day on Pro, 48/day on Premium) isn't sufficient. Start by locating this app's existing background job infrastructure (hosted service, worker, scheduled task runner) and reuse it -- do not introduce a new job scheduling library if one is already present.

Design and implement the following flow, proposing it for approval before writing code:

1. **Trigger:** Authenticate via the service principal already used for embedding (reuse the existing AAD/MSAL auth code path rather than duplicating it), then call `POST /v1.0/myorg/groups/{groupId}/datasets/{datasetId}/refreshes`. Support an optional `notifyOption` and, if the dataset supports it, partial refresh via `objects` for large incremental-refresh-enabled datasets rather than always forcing a full refresh.
2. **Monitor:** Poll `GET /v1.0/myorg/groups/{groupId}/datasets/{datasetId}/refreshes` (or the specific refresh ID endpoint) on a backoff interval (e.g. start at 10-15 seconds, back off for long-running refreshes) until status is "Completed" or "Failed" -- do not tight-poll every second, which risks throttling (429) on top of wasting API calls.
3. **Failure handling:** On "Failed" status, retrieve the refresh's error detail (`serviceExceptionJson`) and log it (redacting any connection-string or credential fragments that might appear in the error payload), then raise an alert/notification through whatever mechanism this app already uses (email, logging pipeline, monitoring dashboard) -- do not silently swallow failures.
4. **Timeout guard:** Cap total wait time (e.g. 2 hours) and treat an unresolved refresh as a distinct "timed out" outcome from an explicit "Failed" status, since large datasets can legitimately run long and you don't want false-positive failure alerts.
5. **Throttling:** Handle 429 responses from the refresh-trigger and status-poll calls with retry-with-backoff (see the dedicated throttling prompt if this app doesn't already have a shared retry policy) rather than failing the job outright on a transient throttle.

Write tests for the trigger/poll/failure-handling logic using a mocked HTTP client per this project's existing test conventions, covering success, failure, timeout, and throttled-retry paths.
