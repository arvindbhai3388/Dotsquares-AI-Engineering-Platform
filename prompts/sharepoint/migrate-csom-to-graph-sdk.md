# Migrate Legacy CSOM Code to Microsoft Graph SDK

**Category:** SharePoint (Microsoft Graph)
**Use when:** Modernizing an older SharePoint integration off CSOM/SharePoint Online Management Shell.

## Prompt

Migrate the legacy CSOM (Client-Side Object Model, `Microsoft.SharePoint.Client`) code in this integration to the `Microsoft.Graph` SDK. Locate the existing CSOM usage first (`ClientContext`, `Web`, `List`, `ListItem`, `File`, `SharePointOnlineCredentials` or `AuthenticationManager` patterns) and inventory every distinct operation it performs before writing replacement code, since a partial migration that leaves some paths on CSOM and silently drops error handling on others is worse than not migrating.

Requirements:
- Map each CSOM operation to its Graph equivalent explicitly and call out where there is no direct 1:1 mapping (e.g., certain CSOM-only features like specific workflow or fine-grained CAML scenarios) — for those, either find the closest Graph equivalent or explicitly decide to keep CSOM for just that operation, isolated behind the same interface as everything else (see add-caml-query-for-complex-filter for the fallback pattern). Do not assume every CSOM call has a Graph equivalent without checking.
- Replace CSOM's `ClientContext.ExecuteQueryAsync()` batching model with direct async Graph SDK calls; note that Graph does not batch implicitly the way CSOM does, so if the CSOM code relied on batching several operations into one network round-trip, use Graph's `BatchRequestContentCollection`/`$batch` support to preserve that efficiency rather than issuing many more individual HTTP calls than before.
- Replace the authentication mechanism entirely: CSOM typically used `SharePointOnlineCredentials` or ACS app-only tokens, which are being deprecated/retired by Microsoft. Set up proper Azure AD app registration and `GraphServiceClient` construction as described in the graph-app-registration-walkthrough prompt rather than trying to reuse the old credential model.
- Preserve the existing public interface/contract of the service being migrated wherever possible, so callers elsewhere in the app don't need to change, per this repo's backward-compatibility expectations — this is an internal implementation swap, not a behavior change, unless a behavior difference is unavoidable (call those out explicitly).
- Remove the CSOM NuGet package reference only after confirming no remaining code path uses it, to avoid leaving a dead but still-referenced dependency.
- Re-verify least-privilege Graph permissions needed for the migrated operations; CSOM's permission model doesn't map 1:1 to Graph scopes, so do not just request the broadest Graph permission that "covers everything" as a shortcut.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: deliver the CSOM-to-Graph operation mapping and flag any operations without a clean equivalent first, then migrate incrementally with tests confirming each migrated operation's behavior matches the original CSOM behavior before removing the old code path.
