# Code Review Checklist

> **Template usage:** Run through this against a diff before calling implementation work
> done — as the "Review" step of the workflow, and as the basis for a human or
> `code-reviewer`-agent pass. Applies to any stack in this platform. Not every item applies
> to every change; mark N/A rather than skipping silently.

## 1. Correctness

- [ ] The change actually implements the requested behavior/acceptance criteria.
- [ ] Root cause was fixed (for bug fixes), not just the reported symptom.
- [ ] Edge cases are handled: empty/null input, boundary values, empty collections,
      duplicate calls, concurrent access where relevant.
- [ ] No off-by-one, sign, or unit-conversion errors in the touched logic.
- [ ] Tests written in Test-First actually exercise the new/changed behavior, and pass for
      the right reason.

## 2. Security

- [ ] No hardcoded credentials, secrets, tokens, connection strings, or API keys anywhere in
      the diff.
- [ ] No restricted/config file was read, modified, or had its contents echoed into output.
- [ ] All external/user input is validated before use.
- [ ] Authorization is checked server-side — no reliance on client-supplied IDs/roles/flags
      for access decisions.
- [ ] All SQL is parameterized (EF Core LINQ, `SqlParameter`, Dapper parameters) — no
      string-concatenated or interpolated SQL built from external input.
- [ ] No new attack surface introduced without corresponding validation/auth (new endpoint,
      new file upload, new deserialization path, new external HTTP call).

## 3. Nullability

- [ ] Nullable reference type annotations are accurate — no `!`-suppression used to silence
      a real possible-null path.
- [ ] Public method signatures reflect what can actually be null (parameters and return
      values).
- [ ] No new `NullReferenceException` risk on a path exercised by the tests or an obvious
      manual walkthrough.

## 4. Error Handling

- [ ] Exceptions are caught at an appropriate boundary, not swallowed silently.
- [ ] Errors are surfaced to the caller in the project's existing shape (e.g.
      `ProblemDetails`, existing exception-to-status mapping) rather than a new ad hoc shape.
- [ ] Logging captures enough to diagnose the failure, without logging secrets, tokens, or
      unnecessary personal data.
- [ ] `CancellationToken`s are accepted and propagated on async I/O paths that support them.

## 5. Performance

- [ ] No new N+1 query pattern introduced (check `Include`/projection usage on any new EF
      Core query, or equivalent batching for other data access).
- [ ] No blocking calls (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`) on async code in
      request-handling or hot paths.
- [ ] No unnecessary large in-memory materialization (e.g. loading a full table to filter in
      memory) where the change touches a potentially large dataset.
- [ ] `HttpClient`/DB connections are obtained via the project's existing factory/DI pattern,
      not created ad hoc per call.

## 6. Maintainability

- [ ] Names are clear and consistent with the surrounding code's conventions.
- [ ] Logic is not duplicated where an existing helper/service already does the same thing.
- [ ] No premature abstraction added for a single call site.
- [ ] Comments explain *why*, not *what*, and only where the code isn't self-explanatory.
- [ ] File/class size and structure remain consistent with the project's existing patterns
      (no giant inline handler blocks where the project otherwise extracts methods/classes).

## 7. Backward Compatibility

- [ ] Existing public API fields, response formats, and status codes are unchanged, or the
      break is explicitly documented and approved.
- [ ] Database changes follow an expand/contract pattern if versions may overlap in
      production during deploy.
- [ ] No breaking change to a shared library/contract consumed by other projects without
      that being called out.

## 8. Unintended Changes

- [ ] The diff contains only the files necessary for this change — no incidental
      formatting-only changes, reordering, or whitespace churn in untouched code.
- [ ] No unrelated dependency version bumps.
- [ ] No debug code, commented-out blocks, `Console.WriteLine`/`TODO`s left behind
      unintentionally.
- [ ] `git diff`/`git status` reviewed in full before considering the change complete.
