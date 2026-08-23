# Add Language/Culture Content Variants

**Category:** Umbraco CMS
**Use when:** A site needs multi-language support for existing or new content.

## Prompt

I need to add multi-language support so editors can manage translations of the same content node via Umbraco's culture variants. First determine the current state: are any languages already configured under Settings > Languages, are any Document Types/properties already marked as "Culture Variant" (vs. "Invariant"), and how does the current URL structure/domain-per-culture routing work (or not exist yet)? Do not assume a greenfield setup -- check for partial variant configuration already in place.

Propose the plan before implementing:
1. Language configuration: which cultures to add, which is the default/fallback culture, and whether "Mandatory" is required on any non-default language (blocks publishing until that culture has required fields filled).
2. Per-property variance: for each relevant Document Type, decide which properties should be culture-variant (translatable text, images that differ by market) versus invariant (properties that should be identical across all languages, like a shared category picker or SKU) -- changing a property's variance setting on a Document Type with existing content has migration implications (Umbraco will prompt to move/copy the invariant value into a chosen culture), so flag this explicitly.
3. Domain/URL strategy: culture-per-domain (e.g., site.com vs site.fr) via Umbraco's domain configuration on the content root nodes, versus culture-in-path (site.com/fr/) -- confirm which routing model existing infrastructure (DNS, CDN, existing `IUrlProvider` customizations) supports.
4. Fallback behavior when a translation doesn't exist for a given culture: Umbraco's built-in culture fallback chain (configurable per-request), and what the front end should show (fallback content vs. a "not available in this language" message) -- this needs to be an explicit decision, not left to default behavior nobody verified.
5. Backoffice UX: the language switcher in the content tree, and translation workflow considerations (does this need Umbraco's built-in "Translate" send-for-approval flow or is direct multi-culture editing by the same editors sufficient).

Wait for approval before implementing. Validate: content in the default culture still renders unchanged, a new culture with only partial translations falls back correctly per the agreed rule, publishing one culture doesn't inadvertently publish/affect another, and the language switcher/URLs resolve to the correct node per culture.
