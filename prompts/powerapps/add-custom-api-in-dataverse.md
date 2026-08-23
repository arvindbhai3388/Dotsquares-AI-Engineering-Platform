# Define a Custom API in Dataverse Backed by a Plugin

**Category:** Power Apps / Power Platform
**Use when:** Exposing a specific server-side operation that doesn't fit standard CRUD messages.

## Prompt

Define a Dataverse Custom API for the operation I describe (state whether it's bound to a table/table-set or unbound, its request parameters, and its response) and implement the backing plugin, as a strongly-typed alternative to a generic custom action or an ad hoc CRUD workaround. Before implementing, confirm with me: the exact operation name (following the publisher prefix convention already used in this solution), whether it should be bound (operates on/relative to a specific record or table) or unbound (a free-standing operation, e.g. a calculation or batch trigger), and whether it needs to participate in the caller's transaction (most Custom APIs do, unlike some webhook patterns).

Deliverables:
- The Custom API definition itself: `uniquename`, `bindingtype` (Entity/EntityCollection/Global), `boundentitylogicalname` if bound, `isfunction` (true only for pure, no-side-effect read operations that should be callable via GET), and `isprivate`/`allowedcustomprocessingsteptype` settings matching whether other plugins should be able to pre/post-process it.
- One `CustomAPIRequestParameter` entry per input (name, type, required, matching the plugin's expected `context.InputParameters` keys) and one `CustomAPIResponseProperty` entry per output (matching `context.OutputParameters`), with types chosen from Dataverse's supported set (String, Integer, Boolean, EntityReference, Entity, StringArray, etc.) -- do not invent a type Dataverse doesn't support.
- The backing plugin class, following this repo's plugin skeleton conventions (constructor, `ITracingService`, defensive input validation, `InvalidPluginExecutionException` on business-rule failures) registered against the Custom API's message name rather than a standard CRUD message.
- The calling convention from both Dataverse SDK (`OrganizationRequest` with the Custom API's unique name) and Web API (`POST /api/data/v9.2/<uniquename>` or the bound equivalent) so consumers on either side of this codebase can invoke it correctly.

Explain why a Custom API is the right fit here versus a classic custom action or a plain plugin on an existing message (typically: need for a strongly-typed, discoverable, independently-versioned API surface). Propose the full definition and plugin skeleton, wait for approval, implement, and write Test-First unit tests against the plugin logic before filling in the real business logic.
