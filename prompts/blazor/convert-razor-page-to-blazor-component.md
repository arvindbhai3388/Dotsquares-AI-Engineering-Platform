# Convert a Server-Rendered Razor Page/MVC View to a Blazor Component

**Category:** Blazor
**Use when:** A page needs richer client-side interactivity (live validation, partial updates, real-time data) than plain server rendering provides.

## Prompt

Before converting anything, read the existing Razor Page/MVC view plus its code-behind or controller action, and produce a plan mapping each responsibility to its Blazor equivalent: `PageModel.OnGet`/controller GET action -> `OnInitializedAsync`/`OnParametersSetAsync`; `OnPost`/POST action -> an `EditForm` with `OnValidSubmit`; `ViewData`/`ViewBag`/`TempData` -> component parameters, injected state, or query-string-driven `[Parameter, SupplyParameterFromQuery]`; tag helpers -> Blazor form components (`InputText`, `InputSelect`, etc.). Get my approval on this mapping and on the target hosting model (Server, WASM, or interactive server render mode in a Razor Pages/MVC hybrid app) before writing code.

Identify anything the original view relied on that a Blazor component does not get for free: `HttpContext` access (cookies, headers, request path) needs to go through an injected abstraction rather than direct access; model binding attributes (`[FromQuery]`, `[FromRoute]`) map to `[Parameter, SupplyParameterFromQuery]`/route parameters on `@page` directives; anti-forgery token handling differs (Blazor Server's SignalR circuit provides its own connection security, but any separate API calls still need their own auth). Flag any use of full-page navigation/redirects (`RedirectToAction`) that should become `NavigationManager.NavigateTo(...)` instead.

Preserve the existing URL/route if external links or bookmarks depend on it — replicate the original `@page`/route template on the new component's `@page` directive. If the page currently returns different content based on server-side conditions the client shouldn't see (e.g. admin-only fields), do not simply move that logic client-side unguarded — combine with `AuthorizeView`/server-side authorization as covered in the authorization prompt, since Blazor component code (even in Server, and especially in WASM) is far more inspectable than a server-rendered view's final HTML.

Reproduce validation behavior using `EditForm`/`DataAnnotationsValidator` (see the EditForm validation prompt) rather than porting manual `ModelState` checks. Test the converted component against the same scenarios the original view/controller had test coverage for (if any exist), add bUnit tests for the new component's initial render and form submission paths, and do a side-by-side manual comparison of both pages to confirm no field, validation message, or navigation behavior was dropped in the conversion.
