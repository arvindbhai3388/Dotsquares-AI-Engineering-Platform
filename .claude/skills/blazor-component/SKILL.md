---
name: blazor-component
description: >
  Use to scaffold a new Blazor component (Server or WebAssembly) correctly
  end to end — parameters, events, lifecycle, and a bUnit test. Trigger
  phrases: "create a new Blazor component", "scaffold a component for X",
  "add a reusable component". For general fixes/changes to existing
  components, prefer the blazor-developer agent; use this skill
  specifically when standing up a brand-new component from nothing.
---

# Blazor Component Scaffolding Workflow

A new component should be usable, testable, and correctly disposed from
the moment it's created — this skill walks through each of those
concerns explicitly rather than leaving them to be discovered later.

## Step 1 — Confirm the hosting model and component boundary

- Confirm whether the target project is Blazor Server or WebAssembly
  (affects JS interop sync/async availability and state-sharing
  assumptions — see blazor-developer for the Server-vs-WASM tradeoffs if
  that choice itself is in question).
- Define the component's single responsibility before writing markup —
  if it's doing two unrelated things, split it into two components now
  rather than after it's grown further.
- Decide what's a **parameter** (data/config passed in by the parent),
  what's **owned state** (internal to the component), and what needs to
  **bubble up** via an event callback — get this boundary right before
  writing code; retrofitting it later touches every call site.

## Step 2 — Define the parameter/event contract

```csharp
[Parameter, EditorRequired]
public T Value { get; set; } = default!;

[Parameter]
public EventCallback<T> ValueChanged { get; set; }

[Parameter]
public RenderFragment? ChildContent { get; set; }
```

- Mark parameters `[EditorRequired]` when they're not meaningfully
  optional — this gets caught by the analyzer at compile/build time
  instead of failing at runtime.
- For two-way binding support (`@bind-Value="..."` at the call site), the
  parameter must be named `Value` (or `X`) paired with an
  `EventCallback<T>` named exactly `ValueChanged` (or `XChanged`) — the
  naming convention is load-bearing, not stylistic.
- Use `RenderFragment`/`RenderFragment<T>` for child-content injection;
  be careful about closures over loop variables if the fragment is built
  inside a loop.
- Use `[CascadingParameter]` only for values genuinely shared down a
  whole subtree (theme, current user) — not as a shortcut past two or
  three levels of explicit parameters.

## Step 3 — Implement lifecycle correctly

- One-time setup → `OnInitializedAsync` (not the constructor; components
  can be reused).
- Logic that must react to parameter changes → `OnParametersSetAsync`
  (remember it also runs on the very first set, alongside
  `OnInitializedAsync`).
- Anything needing the rendered DOM (JS interop initializing a widget) →
  `OnAfterRenderAsync`, gated on `firstRender` if it should run exactly
  once.
- If the component subscribes to any event, timer, or JS object
  reference, implement `IDisposable`/`IAsyncDisposable` and unsubscribe/
  dispose there — do this in the same step as adding the subscription,
  not as an afterthought; a component without disposal logic for
  something it subscribed to is an immediate defect.

## Step 4 — Write the markup

- Encode all user-supplied content by default (`@expression` auto-
  encodes); never route untrusted content through `@Html.Raw`-equivalent
  unencoded rendering.
- Keep conditional/display logic in the markup simple; push non-trivial
  logic into the code-behind (`@code` block or partial class) as a
  private method the markup calls.
- Match the project's existing component file convention (single-file
  `.razor` with `@code`, or `.razor` + `.razor.cs` partial class) —
  don't introduce the other style into a project that's standardized on
  one.

## Step 5 — Write a bUnit test

For the general bUnit idioms (confirming the dependency, `TestContext.Services`,
`RenderComponent`, asserting on markup not implementation, triggering via rendered
elements rather than direct method calls), see the canonical guidance in the
`unit-test-writer` agent — do not restate it here. For this new component specifically,
cover:

- Initial render with typical parameters.
- A parameter change (`cut.SetParametersAndRender(...)`) producing the expected
  re-render.
- The event-callback firing with the expected argument on user interaction.

## Step 6 — Review

- Confirm no leaked subscriptions (every subscribe has a matching
  dispose).
- Confirm two-way binding naming is exact if `@bind-` support is
  intended.
- Confirm JS interop calls (if any) are gated to `OnAfterRender`/
  `firstRender` correctly and that `IJSObjectReference`s are disposed.
- Run the bUnit test for real before calling the component done.

## Do
- Decide the parameter/owned-state/event boundary before writing markup.
- Implement disposal alongside any subscription, not after.
- Write and run a bUnit test covering render, parameter change, and
  event callback.

## Don't
- Don't call JS interop before `OnAfterRender`.
- Don't leave a subscription without a matching dispose.
- Don't bind two-way support with mismatched `Value`/`ValueChanged`
  naming.
- Don't claim the component works without running its test.
