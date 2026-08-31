# Fix a Stale-Closure Bug in useEffect or useCallback

**Category:** React
**Use when:** A callback or effect appears to fire with an outdated prop/state value, an interval keeps using the value from when it started, or the exhaustive-deps ESLint rule is disabled to silence a warning.

## Prompt

Before changing anything, identify exactly which value is going stale and why: a value referenced inside `useEffect`/`useCallback`/`useMemo` is captured at the time that closure was created, so if the dependency array omits it (or the array is empty when it shouldn't be), the closure keeps using the value from the render where it was defined instead of the latest one. Check specifically for a disabled or suppressed `react-hooks/exhaustive-deps` warning (`// eslint-disable-next-line`) near the affected hook — that's usually the exact spot the bug was introduced. Report the specific stale variable and the render at which it was captured before proposing a fix, rather than just re-enabling the lint rule and papering over whatever errors appear.

Propose the correct fix for the specific pattern found, since "just add it to the array" is not always right:
- If the missing dependency is a prop/state value that should trigger a re-run when it changes, add it to the dependency array — but check whether that changes how often the effect legitimately needs to run (e.g. an interval that should reset vs. one that shouldn't).
- If the value only needs to be read at call time and shouldn't trigger a re-run (common with `setInterval`/`setTimeout` callbacks or event listeners that should be set up once), use the "functional update" form of `setState` (`setCount(c => c + 1)`) to avoid needing the value in the closure at all, or store the latest value in a `useRef` that's updated on every render and read from inside the effect/callback.
- If a `useCallback`/`useMemo` is memoizing a function/value based on an incomplete dependency array purely to keep referential stability, fix the dependency array first and only reach for `useRef`-based tricks if the real fix would cause a genuinely undesirable re-run cascade — explain the trade-off rather than defaulting to suppressing the warning.

Do not blanket-disable `exhaustive-deps` as the fix. If a dependency is intentionally and correctly omitted (rare), leave a comment explaining exactly why, scoped to that one line, not a project-wide rule override.

Write a test that reproduces the stale-closure bug before fixing it — typically by rendering the component, triggering a prop/state change, then asserting the effect/callback used the updated value (e.g. via a spy, a mocked timer with `vi.useFakeTimers()`/`jest.useFakeTimers()`, or asserting on a side effect like a function call argument) — confirm it fails for the right reason first, then confirm it passes after the fix.
