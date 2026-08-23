# Add Environment Variables and Connection References for Cross-Environment ALM

**Category:** Power Apps / Power Platform
**Use when:** Preparing a solution for ALM across multiple environments.

## Prompt

Add environment variables and a connection reference to the Power Platform solution I specify, so the app/flow's connection details (base URLs, tenant-specific IDs, feature flags) vary correctly across Dev/Test/Prod without anyone manually reconfiguring the app or flow after each import. Start by asking which specific values currently differ per environment (or are currently hardcoded in a flow action or canvas app formula) -- do not guess which values need to become variables.

For each value that should be an environment variable:
- Choose the correct data type (Text, Number, Boolean, JSON, Secret, or Data source) -- use type "Secret" (backed by Azure Key Vault) for anything sensitive rather than plain Text, and explain that Secret-type environment variables require a Key Vault connection to be configured, which is a one-time environment-level setup, not something achievable purely from the maker portal.
- Set a sensible "current value" only in the Dev/unmanaged solution; explicitly do NOT set a current value in the managed solution shipped to Test/Prod -- each target environment's admin sets the environment-specific value after import, which is the entire point of using an environment variable instead of a hardcoded string.
- Reference the environment variable correctly from the consuming flow (via the "Get environment variable value" pattern or direct dynamic content reference) or canvas app (via the `EnvironmentVariableDefinitions`/values in Power Fx) -- show the exact expression.

For the connection itself:
- Create a connection reference (not a raw connection) inside the solution, so the flow/app points at a logical reference that gets rebound to a real, environment-specific connection at import time via the Power Platform import UI or `pac solution import` connection-mapping parameters.
- Explain the import-time step a Test/Prod admin must perform (map each connection reference to a live connection in that environment) since this cannot be fully automated from the definition alone without a deployment pipeline that supplies connection mappings.

Summarize the full list of new environment variables and connection references with their intended values per environment as a table, get my approval, then add them to the unmanaged solution, and note explicitly what still requires a human step (Key Vault setup, per-environment connection mapping) versus what is fully captured in the solution.
