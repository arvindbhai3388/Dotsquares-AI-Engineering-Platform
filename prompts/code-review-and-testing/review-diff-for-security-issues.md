# Review a Diff for Security Issues

**Category:** Code Review & Testing
**Use when:** A PR touches authentication, data access, or user input handling.

## Prompt

Review the attached diff (or the currently staged/uncommitted changes) exclusively through a security lens. Do not comment on style, naming, or unrelated maintainability issues in this pass -- a separate general review can cover those.

Walk the diff hunk by hunk and check for:

1. **Injection** -- SQL built by string concatenation or interpolation instead of parameterized queries/stored procedure parameters; command injection via `Process.Start` with unsanitized input; LDAP/XPath/NoSQL injection; log injection from unescaped user input written to logs.
2. **Authentication and authorization** -- new or modified endpoints/actions missing `[Authorize]` (or the project's equivalent) or scoped to the wrong role/claim; authorization checks performed only client-side; object-level authorization gaps (a user can supply an ID and access another user's/tenant's record because ownership is never checked); missing anti-forgery tokens on state-changing actions.
3. **Secrets and sensitive data** -- hardcoded connection strings, API keys, tokens, or passwords; secrets or PII written to logs, exceptions, or error responses returned to the client; secrets committed to configuration files that should instead flow through the existing options/DI pattern.
4. **Insecure deserialization** -- untrusted input deserialized with a binder that allows arbitrary type resolution (e.g., `TypeNameHandling.All` in Newtonsoft.Json), or deserialization of untrusted XML without disabling external entities (XXE).
5. **Input validation** -- missing server-side validation on inputs that are validated only in the client/UI; unbounded input sizes; path traversal from user-controlled file paths; open redirects from user-controlled URLs.
6. **Cryptography** -- weak hashing (MD5/SHA1 for passwords instead of a salted adaptive hash), predictable random values used for tokens (`Random` instead of a cryptographic RNG), disabled TLS certificate validation.

For each finding, cite the exact file and line, explain the concrete attack scenario (not just "this is bad practice"), rate severity (Critical/High/Medium/Low), and propose the smallest fix that closes the gap without changing unrelated behavior. If nothing in the diff introduces a security issue, say so explicitly rather than inventing marginal nitpicks. Do not weaken existing security controls to make the diff pass. Follow the analyze -> propose -> approve workflow: present findings and proposed fixes, then wait for approval before editing any code.
