# Add Usage Metrics and Audit Log Monitoring for Embedded Reports

**Category:** Power BI
**Use when:** Stakeholders want visibility into who's using the embedded analytics feature.

## Prompt

Add usage metrics and audit-log monitoring for the embedded Power BI reports in this application so stakeholders can see adoption trends and the team can detect anomalous access patterns. Distinguish between two data sources up front and confirm with me which (or both) is actually wanted, since they answer different questions and come from different APIs:

1. **Embed-level usage (application-tracked):** events your own application controls -- who requested an embed token, for which report, when, and whether it succeeded. Implement this as application-level logging/telemetry using whatever this app already uses (Application Insights, existing structured logging, a metrics table) rather than introducing a new telemetry stack. Log the authenticated user identifier, report/dataset ID, and timestamp on every embed-token request; do not log the embed token or access token values themselves. If this app already has a request-logging middleware/filter pattern, extend it rather than adding bespoke logging calls scattered through the embedding code.

2. **Power BI-side usage and audit data (platform-tracked):** Power BI's own usage metrics reports (per-report, built into the service, showing views/viewers over time for users with access to the workspace) and the tenant-wide Activity/Audit Log (accessible via the Microsoft Purview compliance portal or the `GET /v1.0/myorg/admin/activityevents` admin API, which requires admin consent and elevated permissions -- do not attempt to call this with the same service principal used for embedding unless it has been explicitly granted the required admin API permissions, since that's a significant permission escalation worth calling out and confirming separately). If audit-log ingestion into this app's own reporting/alerting is wanted, propose a scheduled job (reusing existing background-job infrastructure) that pulls relevant `PowerBIReportUsage`/`GenerateToken`/`ViewReport` event types and stores summarized counts, not raw PII-heavy event payloads, unless there's a specific compliance need for the full detail.

3. **Anomaly detection (if requested):** define what "anomalous" means concretely for this use case (e.g. a single identity generating an unusually high volume of embed tokens in a short window, which could indicate credential misuse or a scraping attempt) before implementing any alerting threshold, and keep the initial thresholds conservative/configurable rather than hardcoded, tuning based on real observed traffic.

Confirm data retention and access-control requirements for any usage data stored (who can see which user viewed which report) before implementing, since usage data itself can be sensitive.
