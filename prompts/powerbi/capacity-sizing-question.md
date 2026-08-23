# Recommend a Power BI Capacity SKU

**Category:** Power BI
**Use when:** Scoping infrastructure costs for a new embedded-analytics client engagement.

## Prompt

Analyze the expected usage pattern for a new embedded-analytics feature and recommend an appropriate Power BI capacity option -- an Embedded A/P SKU, a shared Premium (P) capacity, or Premium Per User (PPU) -- with reasoning I can take into a cost conversation with the client. This is an analysis/planning task, not a coding task, so stay in the analyze/propose stage and do not scaffold infrastructure yet.

Gather and reason through these inputs (ask me for any I haven't already given you rather than guessing):
- **Concurrency:** peak number of simultaneous users/sessions expected to have a report open at once, not total registered users -- this is the primary driver of v-core/backend memory needs.
- **Report complexity:** number of visuals per report, dataset size, whether models use Import mode, DirectQuery, or a composite model (DirectQuery multiplies backend load per interaction since each filter/slicer change re-queries the source).
- **Refresh frequency and volume:** how often datasets refresh, how many datasets, and whether refreshes will compete with interactive query load on the same capacity (Premium capacities have separate memory allocations per workload but still share the same v-cores).
- **Multi-tenancy shape:** is this "embed for your organization" (internal, AAD-authenticated) or "embed for your customers" (external, service-principal-based) -- this affects SKU eligibility (A SKUs are Embedded-only, via Azure; P SKUs are Power BI Premium, purchasable via M365; PPU requires each user to have a PPU license and does not support the embed-for-customers pattern at all).
- **Growth trajectory:** whether usage is expected to scale significantly in the next 6-12 months, since Embedded A SKUs can be paused/resized/scaled far more flexibly (hourly Azure billing) than annual Premium P capacity commitments.

Produce a short recommendation covering: the specific SKU tier (e.g. A4/A6, P1), the reasoning tied to the inputs above, the pause/scale flexibility tradeoffs, an approximate monthly cost range (call out that exact pricing must be confirmed against current Azure/Microsoft 365 pricing pages since it changes), and any embed-for-customers licensing constraints that rule certain SKUs out. Flag explicitly if PPU is being considered for an embed-for-customers scenario, since that combination is not supported.
