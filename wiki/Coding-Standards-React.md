# React Standards

Standards for React frontends built against this platform's most common
backend pattern, an ASP.NET Core Web API on a separate origin. React is this
platform's first non-.NET/JavaScript stack — these standards assume modern
React (18+, function components and hooks) with TypeScript as the default
language choice; a project standardized on plain JavaScript should keep that
consistency rather than adopting TypeScript mid-project without a deliberate
decision to do so.

## Component design

- **Single responsibility per component.** A component that renders a table,
  owns its own filtering state, calls the API, and manages a modal is four
  components pretending to be one — split them.
- Favor small, composable components over large "page" components with
  hundreds of lines of JSX. A page/route component should mostly orchestrate
  child components and hold page-level state, not contain deeply nested
  markup itself.
- Type every prop with an explicit interface/type; avoid `any`. Treat props
  as input-only — a child component should never mutate a prop value and
  expect the parent to notice; use a callback prop instead.
- Use a callback prop (`onSomething: (value: T) => void`) for child-to-parent
  communication, not a mutable object the child mutates in place. For a
  controlled-input-style component, pair a `value` prop with an
  `onChange`/`onValueChange` callback rather than letting the component keep
  its own shadow copy of a value the parent also owns.
- Prefer **composition** (`children`, or passing an already-configured child
  element down) over threading a prop through several components that don't
  themselves use it, only forward it further down the tree.
- Reach for **Context** only for values genuinely needed by a whole subtree
  (current theme, current authenticated user, a feature-flag set) — not as a
  substitute for two or three levels of explicit props, and not as a
  general-purpose store. Every consumer of a Context re-renders when its
  value changes, so keep fast-changing state (a text input's live value, a
  per-keystroke search term) out of a broadly-consumed Context.
- Controlled inputs (React state drives `value`, `onChange` updates it) are
  the default for form fields, keeping one source of truth for validation
  and formatting. Use an uncontrolled input (`ref` + `defaultValue`) only for
  a deliberate reason — large forms managed by a library like
  `react-hook-form` for performance, or inputs that are inherently
  uncontrolled (`<input type="file">`). Never let an input flip between
  controlled and uncontrolled across its own lifetime (a `value` that is
  sometimes `undefined` and sometimes a string) — React warns because it
  produces genuinely inconsistent behavior.

## State management — decision criteria

| State kind | Use | Not |
|---|---|---|
| Simple, independent local UI state (a dropdown's open/closed flag) | `useState` | A global store for something no other component needs |
| Several related values that change together via defined transitions (multi-step form, wizard) | `useReducer` | Several entangled `useState` calls updated in lockstep from multiple places |
| Data the server owns (a list of records, a user profile fetched from the API) | TanStack Query or SWR | `useState` + manual `useEffect` fetch/loading/error triples |
| Value needed by a whole feature subtree (current step of a wizard, current theme) | `CascadingValue`-equivalent: React Context, or a small state-container hook | Prop-drilling through components that don't use the value themselves |
| Value genuinely global to the session (current user, app-wide feature flags) | A small, well-known number of Context providers or a lightweight external store (Zustand) if Context alone becomes unwieldy | One ad hoc global store per feature |

- Don't add Redux Toolkit, Zustand, or any comparable library to an app whose
  state is a handful of independent `useState` calls plus server data already
  owned by TanStack Query/SWR — that is over-engineering for the problem
  size. Introduce one only when state is genuinely shared across many
  distant parts of the tree and Context-plus-hooks has become unwieldy
  (excessive re-renders on unrelated updates, tangled cross-feature prop
  plumbing).
- Treat **server state** (what the API currently reports) and **client UI
  state** (what a form currently has typed into it, unsaved) as two
  different categories that usually should not live in the same store —
  TanStack Query/SWR owns the former; `useState`/`useReducer` owns the
  latter.

## Hooks rules and pitfalls

- **`useEffect` dependency arrays must be complete.** List every reactive
  value the effect body reads (props, state, functions/values defined in the
  component). An incomplete dependency array is the most common source of
  stale-closure bugs — the effect keeps acting on a value captured from an
  earlier render. Let the `exhaustive-deps` lint rule drive this instead of
  suppressing it.
- **Clean up every subscription.** Any effect that adds an event listener,
  starts a `setInterval`/`setTimeout`, opens a WebSocket/SignalR connection,
  or creates an `AbortController` must return a cleanup function that
  reverses it — an effect without cleanup that re-runs on dependency changes
  silently accumulates subscriptions.
- **Don't use an effect to compute derived state.** A filtered list, a
  formatted string, a sum — compute it directly in the render body (or wrap
  in `useMemo` only if the computation is measurably expensive). An effect
  that calls `setState` from values already available during render costs an
  extra render pass and is a sign the value belongs in the render body, not
  in an effect.
- **Don't use an effect to react to a user action.** When a click handler
  changes state and an effect then "reacts" to that state change to do more
  work, the causality is harder to follow than simply doing the follow-up
  work directly inside the click handler that caused it.
- **Stale closures** show up beyond effects too — in callbacks passed to
  children, timers, or promise `.then` handlers that reference a
  prop/state value from the render where the function was created. Fix with
  the functional state-updater form (`setCount(c => c + 1)`) when the next
  value depends on the previous one, a complete dependency array, or a ref
  when the latest value is needed without triggering a re-subscription.
- **List `key`s must be stable and real.** Use an actual unique ID from the
  data, never the array index, for any list that can be reordered, filtered,
  or have items inserted/removed — an index key causes React to misattribute
  component state and DOM nodes across re-renders (a common symptom: a typed
  value or focus state "jumps" to the wrong row after a delete).
- **Memoization is not free.** `useMemo`, `useCallback`, and `React.memo`
  each carry their own comparison/memory cost and only pay off when a
  measured re-render is actually expensive, or when a stable reference is
  required for correctness (e.g., satisfying another hook's dependency
  array, or an item inside a `React.memo`-wrapped list). Applying them
  defensively everywhere adds complexity and can itself introduce bugs (a
  stale value trapped in a `useCallback` with a wrong dependency array) —
  profile before optimizing, and optimize the component that is actually
  slow.

## Testing philosophy

- Test **user-visible behavior**, not implementation details. Render the
  component with React Testing Library, query by accessible role/label/text
  (`getByRole`, `getByLabelText`), simulate real interactions with
  `userEvent` (not `fireEvent`, which skips intermediate browser events),
  and assert on the rendered output or on a callback prop having been
  called with the expected argument.
- Do **not** reach into a component's internal state, call an internal
  function directly, or count render invocations to make assertions — that
  is enzyme-era shallow-rendering-style testing; it couples tests to
  implementation and breaks on harmless refactors that don't change what a
  user actually sees or can do.
- For components depending on server state (TanStack Query/SWR), test
  through the same provider the app uses and mock the network layer (e.g.,
  MSW) rather than mocking the query hook — this exercises the real
  loading/error/success transitions instead of a hand-simulated shortcut.
- Cover, at minimum, for any component with meaningful logic: the initial
  render, a prop change producing the expected updated output, and a user
  interaction triggering the expected callback or state change.

## Security

- **`dangerouslySetInnerHTML` is the one path that bypasses JSX's automatic
  escaping.** Ordinary JSX text interpolation (`{value}`) is always escaped
  and is not an XSS vector on its own. Any value passed to
  `dangerouslySetInnerHTML` must go through a real sanitizer (e.g.,
  DOMPurify) immediately before use; never pass user-supplied or third-party
  content to it directly, and prefer avoiding it entirely when the same
  result can be achieved with normal JSX composition.
- **Client-exposed environment variables are public.** Any variable behind a
  bundler's client-exposure prefix (`VITE_*` in Vite, `REACT_APP_*` in
  Create React App, `NEXT_PUBLIC_*` in Next.js) is compiled into the
  JavaScript bundle shipped to every browser and is trivially readable by
  anyone — never place a real API key, connection string, or other secret
  behind one of these prefixes. Secrets stay server-side on the ASP.NET Core
  API; the client only ever holds a short-lived token issued to it.
- Treat the ASP.NET Core API as a separate origin by default — a request
  that fails only in the browser with a CORS error is a server-side CORS
  configuration gap (`AddCors`/`UseCors`), not something to work around from
  the client by disabling credentials or widening `AllowAnyOrigin`; combining
  `AllowAnyOrigin` with credentials is both invalid in browsers and a real
  security regression if it were possible.
- Never trust data echoed back from the client (a client-computed
  authorization flag, a client-supplied ID) for a security decision made on
  the server — the API must re-derive authorization from its own
  session/token, exactly as it would for any other client.

## Related pages

- [Architecture Overview](Architecture-Overview.md)
- [Onboarding Guide](Onboarding-Guide.md)
