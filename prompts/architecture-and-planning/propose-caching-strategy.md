# Propose a Caching Strategy for a Read-Heavy Feature

**Category:** Architecture & Planning
**Use when:** A feature's read load needs to be reduced but the right caching layer/invalidation approach isn't obvious.

## Prompt

Propose a caching strategy for the read-heavy feature described below. This is a design proposal only — do not implement any caching code yet; produce a document for me to review and approve first.

Start by reading the current implementation of the feature's read path (controller/service/repository) and its write path(s), so the proposal is grounded in the real data-access pattern and update frequency, not assumptions.

In your proposal, cover:

1. **Read/write characteristics** — current read volume/frequency versus write frequency for the underlying data, how stale a cached value is allowed to be for this feature, and whether results vary per user/tenant (affecting cache key design).
2. **Caching layer options** — evaluate in-memory caching (e.g., `IMemoryCache`) versus a distributed cache (e.g., Redis) versus HTTP/output caching versus a materialized/denormalized read table, with an explicit recommendation and why, considering that this is a multi-instance deployment (so in-memory caching is per-instance and can serve stale/inconsistent data across nodes unless that's acceptable).
3. **Cache key design** — what the key is composed of (including tenant/user scoping if relevant) and how you avoid key collisions or unbounded key growth.
4. **Invalidation strategy** — explicit invalidation on write (and exactly which write paths must trigger it), time-based expiration (TTL) as a backstop, or a hybrid; state the trade-off between staleness and cache hit rate for the chosen approach.
5. **Failure behavior** — what happens on a cache-layer outage (fail open to the database vs. fail closed) and the added load on the database if the cache is cold or unavailable.
6. **Observability** — what you would log/metric (hit rate, eviction count, stale-serve incidents) to validate the strategy is working post-launch.
7. **Alternatives considered and rejected** — briefly, with reasons.
8. **Effort estimate and rollout** — implementation size, and whether this can be introduced behind a flag with a safe fallback to the uncached path.

Wait for my approval of the chosen approach before writing any implementation code.
