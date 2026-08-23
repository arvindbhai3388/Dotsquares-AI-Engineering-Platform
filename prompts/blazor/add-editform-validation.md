# Add EditForm Validation with DataAnnotations

**Category:** Blazor
**Use when:** A form needs client-side validation with proper inline error display before submission reaches the server.

## Prompt

Before implementing, confirm the model backing the form (existing DTO/view-model vs. a new one) and whether validation rules should live as `DataAnnotations` attributes on that model or require custom logic (see the custom-validation-attribute prompt if built-in attributes can't express the rule). Propose the model shape and validation approach for my approval first.

Wrap the form in `<EditForm Model="@model" OnValidSubmit="HandleValidSubmit" OnInvalidSubmit="HandleInvalidSubmit">` (or bind an explicit `EditContext` if the form needs to react to field-level changes via `EditContext.OnFieldChanged`/`OnValidationStateChanged`, e.g. to enable/disable a submit button reactively). Add `<DataAnnotationsValidator />` inside the form so attribute-based rules are actually evaluated, and place a `<ValidationSummary />` for form-level errors plus `<ValidationMessage For="@(() => model.PropertyName)" />` next to each field for inline errors.

Bind each input with the typed component that matches the property (`InputText`, `InputNumber`, `InputDate`, `InputSelect`, `InputCheckbox`) rather than raw `<input>` elements with manual `@bind`, since the typed components integrate with `EditContext` validation state and apply the `modified`/`valid`/`invalid` CSS classes Blazor generates automatically — style those classes rather than inventing new ones if this codebase already has form styling conventions.

For submit handling, keep `HandleValidSubmit` async and disable the submit button (or show a spinner) for the duration of the call to prevent double-submission; do not rely solely on client-side validation for security-sensitive rules — re-validate server-side regardless. If the form does async validation (e.g. checking uniqueness against an API), debounce the check and update `EditContext` via `Microsoft.AspNetCore.Components.Forms.ValidationMessageStore` rather than fighting the built-in validator. Add bUnit tests that submit valid and invalid data through the rendered form and assert the correct validation messages appear, plus a test confirming `OnValidSubmit` only fires when the model actually passes validation.
