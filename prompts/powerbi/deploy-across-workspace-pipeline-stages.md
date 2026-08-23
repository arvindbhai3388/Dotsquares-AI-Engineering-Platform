# Set Up a Deployment Pipeline Across Dev/Test/Prod Workspaces

**Category:** Power BI
**Use when:** Reports/datasets need a controlled promotion process across environments.

## Prompt

Set up a Power BI deployment pipeline to promote reports and datasets across Dev, Test, and Prod workspaces in a controlled way, with parameterized data source rules so each stage points at its own environment's data source without manual edits after each promotion. This spans both Power BI service configuration and, if automation is wanted, .NET code calling the Deployment Pipelines REST API -- clarify with me which is needed (one-time manual setup vs. an automated promotion trigger from this app's CI/CD) before starting.

Scope of work:

1. **Pipeline structure:** Confirm three workspaces exist (or will be created) for Dev, Test, and Prod, each on a capacity that supports the required features (deployment pipelines require Premium/Fabric capacity or Premium Per User on at least the stages involved) -- Pro-only workspaces cannot participate in a deployment pipeline.
2. **Deployment rules (parameterization):** For each dataset with an environment-specific data source (connection string, server name, or a Power Query parameter for environment), configure deployment rules at the Test and Prod stage level so that content deployed from Dev automatically repoints to the correct Test/Prod data source rather than requiring a manual credential/connection swap after every deployment -- this is the core mechanism that prevents Test or Prod from accidentally querying Dev data after a promotion.
3. **Credentials per stage:** Confirm each stage's dataset has its own valid data source credentials configured (credentials are not carried over automatically by the pipeline and must be set per workspace) -- a deployment can succeed while leaving the promoted dataset unable to refresh until credentials are (re)bound in the new stage.
4. **RLS carries over:** Confirm RLS roles defined on the Dev dataset are verified again after promotion to Test/Prod (see the dedicated RLS-testing prompt), since role definitions travel with the dataset but should still be re-validated against that stage's actual data before sign-off.
5. **Automation (if requested):** If promotion should be triggered from this app's existing CI/CD rather than done manually in the Power BI service UI, use the Deployment Pipelines REST API (`POST /v1.0/myorg/pipelines/{pipelineId}/Deploy`) via the same service-principal auth pattern already used elsewhere, gated behind an explicit approval step in the pipeline (e.g. a manual approval gate before Prod deployment) -- never auto-promote to Prod without an explicit human approval step, matching this platform's approve-before-implement discipline applied to the deployment process itself.

Document the final pipeline configuration (stages, rules, capacity assignment) so the next promotion doesn't require re-deriving this setup from scratch.
