# Add a View Component for a Reusable Dynamic UI Fragment

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a partial view needs its own data-fetching logic, not just markup reuse.

## Prompt

I need a self-contained, reusable UI fragment (for example: a cart summary widget, a "recently viewed" list, a notifications badge) that needs its own data-fetching logic, not just shared markup -- meaning a plain partial view isn't enough because the caller shouldn't have to fetch and pass in the data itself. Implement this as a View Component rather than a partial view plus controller-side data-fetching duplicated at every call site.

Before implementing, confirm the component's dependencies (which service/repository it needs to fetch its own data) and how it will be invoked from views (`@await Component.InvokeAsync("Name", new { ... })` or the `<vc:name>` tag helper if this project's Razor views already use tag-helper-style invocation elsewhere -- match the existing convention rather than introducing a new invocation style). Get my confirmation on the component's public parameters before implementing, since that's the contract every call site will depend on.

Create the `ViewComponent` subclass in this project's existing view-components location (or establish one following its folder-per-concern convention), inject its dependencies via constructor (respecting existing DI lifetimes -- do not inject a scoped `DbContext` in a way that outlives the request), and implement `InvokeAsync` to fetch only what the fragment needs, returning `View(model)` with a dedicated small view model for the component -- never the raw entity. Create the companion Razor view under `Views/Shared/Components/<ComponentName>/Default.cshtml` (or the project's established path pattern).

Handle the "no data" case explicitly in the component (empty cart, no notifications) so it renders sensible empty-state markup rather than throwing or rendering broken HTML. Verify the component can be invoked from multiple different pages without each caller needing to know its internals. Write unit tests invoking `InvokeAsync` directly with a mocked dependency, covering the populated and empty-state cases.
