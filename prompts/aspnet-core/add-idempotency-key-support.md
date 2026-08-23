# Add Idempotency Key Support to a Write Endpoint

**Category:** ASP.NET Core
**Use when:** client or gateway retries could cause duplicate side effects (e.g., duplicate charges/records).

## Prompt

Analyze the POST/PUT endpoint I specify: what side effect it performs (record creation, external API call, payment/charge, notification send), whether it's already naturally idempotent (e.g., an upsert keyed on a natural unique field) or genuinely at risk of duplication on retry, and what storage is available/appropriate for tracking idempotency keys (existing database, distributed cache like Redis, or an in-memory store — flag if in-memory is not viable because the app runs on multiple instances).

Propose the design before implementing: the idempotency key source (a client-supplied `Idempotency-Key` header is the common convention — confirm this matches or should establish the project's convention), the storage schema for tracking seen keys (key, request hash or fingerprint, response status/body, timestamp, expiry), the behavior on a repeat request with the same key — return the original cached response without re-executing the side effect — versus a same-key-different-payload request, which should be rejected as a conflict (409) rather than silently executing either version, and the retention/expiry window for stored keys (they shouldn't live forever).

Once approved, implement:
- Add middleware or an action filter that reads the idempotency key header, looks up prior results before invoking the handler, and short-circuits with the stored response if found.
- Store the result only after the operation completes successfully, keyed with an expiry (e.g., 24 hours, confirm the actual value with me), using parameterized queries/proper cache APIs — never construct storage keys directly from unvalidated input without sanitization.
- Guard against a race where two concurrent requests with the same key both pass the "not seen yet" check — use a database unique constraint, a distributed lock, or an atomic cache `SETNX`-style operation rather than a plain read-then-write.
- Return a clear error (400) if the header is required but missing, if that's the agreed contract.

Write or update tests covering: first request executes the side effect once, a retried request with the same key returns the cached result without re-executing, concurrent duplicate requests only execute the side effect once, and a same-key/different-body request is rejected. Confirm with me before applying this pattern to any endpoint already live, since it changes client-visible retry semantics.
