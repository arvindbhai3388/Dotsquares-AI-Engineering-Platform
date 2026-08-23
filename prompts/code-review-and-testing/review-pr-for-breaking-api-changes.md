# Review a PR for Breaking API Changes

**Category:** Code Review & Testing
**Use when:** A change touches a controller/DTO that external or other-team consumers depend on.

## Prompt

Review the PR/diff I specify for backward-incompatible changes to a public API, given that the affected controller/DTO has consumers outside this immediate change (another team's service, an external client, or a mobile/frontend app that may not deploy in lockstep with this change).

Check specifically for:

1. **Removed or renamed fields** in request/response DTOs -- a removed field breaks any consumer still reading it; a renamed field is effectively a removal-plus-addition and breaks deserialization on the consumer side even if the new name seems obviously equivalent.
2. **Changed field types** -- a field changed from string to a structured object, from nullable to non-nullable (or vice versa, if the consumer's client-side model doesn't tolerate null), from a number to a string representation, or an enum with renumbered/removed values that changes the wire value for existing options.
3. **Changed status codes or error contract** -- an endpoint that previously returned 200 with an empty body now returning 204, a validation failure moved from 400 to 422 or vice versa, or an error response body's shape changing (field renamed/removed in a standard error envelope).
4. **Changed route or verb** -- a route parameter renamed, a route moved, or the HTTP verb changed for an existing operation.
5. **Changed semantics without a shape change** -- a field that keeps the same name and type but now means something different (e.g., a `total` field that used to exclude tax now includes it) -- this is the hardest to catch mechanically and needs a manual read of the diff's intent, not just a schema diff.
6. **Tightened validation** -- new required-field or format validation on an existing request field that previously accepted a wider range of input, which would reject requests from consumers that were previously valid.

For each finding, state clearly whether it is breaking, and if so, propose the additive alternative: add a new field/endpoint/version instead of changing the existing one, deprecate the old field with a clear marker before removal, or gate the new behavior behind API versioning if the project has a versioning scheme. If a breaking change is genuinely unavoidable for this ticket, say so explicitly, identify who needs to be notified (or which teams' contracts need updating), and confirm with me before treating it as acceptable rather than assuming it's fine because "it's already in the PR."
