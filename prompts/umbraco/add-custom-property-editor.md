# Build a Custom Property Editor

**Category:** Umbraco CMS
**Use when:** Editors need a specialized input UI in the backoffice that no built-in Data Type covers.

## Prompt

I need a custom Property Editor for the Umbraco backoffice because none of the built-in editors (Textstring, Textarea, Numeric, Dropdown, Content Picker, Block List, etc.) fit this use case. Before writing anything, analyze the requirement and propose whether this genuinely needs a custom editor or whether an existing editor plus a Data Type configuration (e.g., a Dropdown with prevalues, a Textstring with a regex validation pattern, or a Block List with a constrained Element Type) already solves it -- do not build a custom package for something Umbraco already supports out of the box.

If a custom editor is justified, propose:
1. Whether this targets the legacy AngularJS backoffice editor pattern or the newer package.manifest / Vite-based approach, matching whatever version of Umbraco and whichever pattern existing custom editors in this codebase already use (check `~/App_Plugins/` first).
2. The property editor's alias, icon, group, and whether it is a "property value converter" candidate (i.e., does the stored JSON/string need converting to a strongly-typed C# model for front-end consumption via `IPropertyValueConverter`).
3. The editor's client-side view (HTML/JS or Web Component), its model/value shape, and any validation rules enforced both client-side and server-side.
4. Data storage format (string, JSON, integer) and whether a custom `IDataEditor` and `IDataValueEditor` are required server-side, or whether a manifest-only client editor suffices.

Wait for my approval before implementing. On implementation, register the editor via `package.manifest` (or C# `DataEditor` attribute registration depending on the established pattern), implement the value converter if the raw stored value needs shaping for Razor views, and add validation so invalid values cannot be saved from the backoffice. Test that content saves/publishes correctly with the new editor, that existing content with no value for this property still renders without null-reference errors, and that the editor behaves correctly inside a Block List/Nested Content context if it will be used there. Note any upgrade-compatibility risk if this depends on internal/undocumented backoffice APIs.
