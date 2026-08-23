# Add a Members-Based Authentication Flow

**Category:** Umbraco CMS
**Use when:** A site needs gated content or a customer portal area.

## Prompt

I need to add login, registration, and restricted-content access using Umbraco's built-in Members system (not a separate ASP.NET Identity/custom user table, unless one is already in use here -- check first whether this codebase has custom membership infrastructure before assuming vanilla Umbraco Members). Locate the existing Member Types, any existing login/register Surface Controllers, and how `Umbraco.Cms.Web.Website.Security` (`MemberSignInManager`, `MemberManager`) is or isn't already wired in.

Propose a plan covering:
1. The Member Type(s) needed (fields beyond the built-in email/username/password) and whether existing Member Types can be reused or extended.
2. Surface Controllers for login and registration, following Umbraco's `SurfaceController` pattern with `[ValidateAntiForgeryToken]` on POST actions, using `MemberSignInManager`/`MemberManager` for sign-in and account creation rather than manual cookie handling.
3. Content restriction (Public Access) on the relevant content nodes/sections -- via the backoffice Public Access settings pointing at a login/error page, or set programmatically via `IPublicAccessService` if this needs to be applied dynamically.
4. Password requirements, email confirmation flow if required, and account lockout behavior, matching whatever `MemberPasswordConfiguration` already exists in configuration (do not hardcode policy values that belong in options/config -- and do not read or expose secrets from restricted config files; use the strongly-typed options pattern).
5. Session/anti-forgery handling and CSRF protection on all form posts, and explicit handling of the "already logged in" and "not authorized -- redirect to login" cases.

Wait for my approval before implementing. On implementation, use Umbraco's member sign-in/out APIs rather than raw cookie manipulation, ensure passwords are never logged, and add a redirect-back-to-original-page flow after login. Validate: successful registration, duplicate-email registration rejection, correct login, wrong-password rejection, access denied for anonymous users on restricted content, and correct access once authenticated. Confirm backoffice member management (viewing/editing members) still functions correctly after the change.
