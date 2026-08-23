# Wire Up Canvas App Data Source Connections

**Category:** Power Apps / Power Platform
**Use when:** Connecting a new canvas app screen to backend data.

## Prompt

Help me wire a canvas app screen to the data source(s) I specify (Dataverse table, SQL Server table/view, or a custom connector). Start by asking what the screen needs to do (browse/search/filter, form-based create/edit, or a dashboard read) and which data source(s) are involved, since the delegation and connection approach differs significantly between them.

For each data source, produce:
- The exact `Connector.Add`/`Connector.Update`/`Connector.Remove` or Dataverse table reference pattern to use in `OnStart`/`OnVisible` versus relying on implicit data source binding, matching how this app's other screens are already structured (inspect existing screens/data sources first rather than assuming a pattern).
- A concrete Power Fx formula for the gallery/form `Items` property that filters and sorts correctly, written to stay within delegation limits for that specific data source (Dataverse delegates most standard operators; SQL Server delegation is narrower and does not cover things like nested `AND`/`OR` combinations with certain functions, `RelativeDate`, or client-side-only functions like `Sort` on non-delegable columns).
- An explicit list of delegation warnings I should expect to see in Power Apps Studio for this formula, and what each one means in practice (e.g. data silently truncated at 500/2000 rows vs. a real logic error).
- Guidance on connection references vs. hardcoded connections, so the screen behaves correctly when the app is exported/imported into Test or Prod as part of a managed solution (see the environment-variable/connection-reference ALM pattern used elsewhere in this app, and reuse it rather than inventing a new one).
- Any security-role implications: if this canvas app runs under a Dataverse security role that restricts row/column visibility, flag which fields/rows might silently disappear for lower-privileged users rather than erroring.

Follow analyze -> propose -> approve -> implement: show me the proposed formulas and delegation analysis before you (or I) apply them in Studio, since canvas app changes are typically applied by a human in the Power Apps Studio UI rather than as a file edit -- treat your output as the exact text to paste into the formula bar, not as a diff to a project file, unless the app source is checked into source control as `.pa.yaml`/msapp-unpacked files in this repo, in which case edit those directly.
