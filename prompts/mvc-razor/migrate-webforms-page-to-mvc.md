# Plan and Execute a Web Forms Page to MVC Migration

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a client project has legacy Web Forms pages that need modernizing incrementally.

## Prompt

I have a legacy ASP.NET Web Forms page (`.aspx`/`.aspx.cs` code-behind) that needs to be migrated to MVC as part of an incremental modernization effort. This is a planning-heavy task -- do not start rewriting code before the Understand and Plan steps are complete and I've approved the approach, since Web Forms and MVC differ fundamentally in lifecycle (page lifecycle + ViewState + server controls vs. stateless request/response + explicit model binding).

Start by reading the `.aspx` markup and its code-behind fully: enumerate every server control (`asp:GridView`, `asp:FormView`, `asp:Repeater`, `UpdatePanel`, etc.) and what each one does, every event handler (`Page_Load`, button click handlers, `SelectedIndexChanged`, etc.) and what business logic it triggers, any use of `ViewState`/`Session` for cross-postback state, and any direct data access in the code-behind that will need to move into a controller/service layer. Produce a written mapping before implementing: which server control becomes which Razor/HTML equivalent (GridView -> a foreach loop over a view model collection, or a partial/tag helper; postback event handlers -> discrete controller actions; ViewState-carried state -> either resend via hidden fields, TempData, or a redesigned flow).

Flag anything that doesn't map cleanly (heavy `UpdatePanel`/AJAX postback reliance, third-party Web Forms controls, deeply nested master pages) and propose how each will be handled, rather than silently approximating behavior. Identify all business logic currently living in code-behind that must be extracted into a service/repository so it's testable and reusable, separate from the new controller.

Once I approve the mapping, implement incrementally (one page/flow at a time, not a big-bang rewrite) with the new controller, view model(s), and Razor view, preserving the original page's exact business rules and validation. Write unit tests for the extracted business logic and the new controller actions before considering the migration of that page complete, and confirm the new page is functionally equivalent to the original before it replaces it in navigation/links.
