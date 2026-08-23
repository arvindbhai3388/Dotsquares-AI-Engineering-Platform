# Add Error Handling and Tracing to a Dataverse Plugin

**Category:** Power Apps / Power Platform
**Use when:** A plugin fails silently or with an unhelpful error message for end users.

## Prompt

Add proper exception handling and tracing to the Dataverse plugin I point you to, which currently either fails silently, throws an unhandled exception with a generic/unhelpful Dataverse error dialog, or gives no diagnostic trail when something goes wrong in Test/Prod. First read the plugin's current `Execute` method end to end and identify every place it can fail: null/missing attributes on the target entity, a Dataverse service call that can throw `FaultException<OrganizationServiceFault>`, an external call, or a business-rule violation that should stop the operation.

Implement:
- An `ITracingService` obtained from the service provider at the top of `Execute`, with `tracingService.Trace(...)` calls at each meaningful step (entry with key input values, before/after each external call, before returning) -- traces should include enough context to diagnose a Prod issue (entity id, message name) without logging full PII or secrets.
- A top-level try/catch around the plugin's business logic that catches specific exceptions first (e.g. `FaultException<OrganizationServiceFault>` to inspect `.Detail.ErrorCode`/`.Detail.Message`) before a general `catch (Exception ex)`, and in every catch path, trace the full exception (`ex.ToString()`) before deciding how to surface it.
- Re-throwing business-rule violations as `new InvalidPluginExecutionException("<clear, end-user-safe message>")` so the Dataverse UI shows something actionable instead of a generic "An error has occurred" -- never let a raw `NullReferenceException` or SQL-level error message reach the end user.
- Distinguishing between expected validation failures (should stop the operation cleanly with a clear message) and unexpected infrastructure failures (should still be traced in detail but can use a more generic user-facing message plus an internal correlation identifier for support to search logs by).
- If the plugin runs asynchronously, note that trace logs surface in the Async Operation ("System Job") record's message, and unhandled exceptions there won't block the user's UI -- so make sure any user-visible failure path is on a synchronous step instead, and flag it if it currently isn't.

Show me the diff before applying it, and add or update a unit test (mocking `IOrganizationService`/`ITracingService`/`IPluginExecutionContext`) that asserts the plugin throws `InvalidPluginExecutionException` with the expected message for at least one failure scenario, per the Test-First/Validate workflow.
