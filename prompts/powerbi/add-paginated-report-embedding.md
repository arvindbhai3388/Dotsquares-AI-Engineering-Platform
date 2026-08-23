# Embed a Paginated (RDL) Report

**Category:** Power BI
**Use when:** A client needs pixel-perfect, printable reports (invoices, statements) rather than interactive dashboards.

## Prompt

Add support for embedding a paginated report (RDL-based, e.g. an invoice or statement layout) in this application, alongside or instead of the existing interactive Power BI report embedding. Paginated reports use a different embed type and a subset of the REST API surface from interactive reports, so treat this as its own flow rather than assuming the existing interactive-report embed code can be reused unchanged.

Before implementing, confirm with me: (a) whether the paginated report already exists in the target workspace (paginated reports are authored in Power BI Report Builder, not something to generate from code), and (b) whether this requires Premium/Embedded capacity (paginated report embedding, like interactive embedding, requires a paid capacity -- it is not available on a shared/free workspace).

Implementation requirements:
- Reuse the existing service-principal/MSAL authentication code path from the interactive-report embedding work rather than duplicating auth logic.
- Call the paginated-report-specific `GenerateToken` endpoint (`POST /v1.0/myorg/groups/{groupId}/reports/{reportId}/GenerateToken`) -- note the request body and supported token scopes differ slightly from interactive reports (no `EffectiveIdentity`/RLS support for paginated reports in the same way; access control instead comes from the calling identity's Power BI permissions on the report, or from parameters passed at render time).
- On the frontend, use the `powerbi-client` library's paginated-report embed type (`models.reportType: 'paginated'` equivalent config) or the paginated-report-specific viewer, since the interactive report's rendering surface (visuals, filters pane, bookmarks) does not apply to paginated reports.
- Support passing report parameters at embed time if the RDL report defines any (e.g. a customer ID or invoice number parameter), validating and sanitizing any parameter value that originates from user input before passing it through, since paginated report parameters are a potential injection surface if the underlying report has parameterized data-source queries.
- If the client needs a downloadable copy rather than just an on-screen view, note that this is a separate concern -- point me at the export-to-PDF prompt in this library rather than conflating the two in this task.

Confirm expected page size/orientation behavior and printable output fidelity as part of Validate, since paginated reports are specifically meant to render identically across screen and print/export.
