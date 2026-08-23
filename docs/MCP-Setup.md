# MCP Setup

What MCP is in the context of this platform, why credentials never live here, and how to adapt `templates/mcp-baseline.json` for a new client project.

## What MCP is, here

The Model Context Protocol (MCP) is how Claude Code connects to external systems as callable tools — an issue tracker, a wiki, an internal API — beyond the file/shell tools it already has. A client project wires this up in its own `.mcp.json` at its repo root; Claude Code reads that file and exposes each configured server's tools automatically in that project.

This platform repo does not ship a live `.mcp.json` of its own, because MCP wiring is inherently per-client: the servers, endpoints, and credentials a project needs depend entirely on that client's actual tooling (which Jira instance, which Azure DevOps org, which internal API). What this platform does provide is a **credential-free starting shape** — see below.

## Why credentials stay per-client-project

Same rule as everywhere else in this framework: **security > convenience**. An MCP server config routinely needs a URL and an auth token/header to do anything useful, and those values are specific to one client's tenant — they must never be committed to this shared platform repo, and they must never be copied from one client project into another. Concretely:

- `templates/mcp-baseline.json` in this repo contains only placeholder tokens (`<MCP_SERVER_URL>`, `<MCP_AUTH_TOKEN>`, etc.) — never a real endpoint or credential.
- A client project's real `.mcp.json`, once filled in with actual URLs/tokens, follows the same rule as `appsettings.Development.json`: it belongs in that project's own gitignored/local configuration or its secret store, not hardcoded and committed. See [Security Guidelines](Security-Guidelines.md) for where secrets belong in each environment.
- If a real MCP server needs a bearer token or API key, prefer resolving it from an environment variable at launch rather than writing the literal value into `.mcp.json`, and confirm the file (or the specific keys holding real values) is excluded from source control the same way `appsettings.*.json` would be.

## Adapting `templates/mcp-baseline.json` for a new client repo

1. Copy `templates/mcp-baseline.json` to the client project's root as `.mcp.json`.
2. Remove the `_comment` keys — they're documentation only, not a Claude Code setting, the same convention used in `templates/permissions-baseline.json`.
3. Rename the example server entries (`project-tracker`, `team-wiki`, `local-tooling`) to match what the client project actually uses, and delete any entry that doesn't apply.
4. Replace every `<PLACEHOLDER>` with the client's real values, sourcing anything sensitive (auth tokens, API keys) from an environment variable or the client's secret store — never typed in as a literal.
5. Confirm the filled-in `.mcp.json` (or at least the keys holding real credentials) is covered by that project's `.gitignore` before committing anything, and never commit a real URL/token even temporarily "to test it."
6. Verify the server loads by starting Claude Code in that project and confirming the expected tools are available, before relying on it for real work.

## Related pages

- [Getting Started](Getting-Started.md)
- [Claude Code Setup](Claude-Code-Setup.md)
- [Security Guidelines](Security-Guidelines.md)
