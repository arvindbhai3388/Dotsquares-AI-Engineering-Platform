# Scaffold a Dataverse Plugin Skeleton

**Category:** Power Apps / Power Platform
**Use when:** Business logic needs to run server-side on a Dataverse table event.

## Prompt

Scaffold a Dataverse plugin class that implements `IPlugin` for the table, message, and stage I specify (e.g. Create/Update/Delete/Assign on a named entity). Before writing code, ask me for: the logical entity name, the message, the pipeline stage (pre-validation, pre-operation, post-operation), and whether it must run synchronously or asynchronously -- do not assume defaults.

The skeleton must include:
- A constructor accepting unsecure/secure configuration strings, stored as readonly fields, with null-safe handling.
- An `Execute(IServiceProvider serviceProvider)` method that resolves `IPluginExecutionContext` (or `IPluginExecutionContext2` if depth/shared-variable access is needed), `IOrganizationServiceFactory`, `IOrganizationService` (via `CreateOrganizationService(context.UserId)`), and `ITracingService`.
- Defensive checks: verify `context.Depth` to guard against infinite recursion, verify the target entity/message name matches expectations before casting `context.InputParameters["Target"]`, and early-return when preconditions aren't met.
- A try/catch around the business logic that wraps unexpected exceptions in `InvalidPluginExecutionException` with a user-meaningful message (never leak raw stack traces to the Dataverse UI), while writing diagnostic detail via `tracingService.Trace(...)`.
- A comment block documenting the intended plugin step registration: message, primary entity, stage, execution mode (sync/async), filtering attributes, and any secure/unsecure configuration expected -- this is what gets entered in the Plugin Registration Tool or a solution's `PluginStep` record, so be explicit and accurate.

Explicitly flag the sync-vs-async tradeoff for this specific scenario (does the caller need the result before the operation completes, or is eventual consistency acceptable) and recommend one, but let me confirm before finalizing. Also flag if this logic would be better suited to a Power Automate cloud flow or a classic workflow instead of a plugin, per the business-rule/workflow/plugin decision criteria. Follow the propose -> approve -> implement -> test workflow: show the skeleton and registration notes first, wait for approval, then write the file into the correct plugin project structure, and note that Test-First unit tests (mocking `IOrganizationService`/`ITracingService`) should be written before the real business logic is filled in.
