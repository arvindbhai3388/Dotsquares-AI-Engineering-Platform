# Add Graph Webhook Subscription for SharePoint List/Drive Changes

**Category:** SharePoint (Microsoft Graph)
**Use when:** The app needs near-real-time notification of SharePoint changes instead of polling.

## Prompt

Implement a Microsoft Graph webhook (change notification) subscription so this app is notified in near real time when items in a SharePoint list or drive change, replacing or supplementing a polling loop.

Requirements:
- Create the subscription via `graphClient.Subscriptions.PostAsync()` targeting the appropriate resource (`sites/{site-id}/lists/{list-id}/items` for a list, or `sites/{site-id}/drive/root` for a drive), with `changeType` set to `updated` (and `created`/`deleted` if the resource supports them) and `notificationUrl` pointing at a publicly reachable endpoint in this app.
- Implement the validation handshake required when Graph first calls the notification endpoint: the endpoint must echo back the `validationToken` query parameter as plain text with a 200 response within the timeout Graph enforces — this must work before Graph will accept the subscription creation.
- Secure the callback endpoint using `clientState` (a shared secret value set at subscription creation and checked on every incoming notification) so the endpoint rejects notifications that don't carry the expected value; do not hardcode this secret in source, generate/store it via existing config/secrets conventions.
- Remember that Graph subscriptions expire (max lifetime varies by resource type, often under 3 days for SharePoint resources) — implement a renewal job (e.g., a scheduled background task matching this app's existing worker/scheduling pattern) that calls `PATCH /subscriptions/{id}` with a new `expirationDateTime` well before expiry, and recreates the subscription if renewal fails outright.
- Note that change notifications are "something changed, go look" signals, not full payloads — on receipt, use the notification's `resource` and item ID to fetch the actual current item via Graph (respecting throttling/retry policy), rather than assuming the notification body has the full record. If richer payloads are configured (`includeResourceData`), require and validate encryption certificates per Graph docs.
- Persist active subscription IDs and their expiration so a restart doesn't create duplicate subscriptions, and clean up (`DELETE /subscriptions/{id}`) subscriptions that are no longer needed.
- Confirm least-privilege permissions cover subscription creation for the target resource type.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose the notification endpoint contract, clientState handling, and renewal schedule first, then implement with tests covering the validation handshake, a rejected notification with wrong clientState, and the renewal job's near-expiry trigger.
