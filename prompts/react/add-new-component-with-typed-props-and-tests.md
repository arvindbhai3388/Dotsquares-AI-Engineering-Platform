# Create a New Component with Typed Props and Tests

**Category:** React
**Use when:** Building a new reusable component from a spec, or extracting duplicated JSX/logic into one.

## Prompt

Analyze the surrounding feature area and any similar existing components first, then propose a plan for the new component before writing code — follow the analyze -> propose -> approve -> implement -> test -> review workflow and wait for my go-ahead on the prop shape and file layout before implementing.

Define the component's props as an explicit `interface` (or `type`) — never `any`, and avoid `React.FC` unless that's the existing project convention, since it implicitly adds `children` and complicates generic components. Distinguish clearly between required and optional props (`?`), give optional props sensible defaults via destructuring (`{ variant = "primary" }`) rather than `defaultProps` (deprecated for function components), and type callback props as concrete function signatures (`onSelect: (id: string) => void`) rather than bare `Function`. If the component accepts arbitrary children, type it as `React.ReactNode`, and if it needs to forward a ref to the underlying DOM node, use `forwardRef<HTMLElement, Props>` with the ref properly typed rather than accepting a loosely-typed ref prop.

Keep the component pure and presentational where possible — push data fetching and business logic into a parent, container, or custom hook rather than embedding it directly, matching how this codebase already separates concerns. If the component wraps a native element, spread any unrecognized/pass-through props (`...rest`) onto that element so callers can still pass `className`, `id`, `data-*`, and `aria-*` attributes. Co-locate the component's styles using this project's existing styling approach (CSS Modules, Tailwind, styled-components, etc.) — do not introduce a new styling method without checking what's already in use.

Write tests with React Testing Library (and Vitest or Jest, matching this project's existing test runner) covering: default render, each meaningful prop variation, conditional rendering branches, and any callback prop firing correctly on user interaction via `@testing-library/user-event` (not `fireEvent` directly, unless the codebase already standardizes on `fireEvent`). Query elements the way a user would — `getByRole`, `getByLabelText`, `getByText` — before falling back to `data-testid`. Run the test suite and confirm the new tests pass before reporting completion, and flag if the component needs a corresponding Storybook story per this project's existing conventions.
