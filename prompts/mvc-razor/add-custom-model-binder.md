# Add a Custom Model Binder for a Non-Standard Request Shape

**Category:** ASP.NET MVC / Razor Pages
**Use when:** default model binding can't handle the incoming request format (e.g., comma-separated query values, custom headers).

## Prompt

An incoming request shape (for example: a comma-separated list in a single query parameter, a value that needs to come from a custom header instead of the body/query/route, or a legacy client sending a format the default binder can't parse) doesn't bind cleanly with the framework's default model binding. Before writing a custom binder, confirm this genuinely can't be solved with a simpler option -- a `[FromQuery]`/`[Bind]` attribute tweak, a `TypeConverter` on the target type, or parsing manually inside the action -- since a custom `IModelBinder` is more machinery than most cases need. Show me why the simpler options don't fit before proceeding.

If a custom binder is warranted, implement `IModelBinder` (or `IModelBinderProvider` if it needs to apply conditionally based on the target type) matching this project's existing binder(s) if any already exist, so registration and structure are consistent. Read the raw value from the correct source (`bindingContext.ValueProvider` for classic MVC, `bindingContext.HttpContext.Request` for direct header/body access in ASP.NET Core) rather than assuming query string only. Handle the missing-value and malformed-value cases explicitly -- set `bindingContext.Result = ModelBindingResult.Failed()` and add a `ModelState` error with a clear message rather than throwing an unhandled exception, since a binder exception typically surfaces as an opaque 500 to the client.

Register the binder narrowly (via a `[ModelBinder(BinderType = typeof(...))]` attribute on the specific parameter, preferred over a global provider unless multiple actions need it) so it doesn't unexpectedly change binding behavior elsewhere in the app.

Write unit tests instantiating the binder directly against a constructed `ModelBindingContext` (or the appropriate test double for this project's MVC version), covering: well-formed input, empty/missing input, and malformed input, confirming `ModelState` errors are set correctly on failure before considering this done.
