# Package a Power Platform Solution and Set Up an ALM Pipeline

**Category:** Power Apps / Power Platform
**Use when:** Setting up a repeatable deployment process for Power Platform customizations.

## Prompt

Set up (or fix) the solution packaging and export/import pipeline for the Power Platform solution I specify, so customizations move through Dev -> Test -> Prod repeatably instead of via manual maker-portal exports. Ask first whether any pipeline already exists (Azure DevOps, GitHub Actions, or manual `pac` CLI steps) so we extend the existing approach rather than introducing a second, competing one.

Cover:
- **Unmanaged vs. managed**: confirm the Dev environment holds the unmanaged (source-of-truth, editable) solution, while Test and Prod only ever receive the managed solution build -- never import unmanaged into Test/Prod, since that creates unremovable customizations and makes future managed upgrades unreliable. State this explicitly if the current setup gets this backwards.
- **Solution versioning**: recommend a version-bump convention (e.g. major.minor.build.revision tied to the pipeline run number or ticket) and where that gets set (`solution.xml` via `pac solution version` or the maker portal) before each export.
- **CLI-driven export/pack/unpack**: the `pac solution export`, `pac solution unpack` (to get source-controllable XML/YAML instead of a single zip, so diffs are reviewable in PRs), `pac solution pack`, and `pac solution import --async` commands, with the correct flags for managed (`--managed`) vs unmanaged builds.
- **Connection references and environment variables**: the import step must supply a connection-mapping / environment-variable-values file (`--settings-file` on `pac solution import`) so Test/Prod get their own connection and variable values automatically rather than requiring a human to click through the import wizard each time -- reuse the environment-variable/connection-reference setup already in the solution rather than re-deriving it.
- **Solution checker**: recommend running `pac solution check` (or the maker-portal Solution Checker) as a pipeline gate before import, and treat high-severity findings as blocking.
- **Rollback plan**: state what "rollback" actually means for a managed solution (typically: import the previous managed solution version, since Power Platform doesn't support a clean managed-solution downgrade in all cases) and flag this limitation explicitly rather than implying a simple revert is always possible.

Propose the pipeline stages/commands as a checklist, get my approval on the unmanaged/managed boundary and versioning convention specifically (these are hard to change after the fact), then implement the pipeline definition file(s).
