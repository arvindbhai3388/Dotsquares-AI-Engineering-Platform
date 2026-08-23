# Add a Surface Controller for Front-End Form Handling

**Category:** Umbraco CMS
**Use when:** A page needs custom form handling (e.g., a contact form) integrated with Umbraco's content pipeline.

## Prompt

I need a Surface Controller to handle a front-end form submission (e.g., contact form, newsletter signup, quote request) that posts back into an Umbraco-rendered page rather than a separate API endpoint. First locate any existing Surface Controllers in this codebase to match naming, namespace, and error-handling conventions, and identify the Document Type/view where the form will live.

Propose the plan before implementing:
1. Controller class extending `Umbraco.Cms.Web.Website.Controllers.SurfaceController`, with a strongly-typed view model (not passing `IPublishedContent` fields loosely into the form) and `[HttpPost]`/`[ValidateAntiForgeryToken]` on the submit action.
2. Server-side validation via data annotations or `ModelState`, mirroring/complementing any client-side validation already used elsewhere in the site, plus explicit handling for the invalid-submission case (`return CurrentUmbracoPage()` with `ModelState` errors so the same page re-renders with the user's entered values and validation messages, rather than a generic error page).
3. What happens on success: redirect-after-post (`RedirectToCurrentUmbracoPage` or `RedirectToUmbracoPage` to a "thank you" page) to prevent duplicate submissions on refresh, persisting the submission (to `AS_`-style custom table via existing data-access patterns, to Umbraco Forms if that package is installed, or emailing via the site's existing mail service) -- check which persistence mechanism already exists rather than introducing a new one.
4. Anti-spam/anti-bot protection (honeypot field, rate limiting, or CAPTCHA) matching what other forms on the site already use.
5. Whether the form partial needs to be embedded via `Html.BeginUmbracoForm()` so the routing correctly targets the Surface Controller action from any page.

Wait for approval, then implement. Validate: successful submission end-to-end (data persisted/emailed as designed), validation-error submission redisplays the form with entered values preserved and correct error messages, CSRF token is present and enforced, and the form works correctly when the hosting page itself is under Public Access/member restriction if applicable. Confirm no sensitive form data is written to logs.
