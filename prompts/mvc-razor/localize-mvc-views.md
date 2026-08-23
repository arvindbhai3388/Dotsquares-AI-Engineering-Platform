# Localize MVC Views and Controllers

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a client requires multi-language UI support.

## Prompt

This client requires multi-language UI support and the views/controllers I point you to currently have hardcoded English strings. Before touching any file, check whether this project already has any localization infrastructure in place (resource files, `IStringLocalizer`/`IHtmlLocalizer` usage, a culture-selection mechanism in `Startup`/`Global.asax`, or a request-culture cookie/route convention) -- do not introduce a second, competing localization approach if one already exists partially; extend it consistently instead.

If no localization exists yet, propose the approach before implementing: resource files (`.resx`) per view/controller with the standard ASP.NET Core `IStringLocalizer<T>`/`IViewLocalizer` pattern, or classic MVC's `.resx` + `Resources.X` static access, matching whichever fits this project's framework (see the per-project TFM/type before choosing an API that doesn't exist in that framework version). Get my confirmation on the resource-file organization (one file per controller/view vs. shared resource files for common strings like button labels) before creating files, since this shape is tedious to restructure later.

Extract every hardcoded user-facing string (labels, validation messages, button text, error/success messages, `[Display(Name = "...")]` attributes on view models) into resource keys with a clear, consistent naming convention, and replace each usage with the localized lookup. Do not localize internal-only strings (logging, exception messages not shown to users) -- localizing those adds noise without user value. Pay special attention to validation attribute messages and any strings built via string concatenation/interpolation with parameters -- these need placeholder-based resource strings (`{0}`), not string-splicing around a translated fragment, since word order differs across languages.

Verify the culture-selection mechanism actually applies to the affected views (test by switching culture and confirming strings change, including validation messages), and confirm the default/fallback culture still renders correctly if a translation is missing for a given key.
