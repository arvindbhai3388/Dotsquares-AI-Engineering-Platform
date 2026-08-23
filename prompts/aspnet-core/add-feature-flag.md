# Introduce a Feature Flag Around a New or Risky Code Path

**Category:** ASP.NET Core
**Use when:** a change needs to ship dark or be toggled per environment/tenant.

## Prompt

Analyze the code path I need flagged: the exact boundary where old and new behavior diverge, whether `Microsoft.FeatureManagement` (or another flag system) is already referenced in this codebase, and if so its existing conventions (flag naming, where flags are declared in configuration, how `IFeatureManager`/`[FeatureGate]` is already used elsewhere) — match that rather than introducing a second flagging mechanism. If no flag system exists, propose the smallest viable option (a simple configuration-bound boolean via `IOptions<T>` versus adding `Microsoft.FeatureManagement`) based on whether this needs to be just an on/off switch or something more sophisticated (percentage rollout, per-tenant targeting).

Propose the flag design before implementing: the flag's exact name (following existing naming convention if one exists), its scope (global, per-environment via configuration, per-tenant/per-user if targeting filters are needed), the default state when the flag is absent/misconfigured (default to the safe/old behavior, not the new path), and where the branch point lives in code — prefer a single, clearly-named branch point over scattering `if (flagEnabled)` checks across multiple layers.

Once approved, implement:
- Wire the flag check at the narrowest correct point — ideally in one place (a service method or middleware decision), not duplicated across controller, service, and view layers.
- Ensure the new code path and the old code path both remain fully functional and independently testable while the flag exists; do not let the "temporary" branch silently become the only supported path without cleanup being tracked.
- Never gate a security control behind a flag defaulting to the less-secure state.
- Make the flag's current value observable (log it at startup or expose it in a diagnostics/health endpoint) so it's clear at runtime which path is active.

Write or update tests covering both the flag-on and flag-off paths explicitly (not just whichever is currently default), and a test confirming the documented default behavior when the flag is unset. Confirm with me on the flag's default value and rollout scope before merging, and flag (in your response, not in code) that flag cleanup should be tracked as follow-up work once the feature is fully rolled out.
