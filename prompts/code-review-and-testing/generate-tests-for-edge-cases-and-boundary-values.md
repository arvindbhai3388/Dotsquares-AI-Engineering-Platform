# Generate Tests for Edge Cases and Boundary Values

**Category:** Code Review & Testing
**Use when:** Existing tests only cover the happy path.

## Prompt

Review the existing tests for the method/class I specify, identify that they only exercise the happy path, and add tests that specifically target edge cases and boundary values the current suite misses.

Read the method's actual logic first -- conditionals, loops, range checks, comparisons -- and derive boundaries from the real code rather than a generic checklist, then also apply this checklist to make sure nothing obvious is missed:

- **Null and missing values** -- null arguments, null properties on an input object, a null entry inside an otherwise valid collection.
- **Empty collections/strings** -- empty list/array passed where the method expects at least one item; empty string vs. whitespace-only string, if the method treats them differently (or should).
- **Numeric boundaries** -- zero, negative numbers where only positive is expected (and vice versa), the exact value at a comparison boundary (`<` vs `<=` -- test the value equal to the boundary, one below, and one above), `int.MinValue`/`int.MaxValue` if overflow is plausible, and division-by-zero risk if the method divides by a value that could be zero.
- **Off-by-one conditions** -- first and last element of a collection, a loop bound that iterates `Count` vs `Count - 1` times, pagination logic at the first page, last page, and a page beyond the last.
- **Date/time edges** -- month/year boundaries, leap years if date arithmetic is involved, and time zone handling if the method mixes UTC and local time.
- **Duplicate/conflicting input** -- duplicate keys in a dictionary-building operation, conflicting flags passed together that the method may not explicitly guard against.
- **Size/length limits** -- input exceeding a documented or implicit maximum length, and the exact value at that maximum.

For each new test, name it clearly with the specific boundary it targets (e.g., `Calculate_QuantityAtExactDiscountThreshold_AppliesDiscount`), and use Arrange-Act-Assert structure consistent with the existing test file's framework (xUnit, MSTest, or NUnit -- match what's already there). Prefer parameterized tests (`[Theory]`/`[InlineData]`, `[DataTestMethod]`/`[DataRow]`, or NUnit `[TestCase]`) for boundary sweeps that share the same assertion shape across several input values, instead of near-duplicate test methods.

Run the new tests and report actual results. If a boundary test reveals the method doesn't actually handle that case correctly, stop and report it as a likely bug rather than adjusting the test's expected value to match the current (possibly wrong) behavior -- flag it for a decision before treating it as passing.
