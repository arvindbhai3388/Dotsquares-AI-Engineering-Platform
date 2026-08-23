# Audit and Clean Up Fragile TempData Usage

**Category:** ASP.NET MVC / Razor Pages
**Use when:** TempData-based messaging (e.g., success banners) is unreliable.

## Prompt

There's a controller flow where TempData-based messaging (success/error banners, cross-redirect state) is unreliable -- messages sometimes don't show up, or show up on the wrong page. Locate every read and write of TempData across the actions involved in this flow (including any base controller or action filter that might also touch TempData) and inspect the full request lifecycle: which action sets the key, which action(s) read it, and whether any of them run more than one redirect apart (TempData only survives one redirect by default unless `Keep()`/`Peek()` is used deliberately).

Diagnose the root cause before changing anything -- common causes are: mismatched key names (string literals typed differently in two places), reading TempData with `TempData["Key"]` in a GET action that runs after a second redirect has already consumed it, multiple actions racing to overwrite the same key, or TempData being read via `Peek` where `Keep` was needed for a subsequent request. Show me the diagnosis and proposed fix before implementing.

Fix it using consistent, centralized key names -- prefer constants (e.g., a small `static class TempDataKeys`) over magic strings scattered across controllers, so a rename or typo can't silently break the flow again. Where the same message needs to survive more than one redirect, use `TempData.Keep()` explicitly and comment why. Consider whether some of this state actually belongs in the view model or query string instead of TempData, if it's not truly transient cross-redirect state.

Confirm the fix does not change response status codes or existing successful flows. Write or update tests around the affected actions confirming TempData is set/read with the correct key in each success and failure path, then validate manually or via test that the banner appears exactly once and only where intended.
