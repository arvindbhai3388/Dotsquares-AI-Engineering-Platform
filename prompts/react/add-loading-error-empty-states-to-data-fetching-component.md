# Add Loading, Error, and Empty States to a Data-Fetching Component

**Category:** React
**Use when:** A component renders a blank screen, an unhandled crash, or nothing distinguishable while its data is loading, missing, or failed to load.

## Prompt

Read the component's current data-fetch path first — whether it's a raw `useEffect` + `fetch`, a TanStack Query/SWR hook, or something else — and identify every state it currently fails to represent distinctly: initial loading, empty result set, fetch error, and a re-fetch triggered by a changed parameter while old data is still on screen. Propose the state model before implementing (if using TanStack Query/SWR, this is mostly already exposed via `isLoading`/`isFetching`/`isError`/`data`; if using manual `useState`, propose the explicit state shape — e.g. a `status: 'idle' | 'loading' | 'success' | 'error'` field rather than independent booleans that can contradict each other).

Render four distinct branches: a loading state (skeleton or spinner — not a blank `null` return), an error state that surfaces a user-facing message (never the raw error object, stack trace, or Axios/fetch error internals, which may leak backend details) plus a retry action where feasible, a success state with data, and an explicit empty state when the fetch succeeds but returns zero items — "success with zero items" and "still loading" must not look identical to the user. If using manual `fetch`, ensure a non-2xx response is actually treated as an error (checking `response.ok` before parsing the body) rather than only catching network-level failures.

Guard against race conditions when the fetch can be re-triggered before the previous one resolves (e.g. a search box re-fetching per keystroke, or a changed route param): use an `AbortController` for `fetch`, or check that the effect's cleanup function ignores a resolved promise from a stale invocation (e.g. a local `let ignore = false` flag set to `true` in the cleanup) so a slow earlier response cannot overwrite a newer one's result. If already using TanStack Query/SWR, confirm the query key encodes the changing parameter so the library handles this natively instead of adding a manual guard on top.

Write React Testing Library tests covering: initial loading render, successful render with data, empty-result render, error render with a mocked rejected/non-2xx response, and — if the race condition applies here — a test asserting a stale in-flight request doesn't clobber a newer one's rendered result. Use `findBy*` queries (which wait for async updates) rather than wrapping every assertion in manual `waitFor` calls.
