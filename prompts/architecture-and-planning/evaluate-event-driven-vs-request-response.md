# Evaluate Event-Driven vs. Request-Response for a New Integration

**Category:** Architecture & Planning
**Use when:** Designing a new integration and it's unclear which communication style fits the reliability/latency requirements.

## Prompt

Evaluate whether the new integration described below should be built as event-driven (queue/pub-sub/message broker) or synchronous request-response, and produce a recommendation document for me to review — do not implement either approach as part of this task.

Base the evaluation on the integration's actual requirements, not a default preference:

1. **Consistency requirements** — does the caller need to know the outcome immediately (favoring request-response), or is eventual consistency acceptable (favoring async/event-driven)? Identify what "acceptable delay" means concretely for this use case.
2. **Latency and caller experience** — what is the caller waiting on, and is it acceptable for that caller to block for the downstream system's full processing time, or does the work need to be handed off so the caller can return immediately?
3. **Failure handling** — what happens if the downstream system is slow, unavailable, or errors? Compare: request-response requires the caller to handle timeouts/retries/circuit-breaking synchronously and surfaces failure immediately; event-driven allows retry/dead-lettering/backoff without blocking the caller, but requires the caller to handle "accepted but not yet confirmed" state and needs a way to communicate eventual failure back if that matters.
4. **Ordering and duplicate delivery** — does processing order matter, and can the consumer tolerate at-least-once delivery (duplicate messages)? If so, note the idempotency requirement this places on the consumer.
5. **Coupling and scalability** — request-response couples the caller's availability to the downstream system's availability; event-driven decouples them but adds a broker as new infrastructure to operate, monitor, and secure. Weigh this against what's already available in the current stack (existing queue/broker infrastructure vs. introducing a new one).
6. **Observability and debuggability** — request-response is easier to trace end-to-end synchronously; event-driven requires correlation IDs and cross-system tracing to follow a single logical operation.
7. **Recommendation** — a clear choice with reasoning tied to points 1-6, plus a fallback/hybrid option if relevant (e.g., synchronous acceptance with async processing and a status-check endpoint).
8. **Risks and effort** — implementation effort for the recommended approach, and the biggest risk if the wrong style is chosen.

Wait for approval before building either approach.
