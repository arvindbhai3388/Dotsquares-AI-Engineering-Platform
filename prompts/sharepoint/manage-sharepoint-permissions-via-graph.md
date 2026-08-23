# Manage Item/Folder-Level SharePoint Permissions via Graph

**Category:** SharePoint (Microsoft Graph)
**Use when:** An app needs to control sharing/access on specific files or folders.

## Prompt

Implement item- and folder-level permission management (grant, list, and revoke access) for files and folders in a SharePoint document library using Microsoft Graph, so this app can control sharing without users going into the SharePoint UI directly.

Requirements:
- For granting access, use `graphClient.Drives[driveId].Items[itemId].Invite.PostAsync()` for sending sharing invitations to specific users (supports `roles` like `read`/`write`, `requireSignIn`, and `sendInvitation`), and `CreateLink.PostAsync()` for generating anonymous or organization-wide sharing links (`view`/`edit` scope) — implement both as distinct operations since they have different security implications, and default to the most restrictive option (`requireSignIn: true`, specific-people links) unless the caller explicitly requests an anonymous link.
- For listing access, use `graphClient.Drives[driveId].Items[itemId].Permissions.GetAsync()` and map the returned `Permission` objects (which mix link-based and user-based grants) into a single clear DTO that distinguishes "shared with these named people" from "shared via link" so calling code and any UI can present it unambiguously.
- For revoking, use `DeleteAsync()` on the specific permission ID rather than trying to remove access by user identity alone — a user may have multiple distinct permission grants (direct + inherited via link), and only the correct one should be removed.
- Before granting broad access (e.g., an "everyone in the organization" link), require an explicit confirmation step in the calling workflow — do not let this be triggered by a default parameter value.
- Distinguish inherited permissions (inherited from the parent folder/library) from item-specific ones; Graph will not let you directly revoke an inherited permission — flag this case to the caller instead of failing silently or throwing an unhandled exception.
- Never log full recipient email addresses or sharing link URLs at anything above debug level, since sharing links can grant access to anyone who has the URL.
- Confirm the app registration has `Sites.FullControl.All` or a narrower selected-site + item-level grant sufficient for permission management, and prefer the narrower option.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose the DTO shape and the default-restrictive sharing behavior first — this is a security-sensitive feature — then implement with tests covering invite-specific-user, anonymous-link creation being explicitly opted into, permission listing with mixed grant types, and revocation of a specific permission ID.
