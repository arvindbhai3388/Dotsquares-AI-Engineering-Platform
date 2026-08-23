# Secure Backoffice User Group Access and Permissions

**Category:** Umbraco CMS
**Use when:** A client needs role-based restrictions on who can edit/publish specific content.

## Prompt

I need to restrict and audit backoffice access so specific User Groups can only see/edit/publish content within a defined section of the content tree, and cannot perform actions (e.g., publish, delete, permission changes) beyond their role. First inventory the current state: existing User Groups, their assigned "Start Nodes" (content and media), section access (Content, Media, Settings, Members, etc.), and granular permissions (Browse, Update, Publish, Create, Delete, Permissions) already configured, plus whether any content nodes have node-level permission overrides applied on top of group-level defaults.

Propose the plan before making any changes:
1. Which User Group(s) need new/changed Start Node restrictions to scope their content-tree visibility, and confirm Start Nodes restrict visibility but do not by themselves prevent a user from being granted broader permissions elsewhere -- both must be set consistently.
2. Which granular permissions to grant per group per content area (e.g., an "Editors" group with Update+Browse but not Publish, requiring a separate "Approvers" group with Publish rights, implementing a review-before-publish workflow using Umbraco's built-in permission model rather than custom code).
3. Section access: confirm groups needing only Content access don't also have Settings/Users/Packages access, which would let them alter Document Types, Data Types, or other users' permissions beyond their intended role -- this is a common over-permissioning mistake.
4. Whether any node-level permission overrides already exist that would conflict with or be redundant against the new group-level settings, and reconcile rather than layering more overrides on top of an already-inconsistent state.
5. Audit trail: confirm Umbraco's built-in audit log (`IAuditService`) is retained/reviewed for permission-sensitive actions (publish, delete, permission changes) if the client needs accountability, rather than building a custom logging mechanism.

Wait for my approval before changing any User Group or node permission, since this can immediately lock editors out of content they rely on. After implementing, validate by logging in as (or having someone log in as) a representative user from each affected group and confirming they see exactly the tree scope and action buttons intended -- no more, no less -- and that previously-working editors are not unintentionally locked out.
