# Add Polly Retry Policy for Graph API Throttling

**Category:** SharePoint (Microsoft Graph)
**Use when:** A Graph integration under load is failing intermittently due to throttling.

## Prompt

Our SharePoint/Graph integration is failing intermittently under load with 429 (Too Many Requests) and occasional 503 (Service Unavailable) responses. Add a Polly-based retry policy that correctly respects Graph's `Retry-After` header instead of using a fixed or naive exponential backoff that ignores server guidance.

Requirements:
- Locate how HTTP resilience is currently handled elsewhere in this codebase (existing `HttpClient`/Polly registrations, `IHttpClientFactory` policies) and match that pattern rather than introducing a second, inconsistent retry mechanism.
- Implement the policy so that when a Graph response includes a `Retry-After` header (present on both 429 and 503), the retry waits exactly that duration rather than a policy-computed backoff; fall back to exponential backoff with jitter only when the header is absent.
- Scope the retry to transient/throttling status codes only (429, 503, 504) — do not retry on 4xx client errors like 400, 401, or 403, since retrying those wastes quota and hides real bugs (e.g., a permissions problem should surface immediately, not retry silently).
- Cap the number of retry attempts and total elapsed time so a persistently throttled operation fails fast with a clear exception rather than hanging a request or worker thread indefinitely.
- If the Graph SDK's built-in `RetryHandler` is already in the default `GraphServiceClient` pipeline, decide explicitly whether to rely on it, replace it, or layer Polly around the outer call — do not silently stack two uncoordinated retry mechanisms that could multiply delays.
- Log each retry attempt (attempt number, wait duration, status code) at an appropriate level without logging full request/response bodies that might contain sensitive SharePoint content.
- Make the policy configurable (max retries, max total wait) via this app's existing options pattern, not hardcoded magic numbers.
- Ensure the policy plays correctly with cancellation tokens — an in-progress retry wait must honor a passed `CancellationToken` and stop promptly.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose where the policy is registered (DI, `IHttpClientFactory`, or a Graph `RequestAdapter` handler) and its parameters first, then implement with tests simulating 429-with-Retry-After, 503-without-header, and non-retryable 403 responses using a fake handler (no live tenant calls).
