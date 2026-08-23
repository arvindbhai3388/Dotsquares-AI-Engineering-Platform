# Review Umbraco Upgrade Compatibility

**Category:** Umbraco CMS
**Use when:** Planning an Umbraco version upgrade (e.g., v8 to v10+, or across LTS versions).

## Prompt

We are planning to upgrade this Umbraco installation to a newer major/LTS version and I need a compatibility review before any upgrade work starts. Do not modify any code yet -- this is an analysis task; produce a findings report I can approve a remediation plan against.

Scan the codebase (scoped to the actual Umbraco solution, not vendored/generated folders) for the following upgrade risk categories, and cite specific files/lines for each finding:

1. **Framework baseline**: is this on .NET Framework (Umbraco 7/8, requiring a full rewrite to .NET/ASP.NET Core for v9+) or already on .NET Core/.NET (v9+)? This determines whether the upgrade is an in-place version bump or a platform migration.
2. **Deprecated/removed APIs**: usage of `UmbracoApiController` vs `UmbracoApiControllerBase`, `ApplicationContext.Current`/`UmbracoContext.Current` static singletons (removed in favor of DI in v9+), `IContentService`/`IPublishedContentQuery` signature changes across versions, and any use of internal/obsolete-marked APIs (`[Obsolete]` attributes) already flagged by the compiler.
3. **Property editor and package.manifest format**: AngularJS-based custom backoffice editors (v8 and earlier pattern) that need migrating to the new backoffice framework in v14+/v15+, if the target version is that recent.
4. **Custom Examine index configuration**, custom `IPublishedContentModelFactory`/ModelsBuilder usage, and any custom `IContentFinder`/`IUrlProvider` implementations -- these interfaces have changed shape across major versions.
5. **Third-party packages** (list them with current version) and whether each has a known compatible release for the target Umbraco version -- flag any with no upgrade path as a project risk.
6. **Database/schema assumptions**: raw SQL against Umbraco's internal tables (`umbracoNode`, `cmsContentXml`/`cmsContentNu`, etc.) which change between versions and are unsupported to query directly.
7. **Configuration format**: `umbracoSettings.config`/web.config-based configuration (pre-v9) versus `appsettings.json`-based configuration -- do not open or quote actual restricted config file contents; only note that migration is required and reference official Umbraco upgrade documentation for the target version's config keys.

Summarize findings as a prioritized risk list (blocking vs. advisory) with an estimated remediation approach per item, and explicitly flag anything that looks like a full front-end/backoffice rewrite versus a mechanical fix.
