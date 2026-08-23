# Decide Between Synchronous (Real-Time) and Asynchronous Execution

**Category:** Power Apps / Power Platform
**Use when:** It's unclear whether logic must complete before the triggering operation returns to the user.

## Prompt

Analyze the latency and transactional requirements of the logic I describe (state the trigger message/entity and what the logic does) and decide whether it must run synchronously (real-time, blocking the triggering Create/Update/Delete until it completes) or can run asynchronously (queued as a system job, running after the user's operation has already returned). Do not implement or register the step until this is decided and confirmed with me.

Work through these factors explicitly:
- **Transactional integrity**: if the logic must succeed or fail atomically with the triggering operation (e.g. a validation that should prevent the save entirely, or a calculated field the user must see immediately on the same screen), it has to be synchronous and pre-operation or post-operation-within-the-transaction -- async plugins run in a separate transaction after the original save has already committed, so they cannot roll back the original operation.
- **User-perceived latency**: if the logic involves a slow external call (an outbound HTTP request, a large calculation, a call to another Dataverse table with heavy processing), running it synchronously will make the end user wait for the save/form-save to complete -- recommend async unless the transactional-integrity requirement above forces sync, and quantify the tradeoff (e.g. "sync adds an estimated Xms to every save of this table" if you can estimate it from the operation described).
- **Failure visibility**: sync plugin failures surface immediately in the UI as an error dialog the user sees right away; async plugin failures land in a System Job record that nobody sees unless something is actively monitoring failed async jobs -- if we choose async, recommend how failures will actually be noticed (e.g. a monitoring flow, alerting on failed system jobs) rather than leaving them silently invisible.
- **Ordering and race conditions**: if this logic depends on or is depended on by other synchronous logic on the same message/entity, check execution-stage/order conflicts (pre-validation vs. pre-operation vs. post-operation, and the numeric execution order among steps in the same stage) so we don't introduce a race condition by mixing sync and async steps that touch the same fields.
- **Classic workflow alternative**: if async is the right choice and the logic is simple enough, note that a Power Automate cloud flow may be a better fit than an async plugin for maintainability, per the business-rule/workflow/plugin decision criteria.

Present the recommendation with reasoning as a short table, get my approval, then configure the plugin step registration (message, stage, execution mode, execution order, filtering attributes) accordingly, and note the failure-monitoring plan if async was chosen.
