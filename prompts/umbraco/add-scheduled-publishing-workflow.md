# Set Up Scheduled Publish/Unpublish Workflow

**Category:** Umbraco CMS
**Use when:** Content needs to go live or expire automatically at a specific time.

## Prompt

I need to enable and correctly support scheduled publishing (release date) and scheduled unpublishing (expire date) for time-sensitive content in this Umbraco site. This is largely a built-in Umbraco backoffice feature (the "Schedule" panel in the content editor's Info/Publish flow), so first confirm it isn't already fully functional and identify the actual gap: is the requirement to expose scheduling for a Document Type where it's currently disabled, to add custom logic that runs when a scheduled publish/unpublish actually fires, or to surface scheduled-status information somewhere the backoffice doesn't already show it (e.g., a "content publishing soon" dashboard)?

Propose a plan covering:
1. Confirming the relevant Document Type(s) allow scheduling (this is generally available by default in the content editor unless restricted by permissions) and that the background job responsible for processing scheduled publishes (`ScheduledPublishing` hosted service, run via Umbraco's internal recurring background jobs) is enabled and running -- do not build a custom cron/timer to replace this; it already exists.
2. If custom logic must run when a scheduled publish/unpublish actually executes (e.g., send a notification email, sync to an external system, trigger a cache purge): implement it via an `INotificationHandler<ContentPublishedNotification>` and `INotificationHandler<ContentUnpublishedNotification>`, checking whether the notification's context indicates it originated from the scheduled job versus a manual editor action if that distinction matters to the requirement.
3. **Time zone handling**: release/expire dates are stored and evaluated against server time -- flag clearly if editors are in a different time zone than the server and whether the backoffice UI needs a time zone indicator or conversion to avoid content going live at the wrong local time.
4. Edge cases: a release date in the past (should publish on the next scheduled job run, not silently skip), an expire date set before a release date (invalid state -- confirm Umbraco's built-in validation catches this), and content that is manually unpublished before its scheduled expire date (scheduled job should not error on already-unpublished content).

Wait for approval before implementing any custom notification handler. Validate by scheduling a test publish/unpublish a few minutes out (do not rely purely on manual "Publish Now" testing, since that bypasses the scheduled code path) and confirming the correct end state and any custom logic fired exactly once.
