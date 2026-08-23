# Review Code for Thread-Safety Issues

**Category:** Code Review & Testing
**Use when:** A class is used from multiple concurrent requests/threads (e.g., a singleton service, a static cache).

## Prompt

Review the class/module I specify for thread-safety issues, given that it's used concurrently -- confirm first how it's actually consumed (registered as a singleton in DI, held as a static instance, accessed from a background worker alongside request threads, etc.) since the required fix depends on the real usage pattern, not just the code in isolation.

Check specifically for:

1. **Shared mutable state** -- instance or static fields that are read and written by multiple threads without synchronization (a `List<T>`/`Dictionary<K,V>` field mutated by concurrent calls, a mutable counter, a cached value that gets lazily computed and overwritten).
2. **Non-atomic compound operations** -- check-then-act patterns (`if (!dict.ContainsKey(k)) dict[k] = v`), increment operations (`counter++`) that look atomic but aren't, and lazy-initialization without proper double-checked locking or `Lazy<T>`.
3. **Non-thread-safe singletons** -- a service registered as a DI singleton that internally uses non-thread-safe collections or holds request-scoped state it shouldn't (e.g., capturing a value from one request and reusing it for another).
4. **Improper use of `DbContext`** -- confirm no single context instance is shared across concurrent operations; EF `DbContext` instances are not thread-safe and must be scoped per operation/request, never held as a singleton or static field.
5. **Locking correctness** -- locks taken on the wrong object (locking on a boxed value, a string literal, or `this` when external code can also lock on the same instance causing unrelated contention), lock ordering that could deadlock across two classes each locking two shared resources in different order, and locks held across `await` (which doesn't do what it looks like it does and commonly causes deadlocks in ASP.NET-style contexts).
6. **Signal misuse** -- events, `ManualResetEvent`/`SemaphoreSlim` used incorrectly (missing `Dispose`, signal set before wait registered causing a missed signal).

For each finding, explain the concrete race scenario with a timeline (thread A does X while thread B does Y, resulting in Z), not just "this isn't thread-safe" as an abstract claim. Propose the smallest correct fix -- prefer immutable state, `ConcurrentDictionary`/other concurrent collections, or narrowing the singleton's shared surface -- over introducing broad locking that could hurt throughput. Flag if a fix requires reintroducing per-request lifetime instead of a singleton, since that's a bigger DI registration change and needs confirmation before implementing.
