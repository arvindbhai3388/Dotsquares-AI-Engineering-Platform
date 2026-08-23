# Add a Notification Handler for Content Lifecycle Events

**Category:** Umbraco CMS
**Use when:** Business logic needs to trigger automatically when content changes state (saved, published, unpublished, moved, deleted).

## Prompt

I need to add a notification handler that runs custom logic when specific content lifecycle events occur (e.g., `ContentPublishedNotification`, `ContentSavingNotification`, `ContentMovedNotification`, `ContentUnpublishedNotification`, or the corresponding Media/Member equivalents). First locate any existing `INotificationHandler<T>` implementations and their composer registration (`IComposer`/`builder.AddNotificationHandler<...>()`) in this codebase to match the established registration pattern and namespace location, and confirm which specific notification best fits the requirement -- "Saving" vs "Saved" and "Publishing" vs "Published" have different semantics (pre-commit and cancelable versus post-commit and final) and picking the wrong one causes subtle bugs.

Propose the plan before implementing:
1. The exact notification type and whether it needs to be cancelable (the "-ing" notifications implement `ICancelableNotification`, allowing the handler to block the save/publish by setting `Cancel = true` with a validation message) versus a fire-and-forget "-ed" notification for side effects after the fact.
2. What the handler does: keep it fast and non-blocking if it runs on the "-ing" (pre-commit) path, since it runs synchronously in the content-save/publish request; for slower side effects (calling an external API, sending email, syncing search index), prefer hooking the "-ed" notification and consider offloading genuinely slow work to a background queue rather than blocking the editor's Save/Publish click.
3. Filtering: the notification fires for ALL content of ALL Document Types by default, so the handler must check `IContent.ContentType.Alias` (or the equivalent Media/Member type check) early and return immediately for irrelevant content -- do not do expensive work before this filter.
4. Idempotency and error handling: what happens if this handler throws -- for a "-ed" notification this generally shouldn't block the already-completed operation, so wrap risky logic in try/catch and log rather than letting an unhandled exception surface to the editor as a save/publish failure.

Wait for approval, then implement the handler and its composer registration. Validate by triggering the actual lifecycle event in the backoffice (not just unit-testing the handler in isolation) for both a matching Document Type (logic runs) and a non-matching one (logic correctly skips), and confirm a thrown exception inside the handler doesn't break the editor's normal save/publish experience unless it was intentionally designed to cancel the operation.
