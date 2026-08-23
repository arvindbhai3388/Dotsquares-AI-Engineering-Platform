# Export a Report to PDF/PPTX via the REST API

**Category:** Power BI
**Use when:** Users need to download/email a static copy of a live report.

## Prompt

Implement a feature letting users export a live embedded Power BI report to a static PDF or PPTX file via the REST API's export-to-file flow, since this is an asynchronous, poll-for-completion operation and not a simple synchronous download. Locate this app's existing embed-token/service-principal auth code and background-job or async-task conventions first, and reuse them rather than building a parallel auth path.

Implementation flow:
1. **Start the export:** Call `POST /v1.0/myorg/groups/{groupId}/reports/{reportId}/ExportTo` with the desired format (`PDF` or `PPTX`) and, if needed, a `PowerBIReportConfiguration` specifying which report page(s) to include and any filter/bookmark state that should be applied to the exported output (e.g. if the user is exporting exactly what they currently see filtered to, not the report's default state) -- clarify with me whether "export what I'm looking at" (current filters) or "export the default report" is the required behavior, since this materially changes the request payload.
2. **Poll for completion:** The export runs asynchronously; poll `GET /v1.0/myorg/groups/{groupId}/reports/{reportId}/exports/{exportId}` on a backoff interval (e.g. 2-5 seconds, increasing) until status is "Succeeded" or "Failed" -- do not block a web request thread on this for a long-running export; use this app's existing async/background-job pattern and return a job ID to the client immediately, with the client polling your own backend or receiving a completion notification (SignalR/websocket if already used elsewhere in this app, otherwise simple polling).
3. **Retrieve the file:** On success, call `GET /v1.0/myorg/groups/{groupId}/reports/{reportId}/exports/{exportId}/file` to stream the generated file back, and pass it through to the user (direct download or email attachment, matching what was requested) without buffering the entire file in memory if it could be large.
4. **Failure and throttling handling:** Handle "Failed" export status with the API's returned error detail, and handle 429 throttling on both the start-export and poll calls with retry-with-backoff (large/complex reports and concurrent export requests are a common throttling trigger for this specific API).
5. **Security:** Enforce that the requesting user is actually authorized to view the report/data being exported before starting the export -- exporting must go through the same authorization checks as viewing the embedded report, not bypass them.

Write tests around the start/poll/retrieve state machine using a mocked HTTP client, covering success, failure, and timeout paths.
