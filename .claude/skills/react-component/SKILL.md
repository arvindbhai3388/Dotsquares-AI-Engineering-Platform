---
name: react-component
description: >
  Use to scaffold a new React component correctly end to end — typed props,
  the component itself, and a React Testing Library test. Trigger phrases:
  "create a new React component", "scaffold a component for X", "add a
  reusable component". For general fixes/changes to existing components,
  prefer the react-developer agent; use this skill specifically when
  standing up a brand-new component from nothing.
---

# React Component Scaffolding Workflow

*Can be approved with a single Yes/No that carries this through to completion instead of a
per-step check-in — see `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" section.*

A new component should be usable, testable, and correctly typed from the
moment it's created — this skill walks through each of those concerns
explicitly rather than leaving them to be discovered later.

## Step 1 — Confirm dependencies and boundaries

- Confirm the project's actual setup before writing anything: TypeScript or
  plain JavaScript (`tsconfig.json` present? `typescript` in
  `package.json`?), the bundler (Vite/CRA/Next.js), and the test runner
  (Jest or Vitest) plus React Testing Library version already installed —
  don't assume any of these, and don't add TypeScript or a different test
  runner to a project that has already standardized on something else.
- Define the component's single responsibility before writing markup — if
  it renders a list, manages a modal, and calls an API, that's three
  components pretending to be one; split it now rather than after it's
  grown further.
- Decide what's a **prop** (data/config passed in by the parent), what's
  **owned state** (internal to the component via `useState`/`useReducer`),
  and what needs to **bubble up** via a callback prop — get this boundary
  right before writing code; retrofitting it later touches every call site.

## Step 2 — Define the props contract

```tsx
export interface ConfirmButtonProps {
  label: string;
  disabled?: boolean;
  onConfirm: (value: string) => void;
  children?: React.ReactNode;
}
```

- Type every prop explicitly; avoid `any`. Mark genuinely optional props
  with `?` rather than requiring the caller to pass everything.
- Name a callback prop for what it communicates (`onConfirm`, `onSelect`),
  not for the DOM event that triggers it (`onClick`) unless the component
  is a thin, generic wrapper where that's actually accurate.
- For a controlled input-style component intended for two-way use, pair a
  `value` prop with an `onChange`/`onValueChange` callback that receives
  the new value — don't have the component silently manage its own copy of
  a value the parent also thinks it owns (that's the two-controllers bug:
  the input appears to ignore the parent's updates, or reverts unexpectedly).
- Use `React.ReactNode` (`children`, or a named prop) for content
  injection rather than a growing list of boolean/string props trying to
  cover every visual variant.

## Step 3 — Implement the component

- Function component with hooks — no class components.
- One-time or derived setup: compute derived values directly during render
  (or `useMemo` if genuinely expensive) rather than pushing them into
  `useEffect` + extra state.
- Any subscription (event listener, timer, WebSocket/SignalR client) needs
  a cleanup function returned from its `useEffect` — add it in the same
  step as the subscription, not as an afterthought.
- If the component calls the ASP.NET Core API directly rather than
  through TanStack Query/SWR, model `loading`/`error`/`data` explicitly —
  don't leave a fetch with no loading or error UI path.
- Keep JSX readable: push non-trivial conditional/formatting logic into a
  named local function or variable above the `return`, not inline in the
  markup.

## Step 4 — Write the test (React Testing Library)

Cover, at minimum:

- **Initial render** with typical props — assert on visible text/roles via
  `getByRole`/`getByLabelText`/`getByText`, not on internal component
  state or by reaching into implementation details.
- **A prop change** — re-render with `rerender(<Component {...newProps} />)`
  from `@testing-library/react` and assert the output updates as expected.
- **A user interaction** (click/input) — use `userEvent` (not `fireEvent`)
  to simulate the real event sequence, and assert the expected callback
  prop fired with the expected argument (`expect(onConfirm).toHaveBeenCalledWith(...)`)
  or that the resulting rendered output changed.

```tsx
import { render, screen, rerender } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ConfirmButton } from "./ConfirmButton";

test("renders the given label", () => {
  render(<ConfirmButton label="Save" onConfirm={jest.fn()} />);
  expect(screen.getByRole("button", { name: "Save" })).toBeInTheDocument();
});

test("calls onConfirm with the current value when clicked", async () => {
  const onConfirm = jest.fn();
  render(<ConfirmButton label="Save" onConfirm={onConfirm} />);
  await userEvent.click(screen.getByRole("button", { name: "Save" }));
  expect(onConfirm).toHaveBeenCalledTimes(1);
});
```

- Do not test implementation details (calling an internal function
  directly, asserting on internal `useState` values, counting renders) —
  that's shallow-rendering-style testing this platform does not use; it
  makes tests brittle to harmless refactors that don't change user-visible
  behavior.
- If the component depends on server state (TanStack Query/SWR), wrap the
  render in the app's query client provider and mock the network layer
  (e.g., MSW) instead of mocking the query hook, so the test exercises real
  loading/error/success transitions.

## Step 5 — Review

- Confirm every prop is typed and every genuinely-optional prop is marked
  `?` — no `any`.
- Confirm every subscription added in Step 3 has a matching cleanup.
- Confirm any list rendered by the component uses a stable, real `key`
  (never the array index for a list that can reorder or change length).
- Confirm no untrusted content reaches `dangerouslySetInnerHTML`
  unsanitized.
- Run the test for real before calling the component done.

## Do
- Decide the prop/owned-state/callback boundary before writing markup.
- Type every prop; avoid `any`.
- Write and run a React Testing Library test covering render, a prop
  change, and a user interaction.

## Don't
- Don't introduce a class component, or a different test runner/typing
  convention than the project already uses.
- Don't leave a subscription without a matching cleanup in `useEffect`.
- Don't use the array index as a list `key` for data that can change order
  or length.
- Don't claim the component works without running its test.
