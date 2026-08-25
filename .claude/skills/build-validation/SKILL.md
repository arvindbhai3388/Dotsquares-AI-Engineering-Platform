---
name: build-validation
description: >
  Use as the final step before calling any change complete — builds and
  tests the actually-affected project(s) with the correct toolchain for
  that project's type. Trigger phrases: "build this", "run the tests",
  "validate this is working", "is this ready" — and runs automatically at
  the end of any non-trivial implementation task per the new-feature
  workflow's Test/Review steps. Covers both the dotnet CLI (SDK-style
  projects) and MSBuild (legacy non-SDK .NET Framework projects) paths that
  coexist across this platform's demos and, especially, client projects.
  Delegates the actual build/test execution to the `build-validator` agent.
---

# Build Validation Workflow

*Can be approved with a single Yes/No that carries this through to completion instead of a
per-step check-in — see `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" section.*

Never assert a build or test result without having actually run the
command. This skill exists specifically because mixed-toolchain .NET
solutions (a common reality in client codebases with a legacy web app
alongside modern services) produce misleading results if validated the
wrong way — the whole point is choosing the *correct* command per
project, not just running *a* command.

## Step 1 — Identify the affected project(s)

- From the change, determine exactly which `.csproj`(s) were touched.
- If the change altered a public contract (a shared library's public API,
  a database schema a shared `DbContext` maps, an interface consumed
  elsewhere), also identify direct dependents that need validating —
  don't stop at just the changed project if something else in the
  solution compiles against what changed.
- Don't expand to a whole-solution rebuild when a scoped build/test of
  the affected project(s) is sufficient — but don't under-scope either.

## Step 2 — Determine the toolchain per project

Check each affected project's `.csproj` — don't infer from the project
name or assume consistency across the solution:

- **SDK-style** (`<Project Sdk="Microsoft.NET.Sdk...">` at the top,
  modern `net6.0`+ target): use the `dotnet` CLI.
  ```bash
  dotnet build path/to/Project.csproj
  dotnet test path/to/Project.Tests.csproj
  ```
- **Legacy non-SDK** (classic `<Project ToolsVersion=...>` csproj,
  `packages.config`, `.NET Framework 4.x` target): `dotnet build`/`dotnet
  test` cannot correctly build these. Restore and build with MSBuild
  instead:
  ```bash
  nuget restore path/to/Solution.sln
  msbuild path/to/Project.csproj
  ```
  A modern SDK-style test project referencing a legacy project (a common
  pairing, e.g. MSTest testing a .NET Framework MVC app) still runs its
  own tests via `dotnet test` even though the project under test needed
  MSBuild to build.

## Step 3 — Never whole-solution-build a mixed-toolchain solution

If any project in the `.sln` is legacy non-SDK while others are
SDK-style, do **not** run `dotnet build`/`dotnet test` against the whole
`.sln` — the legacy project(s) will fail under the `dotnet` CLI
regardless of whether your actual change is correct, producing a false
negative that obscures the real answer. Build/test only the specific
affected project(s) individually, using each one's correct toolchain from
Step 2.

## Step 4 — Run and capture real output

- Execute the build command; if it fails, stop and report the actual
  compiler error(s) — don't proceed to "test" a project that didn't
  build.
- Execute the test command; capture pass/fail/skip counts and the actual
  failure message + stack trace (trimmed to the relevant part) for any
  failing test.
- If no test project exists for the affected code, or a required test
  needs an environment dependency unavailable here (a live database,
  external tenant), say so explicitly rather than silently skipping
  validation or claiming it passed.

## Step 5 — Report plainly

State exactly what was verified and what wasn't, e.g.:

> Build succeeded for `Foo.csproj` (dotnet build). 14/14 tests passed in
> `Foo.Tests.csproj` (dotnet test). Did not run the SharePoint
> integration test suite — it requires a live tenant not available here;
> the mock-backed unit tests covering the same logic did pass.

Never round this up to a blanket "all good" — the specific, honest
statement is the deliverable.

## Do
- Check each project's actual `.csproj` type before choosing a command.
- Scope to affected project(s) plus direct dependents of a changed
  contract.
- Report real output, including real failures, in full relevant detail.

## Don't
- Don't run `dotnet build`/`dotnet test` against a `.sln` containing any
  legacy non-SDK project.
- Don't claim a build/test passed without having run it.
- Don't fix a failure discovered here yourself unless asked — report it
  back to whoever's implementing.
