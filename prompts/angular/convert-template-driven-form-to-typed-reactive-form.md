# Convert a Template-Driven Form to a Strictly Typed Reactive Form

**Category:** Angular
**Use when:** A `[(ngModel)]`-based form needs proper validation, easier testing, or is growing complex enough that template-driven binding is becoming hard to follow.

## Prompt

Read the existing template-driven form fully — every `ngModel`-bound field, every `#f="ngForm"` template reference used for validation display, and the component method that currently reads `f.value` or the bound model object on submit — and propose the target `FormGroup` shape (field names, types, and which validators map from the current manual/template validation) before converting. Call out any validation currently expressed only in the template (e.g. a `[disabled]` binding gating submit, or manual `*ngIf` error messages tied to `ngModel.errors`) so none of it is silently dropped in the conversion.

Build the form with strictly typed reactive forms (`FormGroup<T>`/`FormControl<T>`, available since Angular 14) — do not fall back to the untyped `FormGroup`/`AbstractControl` API. Define an explicit interface or type for the form's shape and construct it with `FormBuilder.group<YourFormShape>({...})` or, for non-nullable controls, `new FormControl(initialValue, { nonNullable: true, validators: [...] })` so `.value` isn't spuriously typed as including `null`. Use `Validators` (`required`, `email`, `minLength`, `pattern`, etc.) for static rules and a custom `ValidatorFn`/`AsyncValidatorFn` for anything cross-field or server-checked (e.g. password confirmation, username-availability), matching any existing custom validator convention in this codebase rather than inventing a new one.

In the template, replace `[(ngModel)]` with `[formControlName]`/`[formControl]` under a `[formGroup]` directive, and drive error display off `control.invalid && (control.dirty || control.touched)` rather than re-deriving validity state manually. Preserve the existing UX for when errors appear (on blur vs. on submit vs. live) unless the ticket asks to change it.

On submit, guard with `if (this.form.invalid) { this.form.markAllAsTouched(); return; }` before reading `this.form.getRawValue()` (use `getRawValue()` over `.value` if any control is disabled, since `.value` omits disabled controls). Write tests that set control values via `form.controls.field.setValue(...)`, assert `form.invalid`/`form.errors` for each validator, and assert submit is blocked while invalid and proceeds with the correctly shaped value once valid.
