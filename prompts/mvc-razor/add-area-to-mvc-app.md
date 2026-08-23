# Add a New Area to the MVC Application

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a new feature module doesn't belong in the default routing/folder structure.

## Prompt

I need a new logically separate feature module added as an MVC Area rather than crowded into the default controller/view structure. Before creating anything, confirm with me the area's name and scope, and check whether this project already has any existing Areas (look for an `Areas/` folder) so the new one matches their internal folder layout (`Controllers/`, `Models/` or `ViewModels/`, `Views/`) and naming conventions exactly rather than inventing a new structure.

Create the area folder structure (`Areas/<AreaName>/Controllers`, `Areas/<AreaName>/Views`, and view model location consistent with the rest of the project), including a `Views/<AreaName>/_ViewStart.cshtml` and `Views/<AreaName>/Web.config`/`_ViewImports.cshtml` (matching whichever the project uses -- classic ASP.NET MVC 5 needs the Web.config for view compilation, ASP.NET Core needs `_ViewImports.cshtml`) so views resolve correctly. Register the area route: for classic MVC, an `AreaRegistration` subclass registered in `Global.asax`/`AreaRegistration.RegisterAllAreas()`; for ASP.NET Core, the area-aware route in `MapControllerRoute`/attribute routing with `[Area("AreaName")]` on controllers. Get my confirmation on the routing approach before implementing since it affects every link generated into the area.

Add the first controller and at least one view to prove the area resolves correctly (view discovery for areas has different default search paths than the default area, and this is the most common source of a "view not found" error after setup). Verify links into the area from outside it correctly specify `area = "AreaName"` in `Html.ActionLink`/`asp-area` so they don't silently fall through to the default area's routes.

Validate by running the app (or the project's existing smoke-test approach) and hitting the new area's route directly, confirming it resolves to the correct controller/view and that existing routes outside the area are unaffected.
