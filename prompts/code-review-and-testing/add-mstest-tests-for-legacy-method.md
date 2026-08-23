# Add MSTest Tests for a Legacy Method

**Category:** Code Review & Testing
**Use when:** Adding first-time test coverage to an older codebase using MSTest.

## Prompt

Write MSTest tests for the legacy method I specify, in a .NET Framework codebase that has little or no existing test convention to copy. Since there's no established local pattern to mirror, apply solid MSTest defaults explicitly rather than guessing at house style:

- Use `[TestClass]` on the test class and `[TestMethod]` on each test, named `MethodName_Scenario_ExpectedResult`.
- Use `[TestInitialize]`/`[TestCleanup]` for setup/teardown shared across tests in the class -- but keep each test independent; nothing set up in one test method should leak into another, and tests must pass regardless of execution order (MSTest does not guarantee order within a class).
- Structure each test body as Arrange-Act-Assert, and use `Assert.AreEqual`, `Assert.ThrowsException<T>` (or `Assert.ThrowsExceptionAsync<T>` for async), and `CollectionAssert` where appropriate rather than manual boolean checks that produce unhelpful failure messages.
- Use `[DataTestMethod]` with `[DataRow(...)]` for boundary-value and equivalence-class variations of the same logical test instead of copy-pasting near-identical test methods.

Before writing tests, read the method plus its immediate dependencies to understand real behavior -- legacy code in this class of codebase often has surprising side effects (static state, hidden I/O, database calls made inline) that aren't obvious from the signature. If the method is tightly coupled to something impractical to run in a test (a live database connection, `HttpContext`, file system access), flag that explicitly and propose either the smallest viable seam to make it testable, or a characterization/approval-style test that pins current behavior without asserting on internals -- do not silently skip untestable logic.

Cover happy path, boundary values, null/invalid input, and exception paths. If the method interacts with EF6 (`DbContext`)-based data access or raw ADO.NET (this project's existing ADO.NET helper (if one exists)/`SqlCommand`), do not hit a real database from the unit test -- isolate via the existing repository/interface abstraction if one exists, or flag that an integration-test-level approach is more appropriate and ask before proceeding. Run the tests via `dotnet test` (or the project's MSTest runner) afterward and report actual results, not assumed results.
