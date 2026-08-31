# Diagnose and Fix Unnecessary Re-Renders

**Category:** React
**Use when:** A component tree feels sluggish, React DevTools Profiler shows components re-rendering with unchanged output, or typing in an input causes visible lag elsewhere on the page.

## Prompt

Before changing any code, ask me to record a React DevTools Profiler session (or supply an existing one) around the interaction that feels slow, and analyze it with me: which components re-rendered, how many times, and — using the "why did this render" info the Profiler exposes, or by reasoning about props/context — whether each re-render actually changed what's on screen. Do not propose `memo`/`useMemo`/`useCallback` changes based on a hunch; a component re-rendering is not itself the problem, an expensive re-render with no visible change is. Report the specific components and the specific cause for each (e.g. "this list item re-renders every keystroke because it consumes a context object that's recreated every render of the provider") before proposing fixes.

Common root causes to check for in this order:
1. **New object/array/function identity created every render** and passed as a prop or context value — e.g. `<Child options={{ foo: 1 }} />` or `<Provider value={{ user, setUser }}>` recreate their argument every render, defeating any memoization downstream. Fix by hoisting stable literals outside the component, or wrapping the object/function in `useMemo`/`useCallback` with a correct dependency array.
2. **Context consumers re-rendering on unrelated state changes** — if a context value bundles frequently-changing state with rarely-changing state, every consumer re-renders on every change to either. Fix by splitting into separate contexts, or moving the frequently-changing piece to local/state-colocated instead of context.
3. **Missing `React.memo` on a pure child that re-renders purely because its parent did**, with props that are actually unchanged (primitives, or objects now stabilized per point 1). Only add `memo` once prop identity is actually stable — wrapping a component whose props are recreated every render in `memo` adds overhead with zero benefit.
4. **State colocated too high** — state that only affects one deeply-nested subtree living in a top-level component, causing everything between the two to re-render on every update. Fix by moving the state down to the closest common ancestor that actually needs it, or extracting the stateful piece into its own component so the re-render is scoped to it.
5. **Expensive computation re-run every render** unrelated to re-render count per se — wrap in `useMemo` only when profiling actually shows the computation (not the render) as the cost.

Implement only the fix(es) that match a confirmed cause, not a blanket sweep of `memo`/`useMemo`/`useCallback` across the tree — over-memoization adds cognitive overhead and can itself cause bugs (stale closures — see that prompt in this category) without measurable benefit. After each fix, re-profile the same interaction and report the before/after render counts and durations rather than asserting it's faster. Add a regression note or test where feasible (e.g. a test asserting a memoized child does not re-render when an unrelated parent state changes, using a render-count spy).
