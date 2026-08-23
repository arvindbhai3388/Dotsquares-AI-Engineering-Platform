# Add an Action Filter for a Cross-Cutting Concern

**Category:** ASP.NET MVC / Razor Pages
**Use when:** the same boilerplate logic is duplicated at the top of multiple actions.

## Prompt

I've noticed (or want you to find) the same boilerplate logic -- timing/logging, an authorization/permission check, request stamping, or similar -- repeated at the top of several controller actions. Locate every action with this duplicated block and confirm it's truly identical logic, not several similar-looking checks with subtly different rules that shouldn't be merged into one filter.

Propose an action filter as the fix, and tell me which filter type fits: `IActionFilter`/`ActionFilterAttribute` for pre/post logic around the action (timing, logging, response shaping), `IAuthorizationFilter` if it's a permission/access check that should run before model binding and other filters, or `IExceptionFilter` if it's centralizing error handling. Get my confirmation on the filter type and scope (applied per-action via attribute, per-controller, or globally via `FilterConfig`/`MvcOptions.Filters`) before implementing, since a global filter changes behavior for every action in the app and needs to be justified.

Implement the filter following this project's existing filter conventions and location if any filters already exist. Make sure it fails safely -- an exception thrown inside a filter can produce a confusing error for every action it's applied to, so wrap risky logic (e.g., a database check for authorization) and handle failure explicitly by short-circuiting the pipeline (`context.Result = ...`) rather than letting an exception propagate unexpectedly. If it depends on services (a logger, a permission service), get them via constructor injection if this project's DI supports filter activation that way, rather than `new`-ing up dependencies inside the filter.

Remove the duplicated inline logic from each action once the filter is applied and verified to produce identical behavior. Write unit tests against the filter directly (constructing the appropriate `FilterContext` and asserting on `context.Result`/`ModelState`) for the pass and fail cases, plus a check that at least one previously-duplicated action still behaves correctly with the filter applied instead.
