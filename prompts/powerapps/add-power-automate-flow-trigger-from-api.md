# Trigger a Power Automate Flow from a .NET Web API

**Category:** Power Apps / Power Platform
**Use when:** A .NET-side event needs to kick off a no-code/low-code automation.

## Prompt

Add the ability for the .NET Web API/service I point you to (identify the specific controller action or background-job step) to trigger a Power Automate flow via an HTTP request, as part of an existing business process. Before writing code, locate the exact point in the .NET codebase where the triggering event happens, and confirm with me what the flow is supposed to do downstream (e.g. send an approval email, update a SharePoint list, post a Teams message) so the payload contract makes sense.

Implementation should:
- Use the flow's "When an HTTP request is received" trigger contract: define the exact JSON schema the flow expects, and generate a matching C# request DTO plus a typed `HttpClient` call (reuse the existing `HttpClient`/`IHttpClientFactory` registration pattern already used elsewhere in this project rather than `new HttpClient()` inline).
- Store the flow's trigger URL (which contains a SAS-style signature) as configuration, not as a hardcoded string -- point me to where it should live in the strongly-typed options/config pattern this project already uses, and remind me that the actual URL value goes in a config file that is off-limits for you to read or write directly; ask me for a placeholder value instead.
- Handle the call as fire-and-forget vs. await-the-response deliberately: state which one applies here based on whether the calling code path needs the flow's result before continuing, and implement retry/timeout handling (e.g. a short timeout with one retry via Polly if already a dependency, otherwise a simple try/catch with logging) so a slow or failing flow doesn't block or crash the caller.
- Never log the full trigger URL (it's effectively a bearer credential) -- log only the flow name/purpose and the HTTP status code returned.
- Include the negative path: what should happen in the .NET code if the flow call fails (should the business operation still succeed, should it be queued for retry, should it surface a warning to the user).

Follow the standard workflow: propose the DTO, the call site, and the failure-handling behavior; wait for my approval; then implement, and write/update a unit test (mocking the `HttpClient`/message handler) that asserts the request is shaped correctly and that a failure doesn't throw an unhandled exception up to the caller.
