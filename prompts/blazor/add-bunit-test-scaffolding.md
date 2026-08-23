# Add bUnit Test Scaffolding for an Existing Component

**Category:** Blazor
**Use when:** A component currently has no automated test coverage and needs a baseline test suite before further changes.

## Prompt

First, read the target component fully — its parameters, injected services, lifecycle methods (`OnInitialized[Async]`, `OnParametersSet[Async]`, `OnAfterRender[Async]`), and any `IJSRuntime`/`HttpClient`/cascading dependencies — and propose a test plan listing the scenarios you intend to cover (render with defaults, render with each meaningful parameter combination, event callback firing, conditional markup branches, disposal) before writing test code. Wait for my approval of the scenario list.

Set up (or extend) the bUnit `TestContext` for this component: register any required services the component injects via `Services.AddSingleton<T>()`/mocked interfaces with Moq, configure `JSInterop.Mode` (`Loose` for quick scaffolding, `Strict` if precise JS call verification matters) if the component does interop, and supply cascading parameter values via `RenderComponent<T>(parameters => parameters.AddCascadingValue(...))` if the component consumes any.

Write tests using the Arrange/Act/Assert structure already used in this codebase's other bUnit suites: arrange mocks and parameters, act by calling `RenderComponent<T>(...)` (or `cut.SetParametersAndRender(...)` for updates), and assert using bUnit's semantic HTML comparison (`cut.MarkupMatches(...)`) for structural checks or `cut.Find(...)`/`cut.FindAll(...)` plus assertions on text/attributes for targeted checks. For event callbacks, trigger the relevant DOM event via `cut.Find("button").Click()` (or the appropriate `TriggerEvent` overload) and assert the callback fired with the expected argument using a captured variable or Moq verification.

Cover the disposal path if the component implements `IDisposable`/`IAsyncDisposable` — dispose the render context and assert cleanup happened (event unsubscription, JS module disposal called). Do not test Blazor framework behavior itself (e.g. that `[Parameter]` binding works) — focus tests on this component's own logic and branching. Run the test project (`dotnet test` against the correct test project for this codebase) and confirm all new tests pass before reporting completion.
