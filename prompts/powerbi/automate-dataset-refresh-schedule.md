# Configure and Validate a Scheduled Dataset Refresh

**Category:** Power BI
**Use when:** Setting up ongoing data freshness for a published dataset.

## Prompt

Configure a scheduled refresh for a published Power BI dataset and build the supporting failure-handling/alerting so a silently-failing schedule doesn't go unnoticed. Unlike ad-hoc on-demand refresh triggered from application code, this is about the dataset's built-in refresh schedule (times/days configured via the Power BI service or the REST API's dataset refresh-schedule endpoint) plus the monitoring layer around it -- clarify which one I actually need before implementing if it's ambiguous.

Scope of work:
1. **Schedule configuration:** Using the `PATCH /v1.0/myorg/groups/{groupId}/datasets/{datasetId}/refreshSchedule` endpoint (or documented manual steps if this is a one-time setup rather than something to automate), set the refresh frequency/times/timezone/notify options appropriate for the data source's update cadence. Respect the plan's refresh-count limits (8/day shared Pro capacity, 48/day Premium) -- if the required freshness exceeds these limits, flag that this needs the on-demand API-triggered refresh pattern instead (see the separate `refresh-dataset-via-api` prompt) and confirm which approach I want before proceeding.
2. **Credential/gateway validation:** Confirm the dataset's data source credentials and, if applicable, the on-premises data gateway are configured and healthy -- scheduled refreshes fail silently from the application's perspective if the underlying data source connection has expired credentials or an offline gateway, so this must be checked as part of setup, not assumed.
3. **Failure alerting:** Implement (or wire into existing infrastructure) a check for refresh failures using `GET /v1.0/myorg/groups/{groupId}/datasets/{datasetId}/refreshes` on a periodic basis (e.g. a lightweight scheduled health-check job, reusing this app's existing background-job infrastructure), and raise an alert through the existing notification channel (email, Teams, logging/monitoring pipeline) when the most recent scheduled refresh status is "Failed" -- do not rely solely on Power BI's built-in email notifications if this app already has a centralized alerting mechanism, since failures should surface where the team actually looks.
4. **Never hardcode data source credentials** anywhere in this configuration or alerting code -- data source credentials are managed within the Power BI service/gateway configuration, not application config files.

Write a test (or a documented manual verification step, if there's no test project for this kind of job yet) confirming the failure-check job correctly distinguishes "Failed", "Completed", and "no refresh has run yet" states, since misclassifying these produces either false alarms or missed failures.
