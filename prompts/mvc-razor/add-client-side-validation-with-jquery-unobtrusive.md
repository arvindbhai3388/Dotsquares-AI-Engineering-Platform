# Wire Up jQuery Unobtrusive Client-Side Validation

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a form only validates server-side today, causing a poor UX round trip.

## Prompt

The form I point you to currently only validates on the server -- the user submits, gets a full page round trip, and sees errors after the fact. Add client-side validation using jQuery Validation + jQuery Unobtrusive Validation, matching how this is already wired up elsewhere in the project if a working example exists (check `_Layout.cshtml`/bundle config for `jquery.validate.js` and `jquery.validate.unobtrusive.js` references and reuse that same bundle rather than adding a second copy of the libraries).

Confirm the view model's properties already carry the right data annotations (`[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]`, `[Compare]`, `[EmailAddress]`) since unobtrusive validation generates its client-side rules from these -- do not duplicate validation logic in hand-written JavaScript when a data annotation would generate the same rule automatically. Only add a custom `IClientValidatable`-style adapter (or the ASP.NET Core `IClientModelValidator` equivalent) if a validation rule genuinely can't be expressed by an existing built-in attribute, and check whether the project already uses one before writing a new one.

Verify the form is rendered with `asp-for`/`Html.EditorFor`/`Html.TextBoxFor` tag helpers or HTML helpers that emit the `data-val-*` attributes unobtrusive validation reads -- a hand-written `<input>` without these will not validate client-side even with the scripts loaded. Confirm `@Scripts.Render`/`<script>` includes for jquery, jquery.validate, and jquery.validate.unobtrusive load in the correct order and only once per page (duplicate script blocks are a common cause of unobtrusive validation silently failing).

Server-side `ModelState.IsValid` checks must remain in place and unchanged -- client-side validation is a UX improvement, never a substitute for server-side validation, since it can be bypassed. Test both paths: submitting with JavaScript disabled still gets rejected server-side, and submitting with invalid data in a browser shows inline errors without a round trip.
