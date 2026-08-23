# Decide Between a Dataverse Plugin and a Webhook for External Notification

**Category:** Power Apps / Power Platform
**Use when:** Dataverse changes need to notify or update an external system.

## Prompt

Analyze the integration requirement I describe (a Dataverse table change needs to notify or push data to an external system -- name the table, the message, and the external system/endpoint) and decide whether it should be delivered as a Dataverse plugin calling out directly, a registered webhook step, or an Azure Service Bus/Event Grid integration behind a plugin. Do not implement anything until this decision is made and I've approved it.

Base the recommendation on:
- **Coupling and blast radius**: a plugin making a direct outbound HTTP call inside its `Execute` method ties the Dataverse transaction's latency and reliability to the external system's availability -- if the external call is slow or flaky, this can degrade or fail the Dataverse operation for end users. A webhook (HTTP endpoint registered as a step target) or a queued integration (plugin drops a message on Service Bus, a separate subscriber calls the external system) decouples this.
- **Delivery guarantees**: webhooks and direct plugin calls are fire-once with no built-in retry/backoff on Dataverse's side (a webhook that returns non-2xx will cause the platform to retry for synchronous registrations only under specific conditions) -- if the external system needs guaranteed eventual delivery, recommend the queued pattern instead and say so explicitly.
- **Payload shape control**: a webhook receives the full plugin execution context as it's registered (entity images, message name) with less flexibility to reshape the payload; a plugin has full programmatic control to call the external system with a custom-shaped payload and can call multiple downstream systems from one registration.
- **Sync vs async execution**: state explicitly whether this must block the Dataverse save (sync) or can happen after (async, the common case for "notify an external system") and register the step accordingly.
- **Security**: how the external endpoint authenticates the caller (API key, mutual TLS, HMAC signature) and how that secret is stored (never in plugin unsecure configuration -- use secure configuration or an external secret store).

Present the comparison as a short decision table (approach -> pros -> cons -> recommended for this case), get my sign-off on the chosen approach, then implement it following the appropriate skeleton (plugin skeleton or webhook registration notes) and the Test-First workflow for the plugin/handler code involved.
