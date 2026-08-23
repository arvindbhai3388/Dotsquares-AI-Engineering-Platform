# Add a Custom DataAnnotations Validation Attribute

**Category:** Blazor
**Use when:** A business rule can't be expressed with built-in attributes like `[Required]`, `[Range]`, `[RegularExpression]`, or `[Compare]`.

## Prompt

Before implementing, write out the exact business rule in plain language and confirm it with me, including edge cases (null/empty handling, culture-sensitive parsing, whether the rule depends on other properties on the same model or on external state like "must not already exist in the database"). Propose whether this fits as a synchronous `ValidationAttribute` (self-contained, no I/O) or needs a different mechanism (async check via `EditContext.OnValidationRequested` plus a `ValidationMessageStore`, since `ValidationAttribute.IsValid` is synchronous and cannot await a database/API call) before implementing.

For a synchronous rule, create a class deriving from `ValidationAttribute`, override `IsValid(object? value, ValidationContext validationContext)`, and return `ValidationResult.Success` or a `new ValidationResult(errorMessage, new[] { validationContext.MemberName! })` so the error attaches to the correct field rather than showing only in `ValidationSummary`. If the rule depends on a sibling property (e.g. "EndDate must be after StartDate"), use `validationContext.ObjectInstance` to access the containing model, and place the attribute on the dependent property with a clear name explaining the cross-field relationship.

Make the error message configurable via the attribute's constructor or `ErrorMessage`/`ErrorMessageResourceType` properties rather than hardcoding user-facing text inside the attribute class, consistent with how other validation messages in this codebase are authored (check for existing localization/resource patterns before hardcoding English strings). Keep the attribute allocation-light and side-effect-free — it can run multiple times per keystroke under live validation, so avoid any expensive computation or logging inside `IsValid`.

If the rule genuinely requires async/external data, implement it instead via a method that runs on `EditContext.OnFieldChanged` or `OnValidSubmit`, populates a `ValidationMessageStore` keyed to the `FieldIdentifier`, and calls `EditContext.NotifyValidationStateChanged()` — do not attempt to fake async inside a synchronous `ValidationAttribute` with `.Result`/`.Wait()`, which risks deadlocks in Blazor Server's synchronization-context-bound circuit. Add unit tests directly against the attribute's `IsValid` method covering valid input, invalid input, null/empty, and (if applicable) the cross-field case, plus a bUnit test confirming the `EditForm` surfaces the correct validation message when the rule is violated.
