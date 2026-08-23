# Prompt Library

217 copy-paste-ready prompts for Claude Code, organized by stack/category. Every prompt is a self-contained instruction you can paste directly into a Claude Code session on a real client project. Each is written to fit this framework's core discipline:

```
Analyze → Propose → Approve → Implement → Test → Review
```

Most implementation prompts ask Claude to propose an approach and get your sign-off before changing code; the `architecture-and-planning` category is entirely about producing the plan/proposal artifact itself, with no implementation step.

## How to use these

1. Open the file for the task you're doing.
2. Copy everything under `## Prompt` into your Claude Code session.
3. Fill in the bracketed specifics (file/class/endpoint names, business rules) if the prompt calls for them.
4. Let Claude propose its approach first for anything non-trivial — don't skip straight to "just implement it."

## Categories

| Category | Directory | Count | Focus |
|---|---|---|---|
| ASP.NET Core | [`aspnet-core/`](aspnet-core/) | 24 | Web API / minimal APIs — validation, auth, middleware, resilience, background work |
| ASP.NET MVC / Razor Pages | [`mvc-razor/`](mvc-razor/) | 16 | Controllers, view models, tag helpers, partials, legacy Web Forms migration |
| Blazor | [`blazor/`](blazor/) | 21 | Server & WebAssembly components, JS interop, forms, testing, performance |
| Umbraco CMS | [`umbraco/`](umbraco/) | 16 | Document types, property editors, back office, caching, headless delivery |
| Entity Framework Core | [`efcore/`](efcore/) | 22 | Migrations, query performance, concurrency, multi-tenancy, testing strategy |
| SQL Server | [`sql-server/`](sql-server/) | 21 | Indexing, execution plans, locking/deadlocks, safe schema changes, RLS |
| SignalR | [`signalr/`](signalr/) | 16 | Hubs, groups, auth, scale-out, reconnection, strongly-typed clients |
| Power BI | [`powerbi/`](powerbi/) | 16 | Embedded analytics, RLS, embed tokens, refresh automation, deployment pipelines |
| SharePoint (Microsoft Graph) | [`sharepoint/`](sharepoint/) | 16 | Graph SDK CRUD, large file upload, delta queries, webhooks, throttling |
| Power Apps / Power Platform | [`powerapps/`](powerapps/) | 16 | Dataverse plugins, custom connectors, canvas/model-driven apps, ALM |
| Code Review & Testing | [`code-review-and-testing/`](code-review-and-testing/) | 21 | xUnit/MSTest/Moq, WebApplicationFactory, security review, flaky tests, coverage |
| Architecture & Planning | [`architecture-and-planning/`](architecture-and-planning/) | 12 | Implementation plans, ADRs, migration strategy, multi-tenancy design |

**Total: 217 prompts.**

---

## ASP.NET Core (`aspnet-core/`)

- [add-minimal-api-endpoint-with-validation.md](aspnet-core/add-minimal-api-endpoint-with-validation.md)
- [add-api-versioning.md](aspnet-core/add-api-versioning.md)
- [add-rate-limiting-middleware.md](aspnet-core/add-rate-limiting-middleware.md)
- [convert-sync-action-to-async.md](aspnet-core/convert-sync-action-to-async.md)
- [add-health-checks.md](aspnet-core/add-health-checks.md)
- [add-fluentvalidation-to-request-model.md](aspnet-core/add-fluentvalidation-to-request-model.md)
- [add-global-exception-handling-middleware.md](aspnet-core/add-global-exception-handling-middleware.md)
- [add-jwt-bearer-authentication.md](aspnet-core/add-jwt-bearer-authentication.md)
- [add-policy-based-authorization.md](aspnet-core/add-policy-based-authorization.md)
- [add-output-caching.md](aspnet-core/add-output-caching.md)
- [add-response-compression.md](aspnet-core/add-response-compression.md)
- [add-cors-policy.md](aspnet-core/add-cors-policy.md)
- [add-swagger-openapi-docs.md](aspnet-core/add-swagger-openapi-docs.md)
- [add-request-response-logging-middleware.md](aspnet-core/add-request-response-logging-middleware.md)
- [add-idempotency-key-support.md](aspnet-core/add-idempotency-key-support.md)
- [refactor-controller-to-thin-controller.md](aspnet-core/refactor-controller-to-thin-controller.md)
- [add-background-service-with-hosted-service.md](aspnet-core/add-background-service-with-hosted-service.md)
- [add-feature-flag.md](aspnet-core/add-feature-flag.md)
- [add-problemdetails-for-validation-errors.md](aspnet-core/add-problemdetails-for-validation-errors.md)
- [add-api-key-authentication-for-service-to-service.md](aspnet-core/add-api-key-authentication-for-service-to-service.md)
- [add-pagination-to-list-endpoint.md](aspnet-core/add-pagination-to-list-endpoint.md)
- [add-file-upload-endpoint.md](aspnet-core/add-file-upload-endpoint.md)
- [add-graceful-shutdown-handling.md](aspnet-core/add-graceful-shutdown-handling.md)
- [diagnose-memory-leak-web-api.md](aspnet-core/diagnose-memory-leak-web-api.md)

## ASP.NET MVC / Razor Pages (`mvc-razor/`)

- [scaffold-controller-action-with-viewmodel.md](mvc-razor/scaffold-controller-action-with-viewmodel.md)
- [map-entity-to-viewmodel.md](mvc-razor/map-entity-to-viewmodel.md)
- [clean-up-tempdata-usage.md](mvc-razor/clean-up-tempdata-usage.md)
- [extract-partial-view.md](mvc-razor/extract-partial-view.md)
- [add-custom-tag-helper.md](mvc-razor/add-custom-tag-helper.md)
- [add-antiforgery-token-protection.md](mvc-razor/add-antiforgery-token-protection.md)
- [add-client-side-validation-with-jquery-unobtrusive.md](mvc-razor/add-client-side-validation-with-jquery-unobtrusive.md)
- [add-area-to-mvc-app.md](mvc-razor/add-area-to-mvc-app.md)
- [add-custom-model-binder.md](mvc-razor/add-custom-model-binder.md)
- [add-action-filter-for-cross-cutting-concern.md](mvc-razor/add-action-filter-for-cross-cutting-concern.md)
- [convert-viewbag-to-strongly-typed-viewmodel.md](mvc-razor/convert-viewbag-to-strongly-typed-viewmodel.md)
- [add-file-download-action.md](mvc-razor/add-file-download-action.md)
- [add-razor-view-component.md](mvc-razor/add-razor-view-component.md)
- [add-multi-step-form-wizard.md](mvc-razor/add-multi-step-form-wizard.md)
- [localize-mvc-views.md](mvc-razor/localize-mvc-views.md)
- [migrate-webforms-page-to-mvc.md](mvc-razor/migrate-webforms-page-to-mvc.md)

## Blazor (`blazor/`)

- [new-component-with-parameters.md](blazor/new-component-with-parameters.md)
- [convert-blazor-server-component-for-wasm-reuse.md](blazor/convert-blazor-server-component-for-wasm-reuse.md)
- [add-cascading-parameters.md](blazor/add-cascading-parameters.md)
- [create-js-interop-wrapper.md](blazor/create-js-interop-wrapper.md)
- [add-editform-validation.md](blazor/add-editform-validation.md)
- [add-bunit-test-scaffolding.md](blazor/add-bunit-test-scaffolding.md)
- [add-blazor-state-container-service.md](blazor/add-blazor-state-container-service.md)
- [add-virtualize-for-large-list.md](blazor/add-virtualize-for-large-list.md)
- [add-authorization-authorizeview.md](blazor/add-authorization-authorizeview.md)
- [handle-blazor-circuit-disconnect-reconnect.md](blazor/handle-blazor-circuit-disconnect-reconnect.md)
- [add-loading-and-error-states-to-component.md](blazor/add-loading-and-error-states-to-component.md)
- [convert-razor-page-to-blazor-component.md](blazor/convert-razor-page-to-blazor-component.md)
- [add-custom-validation-attribute.md](blazor/add-custom-validation-attribute.md)
- [add-blazor-component-css-isolation.md](blazor/add-blazor-component-css-isolation.md)
- [add-lazy-loading-for-wasm-assembly.md](blazor/add-lazy-loading-for-wasm-assembly.md)
- [add-file-upload-component-inputfile.md](blazor/add-file-upload-component-inputfile.md)
- [add-signalr-integration-in-blazor-component.md](blazor/add-signalr-integration-in-blazor-component.md)
- [add-blazor-hybrid-maui-consideration.md](blazor/add-blazor-hybrid-maui-consideration.md)
- [optimize-blazor-server-render-performance.md](blazor/optimize-blazor-server-render-performance.md)
- [add-dependency-injection-scoped-service-blazor.md](blazor/add-dependency-injection-scoped-service-blazor.md)
- [add-prerendering-support.md](blazor/add-prerendering-support.md)

## Umbraco CMS (`umbraco/`)

- [add-document-type-and-view.md](umbraco/add-document-type-and-view.md)
- [add-custom-property-editor.md](umbraco/add-custom-property-editor.md)
- [use-content-picker-in-view.md](umbraco/use-content-picker-in-view.md)
- [add-output-caching-umbraco-page.md](umbraco/add-output-caching-umbraco-page.md)
- [add-member-authentication-flow.md](umbraco/add-member-authentication-flow.md)
- [review-umbraco-upgrade-compatibility.md](umbraco/review-umbraco-upgrade-compatibility.md)
- [add-custom-umbraco-controller-surface-controller.md](umbraco/add-custom-umbraco-controller-surface-controller.md)
- [add-umbraco-content-app.md](umbraco/add-umbraco-content-app.md)
- [add-media-picker-with-cropping.md](umbraco/add-media-picker-with-cropping.md)
- [add-nested-content-block-list.md](umbraco/add-nested-content-block-list.md)
- [add-umbraco-examine-search.md](umbraco/add-umbraco-examine-search.md)
- [add-scheduled-publishing-workflow.md](umbraco/add-scheduled-publishing-workflow.md)
- [add-umbraco-notification-handler.md](umbraco/add-umbraco-notification-handler.md)
- [add-multi-language-content-variant.md](umbraco/add-multi-language-content-variant.md)
- [add-headless-delivery-api-endpoint.md](umbraco/add-headless-delivery-api-endpoint.md)
- [secure-umbraco-backoffice-access.md](umbraco/secure-umbraco-backoffice-access.md)

## Entity Framework Core (`efcore/`)

- [add-safe-migration.md](efcore/add-safe-migration.md)
- [fix-n-plus-1-query.md](efcore/fix-n-plus-1-query.md)
- [add-optimistic-concurrency-token.md](efcore/add-optimistic-concurrency-token.md)
- [add-seed-data.md](efcore/add-seed-data.md)
- [configure-value-converter.md](efcore/configure-value-converter.md)
- [split-query-vs-single-query-decision.md](efcore/split-query-vs-single-query-decision.md)
- [add-soft-delete-global-query-filter.md](efcore/add-soft-delete-global-query-filter.md)
- [add-compiled-query-for-hot-path.md](efcore/add-compiled-query-for-hot-path.md)
- [configure-owned-entity-type.md](efcore/configure-owned-entity-type.md)
- [add-table-per-hierarchy-inheritance.md](efcore/add-table-per-hierarchy-inheritance.md)
- [add-index-via-fluent-api.md](efcore/add-index-via-fluent-api.md)
- [review-tracking-vs-no-tracking-queries.md](efcore/review-tracking-vs-no-tracking-queries.md)
- [add-bulk-update-delete-executeupdate.md](efcore/add-bulk-update-delete-executeupdate.md)
- [handle-migration-conflict-after-rebase.md](efcore/handle-migration-conflict-after-rebase.md)
- [add-interceptor-for-audit-fields.md](efcore/add-interceptor-for-audit-fields.md)
- [configure-connection-resiliency-retry.md](efcore/configure-connection-resiliency-retry.md)
- [add-shadow-property.md](efcore/add-shadow-property.md)
- [review-cascade-delete-behavior.md](efcore/review-cascade-delete-behavior.md)
- [add-dbcontext-pooling.md](efcore/add-dbcontext-pooling.md)
- [write-integration-test-with-inmemory-vs-sqlite.md](efcore/write-integration-test-with-inmemory-vs-sqlite.md)
- [add-multi-tenant-query-filter.md](efcore/add-multi-tenant-query-filter.md)
- [diagnose-slow-savechanges.md](efcore/diagnose-slow-savechanges.md)

## SQL Server (`sql-server/`)

- [missing-index-analysis.md](sql-server/missing-index-analysis.md)
- [rewrite-query-to-avoid-table-scan.md](sql-server/rewrite-query-to-avoid-table-scan.md)
- [convert-dynamic-sql-to-parameterized.md](sql-server/convert-dynamic-sql-to-parameterized.md)
- [stored-procedure-review.md](sql-server/stored-procedure-review.md)
- [deadlock-investigation.md](sql-server/deadlock-investigation.md)
- [add-covering-index.md](sql-server/add-covering-index.md)
- [analyze-execution-plan.md](sql-server/analyze-execution-plan.md)
- [implement-pagination-with-offset-fetch.md](sql-server/implement-pagination-with-offset-fetch.md)
- [add-row-level-security-policy.md](sql-server/add-row-level-security-policy.md)
- [review-transaction-isolation-level.md](sql-server/review-transaction-isolation-level.md)
- [add-temporal-table-for-audit-history.md](sql-server/add-temporal-table-for-audit-history.md)
- [optimize-bulk-insert.md](sql-server/optimize-bulk-insert.md)
- [add-full-text-search.md](sql-server/add-full-text-search.md)
- [review-implicit-conversion-performance.md](sql-server/review-implicit-conversion-performance.md)
- [design-partitioning-strategy.md](sql-server/design-partitioning-strategy.md)
- [add-computed-column-with-index.md](sql-server/add-computed-column-with-index.md)
- [review-lock-escalation.md](sql-server/review-lock-escalation.md)
- [safe-schema-change-large-table.md](sql-server/safe-schema-change-large-table.md)
- [tune-tempdb-contention.md](sql-server/tune-tempdb-contention.md)
- [review-merge-statement-safety.md](sql-server/review-merge-statement-safety.md)
- [add-json-column-query-support.md](sql-server/add-json-column-query-support.md)

## SignalR (`signalr/`)

- [add-new-hub-method.md](signalr/add-new-hub-method.md)
- [group-based-broadcast.md](signalr/group-based-broadcast.md)
- [handle-reconnect-client-side.md](signalr/handle-reconnect-client-side.md)
- [add-auth-to-hub.md](signalr/add-auth-to-hub.md)
- [scale-out-azure-signalr.md](signalr/scale-out-azure-signalr.md)
- [add-strongly-typed-hub-client.md](signalr/add-strongly-typed-hub-client.md)
- [add-hub-connection-lifecycle-logging.md](signalr/add-hub-connection-lifecycle-logging.md)
- [add-backplane-for-multi-instance.md](signalr/add-backplane-for-multi-instance.md)
- [add-streaming-response-from-hub.md](signalr/add-streaming-response-from-hub.md)
- [add-connection-mapping-user-to-connectionid.md](signalr/add-connection-mapping-user-to-connectionid.md)
- [add-hub-filter-for-cross-cutting-logic.md](signalr/add-hub-filter-for-cross-cutting-logic.md)
- [throttle-hub-method-invocations.md](signalr/throttle-hub-method-invocations.md)
- [add-client-reconnection-with-exponential-backoff.md](signalr/add-client-reconnection-with-exponential-backoff.md)
- [test-hub-with-integration-test.md](signalr/test-hub-with-integration-test.md)
- [migrate-signalr-classic-to-aspnetcore.md](signalr/migrate-signalr-classic-to-aspnetcore.md)
- [add-typing-indicator-presence-feature.md](signalr/add-typing-indicator-presence-feature.md)

## Power BI (`powerbi/`)

- [generate-embed-token.md](powerbi/generate-embed-token.md)
- [setup-rls-role.md](powerbi/setup-rls-role.md)
- [embed-report-in-blazor-mvc-page.md](powerbi/embed-report-in-blazor-mvc-page.md)
- [refresh-dataset-via-api.md](powerbi/refresh-dataset-via-api.md)
- [capacity-sizing-question.md](powerbi/capacity-sizing-question.md)
- [add-embed-for-customers-service-principal.md](powerbi/add-embed-for-customers-service-principal.md)
- [add-paginated-report-embedding.md](powerbi/add-paginated-report-embedding.md)
- [automate-dataset-refresh-schedule.md](powerbi/automate-dataset-refresh-schedule.md)
- [troubleshoot-embed-token-expiry.md](powerbi/troubleshoot-embed-token-expiry.md)
- [add-row-level-security-testing.md](powerbi/add-row-level-security-testing.md)
- [export-report-to-pdf-via-api.md](powerbi/export-report-to-pdf-via-api.md)
- [add-bookmarks-navigation.md](powerbi/add-bookmarks-navigation.md)
- [review-composite-model-performance.md](powerbi/review-composite-model-performance.md)
- [add-usage-metrics-monitoring.md](powerbi/add-usage-metrics-monitoring.md)
- [handle-power-bi-api-throttling.md](powerbi/handle-power-bi-api-throttling.md)
- [deploy-across-workspace-pipeline-stages.md](powerbi/deploy-across-workspace-pipeline-stages.md)

## SharePoint / Microsoft Graph (`sharepoint/`)

- [graph-app-registration-walkthrough.md](sharepoint/graph-app-registration-walkthrough.md)
- [list-crud-via-graph-sdk.md](sharepoint/list-crud-via-graph-sdk.md)
- [large-file-upload-via-graph.md](sharepoint/large-file-upload-via-graph.md)
- [delta-query-change-tracking.md](sharepoint/delta-query-change-tracking.md)
- [throttling-retry-policy-with-polly.md](sharepoint/throttling-retry-policy-with-polly.md)
- [add-webhook-subscription-for-list-changes.md](sharepoint/add-webhook-subscription-for-list-changes.md)
- [search-files-across-site-with-graph-search-api.md](sharepoint/search-files-across-site-with-graph-search-api.md)
- [manage-sharepoint-permissions-via-graph.md](sharepoint/manage-sharepoint-permissions-via-graph.md)
- [add-client-credentials-vs-delegated-auth-decision.md](sharepoint/add-client-credentials-vs-delegated-auth-decision.md)
- [sync-sharepoint-list-to-sql-database.md](sharepoint/sync-sharepoint-list-to-sql-database.md)
- [add-caml-query-for-complex-filter.md](sharepoint/add-caml-query-for-complex-filter.md)
- [handle-graph-api-pagination.md](sharepoint/handle-graph-api-pagination.md)
- [upload-download-with-conflict-resolution.md](sharepoint/upload-download-with-conflict-resolution.md)
- [add-sharepoint-embedded-container-integration.md](sharepoint/add-sharepoint-embedded-container-integration.md)
- [migrate-csom-to-graph-sdk.md](sharepoint/migrate-csom-to-graph-sdk.md)
- [add-site-provisioning-automation.md](sharepoint/add-site-provisioning-automation.md)

## Power Apps / Power Platform (`powerapps/`)

- [custom-connector-openapi-definition.md](powerapps/custom-connector-openapi-definition.md)
- [dataverse-plugin-skeleton.md](powerapps/dataverse-plugin-skeleton.md)
- [canvas-app-data-source-wiring.md](powerapps/canvas-app-data-source-wiring.md)
- [model-driven-app-entity-design-review.md](powerapps/model-driven-app-entity-design-review.md)
- [add-power-automate-flow-trigger-from-api.md](powerapps/add-power-automate-flow-trigger-from-api.md)
- [add-dataverse-webhook-vs-plugin-decision.md](powerapps/add-dataverse-webhook-vs-plugin-decision.md)
- [add-custom-connector-authentication.md](powerapps/add-custom-connector-authentication.md)
- [add-business-rule-vs-plugin-decision.md](powerapps/add-business-rule-vs-plugin-decision.md)
- [add-power-fx-formula-for-canvas-app.md](powerapps/add-power-fx-formula-for-canvas-app.md)
- [add-dataverse-alternate-key.md](powerapps/add-dataverse-alternate-key.md)
- [add-environment-variable-for-connector.md](powerapps/add-environment-variable-for-connector.md)
- [add-plugin-error-handling-and-tracing.md](powerapps/add-plugin-error-handling-and-tracing.md)
- [add-solution-packaging-for-alm.md](powerapps/add-solution-packaging-for-alm.md)
- [add-custom-api-in-dataverse.md](powerapps/add-custom-api-in-dataverse.md)
- [integrate-power-apps-with-external-sql-via-virtual-table.md](powerapps/integrate-power-apps-with-external-sql-via-virtual-table.md)
- [add-real-time-workflow-vs-async-plugin.md](powerapps/add-real-time-workflow-vs-async-plugin.md)

## Code Review & Testing (`code-review-and-testing/`)

- [review-diff-for-security-issues.md](code-review-and-testing/review-diff-for-security-issues.md)
- [generate-xunit-tests-for-existing-method.md](code-review-and-testing/generate-xunit-tests-for-existing-method.md)
- [add-integration-tests-with-webapplicationfactory.md](code-review-and-testing/add-integration-tests-with-webapplicationfactory.md)
- [refactor-for-testability.md](code-review-and-testing/refactor-for-testability.md)
- [code-review-checklist-pass.md](code-review-and-testing/code-review-checklist-pass.md)
- [add-mstest-tests-for-legacy-method.md](code-review-and-testing/add-mstest-tests-for-legacy-method.md)
- [add-moq-based-unit-tests-for-service.md](code-review-and-testing/add-moq-based-unit-tests-for-service.md)
- [add-mutation-testing-pass.md](code-review-and-testing/add-mutation-testing-pass.md)
- [add-test-data-builder-pattern.md](code-review-and-testing/add-test-data-builder-pattern.md)
- [add-contract-tests-for-external-api.md](code-review-and-testing/add-contract-tests-for-external-api.md)
- [add-snapshot-tests-for-api-response.md](code-review-and-testing/add-snapshot-tests-for-api-response.md)
- [review-for-thread-safety-issues.md](code-review-and-testing/review-for-thread-safety-issues.md)
- [add-load-test-scenario.md](code-review-and-testing/add-load-test-scenario.md)
- [add-code-coverage-gate.md](code-review-and-testing/add-code-coverage-gate.md)
- [review-diff-for-performance-regressions.md](code-review-and-testing/review-diff-for-performance-regressions.md)
- [add-architecture-tests-with-netarchtest.md](code-review-and-testing/add-architecture-tests-with-netarchtest.md)
- [generate-tests-for-edge-cases-and-boundary-values.md](code-review-and-testing/generate-tests-for-edge-cases-and-boundary-values.md)
- [add-approval-tests-for-legacy-code.md](code-review-and-testing/add-approval-tests-for-legacy-code.md)
- [review-pr-for-breaking-api-changes.md](code-review-and-testing/review-pr-for-breaking-api-changes.md)
- [add-fake-vs-mock-decision-for-test.md](code-review-and-testing/add-fake-vs-mock-decision-for-test.md)
- [audit-test-suite-for-flaky-tests.md](code-review-and-testing/audit-test-suite-for-flaky-tests.md)

## Architecture & Planning (`architecture-and-planning/`)

- [produce-implementation-plan-for-feature.md](architecture-and-planning/produce-implementation-plan-for-feature.md)
- [evaluate-schema-change-for-backward-compatibility.md](architecture-and-planning/evaluate-schema-change-for-backward-compatibility.md)
- [document-cross-service-flow.md](architecture-and-planning/document-cross-service-flow.md)
- [propose-caching-strategy.md](architecture-and-planning/propose-caching-strategy.md)
- [evaluate-build-vs-buy-for-integration.md](architecture-and-planning/evaluate-build-vs-buy-for-integration.md)
- [produce-adr-for-technical-decision.md](architecture-and-planning/produce-adr-for-technical-decision.md)
- [plan-strangler-fig-migration.md](architecture-and-planning/plan-strangler-fig-migration.md)
- [assess-technical-debt-in-module.md](architecture-and-planning/assess-technical-debt-in-module.md)
- [design-multi-tenant-isolation-strategy.md](architecture-and-planning/design-multi-tenant-isolation-strategy.md)
- [plan-zero-downtime-deployment.md](architecture-and-planning/plan-zero-downtime-deployment.md)
- [evaluate-event-driven-vs-request-response.md](architecture-and-planning/evaluate-event-driven-vs-request-response.md)
- [produce-onboarding-doc-for-new-developer.md](architecture-and-planning/produce-onboarding-doc-for-new-developer.md)
