# Model a Flexible Content Section with Block List

**Category:** Umbraco CMS
**Use when:** A page needs flexible, reorderable content blocks (e.g., a landing page builder).

## Prompt

I need to model a repeatable, editor-friendly, flexible content area using Umbraco's Block List editor (not the deprecated Nested Content unless this codebase specifically still relies on Nested Content elsewhere and consistency matters more than using the newer editor -- check existing usage first and tell me which pattern this project already follows). Locate any existing Block List setups in this codebase for its Element Type structure, naming conventions, and view-rendering pattern before designing a new one.

Propose the plan:
1. The set of Element Types (Document Types marked "Is an Element Type", no template/permissions of their own) representing each block variant needed (e.g., "Hero Block", "Text and Image Block", "Testimonial Block", "Call to Action Block"), each with only the properties that block actually needs.
2. The Block List Data Type configuration: which Element Types are allowed in this particular Block List property, min/max block counts if bounded, custom labels for the block list item display (using `{{propertyAlias}}` label templates so editors can distinguish blocks in the collapsed list view), and whether "Inline editing mode" or the default overlay editing mode fits the editor UX best here.
3. Rendering strategy in the view: a partial-per-block-type dispatch pattern (a switch/lookup on the block's Content Type alias rendering `~/Views/Partials/Blocks/{Alias}.cshtml`) so adding a new block type later doesn't require modifying a giant if/else chain -- check if this project already has such a dispatch convention to follow.
4. Settings Element Type usage if blocks need editor-configurable display options (e.g., background color, spacing) separate from content data.

Wait for approval, then implement the Element Types, Data Type configuration, the parent property on the relevant page Document Type, the dispatch partial, and one partial view per block type. Handle the edge case of an empty Block List (no blocks added yet) rendering nothing rather than an empty wrapper, and a block whose Element Type was since deleted or changed (defensive handling so one bad block doesn't break the entire page render). Validate by adding, reordering, and removing blocks in the backoffice and confirming the front end reflects order and content correctly after publish.
