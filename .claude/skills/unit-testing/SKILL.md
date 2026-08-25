---
name: unit-testing
description: >
  Use to write failing tests before implementing (Test-First) or to
  add/update tests after implementing (Validate) for any project in this
  platform. Trigger phrases: "write tests for X", "add unit tests", "test
  this method/component/hub", "write a failing test first". Enforces
  framework auto-detection (xUnit/MSTest/NUnit/bUnit) instead of assuming
  one, and runs by default before any non-trivial implementation per the
  new-feature workflow. Delegates the actual test authoring/execution to
  the `unit-test-writer` agent.
---

# Unit Testing Workflow

*Can be approved with a single Yes/No that carries this through to completion instead of a
per-step check-in — see `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" section.*

This skill enforces test-first discipline while never assuming a test
framework — a platform used by 50+ developers across many client projects
will encounter all of xUnit, MSTest, and NUnit, sometimes more than one
within the same solution for different generations of code.

## Step 1 — Detect the framework (always, first)

- Locate the target code's paired test project: conventionally
  `<Project>.Tests` or `Tests.<Project>` alongside/near it — search
  narrowly (the same solution/directory tree) before assuming none
  exists.
- Open that test project's file and check package references:
  - `xunit` + `xunit.runner.visualstudio` → **xUnit**
  - `MSTest.TestFramework` + `MSTest.TestAdapter` → **MSTest**
  - `NUnit` + `NUnit3TestAdapter` → **NUnit**
  - `bunit` (on top of xUnit or NUnit — check which) → **bUnit** for
    Blazor components
- If more than one test project exists for the same target project
  (rare but possible during a migration), match whichever one actually
  covers the code being changed; don't introduce a second test file in
  the "other" framework's project for the same class.
- If **no** test project exists yet for the target code: stop and ask
  the user which framework to establish, rather than silently picking
  one. Default suggestion if asked to choose: xUnit + Moq (this
  platform's own convention for new work), but confirm rather than
  assume, since the surrounding solution's other test projects may
  already lean a different way.

## Step 2 — Match existing conventions

Before writing a single test, look at 1-2 existing tests in the target
test project and match:

- Naming pattern (`MethodName_Scenario_ExpectedResult`, `Should_...`,
  `Given_When_Then` — whichever is already consistent there).
- Mocking library (Moq vs NSubstitute) — never introduce a second one
  into a project that already has one.
- Setup/teardown style (constructor + `IDisposable` for xUnit;
  `[TestInitialize]`/`[TestCleanup]` for MSTest; `[SetUp]`/`[TearDown]`
  for NUnit).
- Assertion style (fluent `Assert.That(...)`/FluentAssertions vs classic
  `Assert.AreEqual`/`Assert.Equal` — match what's there).

## Step 3 — Test-First (before implementation exists)

- Write the test to describe the **intended** behavior precisely —
  specific inputs, specific expected outputs/side effects/exceptions.
- Run it. Confirm it fails for the right reason: an assertion failure
  because the behavior doesn't exist yet, not a compile error, a missing
  type, or a broken test fixture. A test that fails for the wrong reason
  tells you nothing about the real behavior — fix the scaffolding before
  treating this as done.
- Do not write the implementation yet — Test-First means the test exists
  and fails first, by design.

## Step 4 — Validate (after implementation)

- Run the full affected test project for real.
- Confirm the Test-First test(s) now pass.
- Add or update any further tests a meaningful business-logic change
  still needs beyond what was written first — success, validation/bad-
  input, failure, authorization, and cancellation paths, as applicable to
  the code under test.
- Never weaken, delete, or skip (`[Ignore]`/`Skip = "..."`) a test —
  existing or new — merely to make an implementation pass. If a test's
  expectation is genuinely wrong given an intentional behavior change,
  update it explicitly and say so; don't silently suppress it.

## Framework cheat sheet

| Concept | xUnit | MSTest | NUnit |
|---|---|---|---|
| Class marker | (none) | `[TestClass]` | `[TestFixture]` |
| Test method | `[Fact]` | `[TestMethod]` | `[Test]` |
| Parameterized | `[Theory]`+`[InlineData]`/`[MemberData]` | `[DataTestMethod]`+`[DataRow]` | `[TestCase]` |
| Per-test setup | ctor / `IDisposable` | `[TestInitialize]`/`[TestCleanup]` | `[SetUp]`/`[TearDown]` |
| Per-class setup | `IClassFixture<T>` | `[ClassInitialize]`/`[ClassCleanup]` | `[OneTimeSetUp]`/`[OneTimeTearDown]` |
| Exception assert | `Assert.Throws<T>` | `Assert.ThrowsException<T>` | `Assert.Throws<T>` |

**bUnit** (Blazor components, built on xUnit or NUnit — check which):
render via `TestContext.RenderComponent<T>(...)`, assert on rendered
markup/component state via `cut.Find(...)`/`cut.Markup`, register
dependencies through `TestContext.Services`, trigger interactions through
element handles (`cut.Find("button").Click()`) rather than calling
component methods directly.

## Do
- Detect the framework before writing anything.
- Confirm a Test-First test fails for the right reason before moving on.
- Cover success, validation, failure, authorization, and cancellation
  paths for meaningful business logic.

## Don't
- Don't assume a framework — check package references every time.
- Don't introduce a second test framework/mocking library into a project
  that already has one established.
- Don't weaken or delete a test to make an implementation pass.
- Don't claim tests pass without running them.
