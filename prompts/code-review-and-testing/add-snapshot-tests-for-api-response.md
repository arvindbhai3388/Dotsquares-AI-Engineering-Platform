# Add Snapshot/Approval Tests for an API Response

**Category:** Code Review & Testing
**Use when:** Verifying an entire response payload manually in assertions is unwieldy.

## Prompt

Add snapshot (approval) tests for the API response I specify, whose full payload is large/complex enough that hand-writing field-by-field assertions would be unwieldy and would obscure what actually matters about the shape.

Use the project's existing snapshot testing library if one is already a dependency (e.g., Verify, or an approval-testing library already referenced in a test project); if none exists, propose adding one and confirm before introducing the new dependency, per this repo's rule against adding dependencies for problems an existing tool could solve, and against adding one without a clear need.

Set the test up so it:

1. Calls the real code path that produces the response (via the actual service/controller method, or an integration-style call through `WebApplicationFactory` if that's more representative of what ships).
2. Serializes the response to a normalized, human-readable format (formatted JSON, not a raw object dump) so the diff on a snapshot mismatch is actually readable in a PR.
3. **Scrubs non-deterministic fields before comparing** -- timestamps, generated GUIDs, request-correlation IDs, or anything else that legitimately changes on every run. Replace them with fixed placeholder values (e.g., `"generatedAt": "<scrubbed>"`) rather than excluding those fields from the snapshot entirely, so a structural change to those fields (wrong type, field renamed) still gets caught.
4. Stores the approved/accepted snapshot file alongside the test, committed to source control, so a snapshot diff shows up clearly in code review.

Explain the tradeoff to whoever reviews this: snapshot tests are excellent at catching accidental/unintended shape changes but weak at expressing *intent* -- a reviewer approving a snapshot diff must actually read it and confirm the new shape is correct, not just click "accept" to make the test pass. Note this risk directly in a comment near the test or in the PR description.

After creating the test, run it once to generate/verify the initial snapshot, then deliberately introduce a small unintended change to the response and confirm the test fails with a readable diff -- proving the test would catch a real regression before you finalize it.
