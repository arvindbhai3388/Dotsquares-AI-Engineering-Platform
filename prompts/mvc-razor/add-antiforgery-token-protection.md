# Add or Verify Anti-Forgery Token Protection

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a new POST/PUT form is being added or an existing one lacks CSRF protection.

## Prompt

Review the form/action pair I point you to (or, if asked to audit broadly, search this controller/area for POST/PUT/DELETE actions) for CSRF protection. For each state-changing action, confirm three things line up: the action is decorated with `[ValidateAntiForgeryToken]` (or, for a Web API-style controller, the equivalent antiforgery middleware/attribute this project uses), the corresponding Razor view's `<form>` actually emits the token (`@Html.AntiForgeryToken()` in classic Razor, or the `asp-` tag helpers which include it automatically for `<form>` elements -- verify it's not a raw `<form>` tag that bypasses the tag helper), and any AJAX/fetch call that submits the form also attaches the token in a header or body field the action's model binder or filter actually reads.

Do not blindly add `[ValidateAntiForgeryToken]` everywhere -- check whether this project has a global antiforgery filter already applied (e.g., via `FilterConfig` or a base controller) that would make a second explicit attribute redundant, and check whether the action is a GET (which should never require or consume state-changing side effects, so antiforgery doesn't apply there). Flag, but do not silently "fix," any action that intentionally excludes antiforgery (e.g., a webhook receiver with its own signature validation) -- confirm with me before touching those.

For AJAX-submitted forms, verify the token is read from a hidden field or meta tag and sent as a header (commonly `RequestVerificationToken`) matching what the action expects, and that failures return a proper 400/401 rather than a generic 500.

After fixing, add or update a test that posts to the action without a valid token and asserts it is rejected, alongside the existing happy-path test, so a regression here fails a test rather than shipping silently.
