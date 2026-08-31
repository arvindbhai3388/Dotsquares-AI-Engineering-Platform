# Extract a Custom Hook from Duplicated Logic Across Components

**Category:** React
**Use when:** The same stateful logic (a fetch pattern, a subscription, a piece of derived state, form-field wiring) is copy-pasted or reimplemented slightly differently across two or more components.

## Prompt

First locate every place the duplicated logic appears and diff them carefully — propose the extracted hook's exact signature (inputs, return shape) based on what's actually common versus what genuinely differs per call site, before writing the hook. Do not force a shared hook onto logic that only looks similar on the surface but serves different purposes or has diverging edge-case handling between call sites; report that finding instead and propose extracting only the parts that are truly identical, or ask whether the divergent behavior should be unified first.

Design the hook to return either a single value (`useX(): T`), a tuple resembling `useState`'s convention (`const [value, setValue] = useDebouncedValue(input, delay)`) for a closely paired value/setter, or a small object for multiple distinct pieces (`const { data, isLoading, error } = useX()`) — pick whichever convention matches similar existing hooks in this codebase rather than introducing a new shape. Name it starting with `use` so the Rules of Hooks lint (`eslint-plugin-react-hooks`) can correctly track its hook calls. Keep any side effects (subscriptions, timers, event listeners, fetches) inside the hook with correct cleanup in `useEffect`'s return function, and make sure the hook's own dependency arrays are exhaustive rather than inheriting a suppressed lint warning from one of the original copies.

Parameterize genuinely varying behavior explicitly through the hook's arguments (including an options object for optional configuration) rather than baking in a value from one call site's context. If the different call sites need different loading/error semantics, expose the raw state and let each caller decide how to render it, rather than embedding call-site-specific UI logic inside the hook itself — a custom hook should carry logic, not JSX.

Update every duplicated call site to use the new hook, verifying each one's existing tests still pass with no behavior change (this is a refactor — the observable output at each call site should be identical unless a bug fix was explicitly intended and called out separately). Write tests for the hook itself using `@testing-library/react`'s `renderHook` (from `@testing-library/react`, not the deprecated standalone `@testing-library/react-hooks` package) covering its state transitions, cleanup behavior on unmount, and each parameter variation, so future changes to the shared hook are protected independently of the components that consume it.
