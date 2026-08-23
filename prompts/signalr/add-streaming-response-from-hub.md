# Add Server-to-Client Streaming from a Hub

**Category:** SignalR
**Use when:** a client needs a continuous stream of results rather than a single response.

## Prompt

Implement server-to-client streaming for <describe the data, e.g., paginated search results / live sensor readings / long-running job progress> on the specified Hub method, using IAsyncEnumerable<T> (preferred for simple cases) or ChannelReader<T> (preferred if you need to push items from another thread/callback into the stream). Analyze the data source first -- is it naturally pull-based (a database cursor, a paged API) suited to IAsyncEnumerable with yield return, or push-based (events, a background producer) suited to a Channel<T> -- and propose which approach fits before implementing.

Requirements:
- Accept a CancellationToken parameter in the streaming method signature (SignalR injects one automatically bound to client-initiated cancellation) and pass it through to every downstream async call (DB queries, HTTP calls) so that if the client calls IStreamResult.dispose()/cancels, the server-side work actually stops instead of continuing to produce unread items.
- If using IAsyncEnumerable, mark the method async and use `await foreach` internally where consuming another async stream, applying [EnumeratorCancellation] on the token parameter if the token is threaded through a local iterator method.
- If using ChannelReader<T>, create a bounded Channel (not unbounded) to apply backpressure and avoid unbounded memory growth if the producer outpaces client consumption; complete the channel writer in a finally block so the stream terminates cleanly on both success and failure.
- Handle mid-stream errors: an exception thrown while streaming should surface to the client as a stream error (the client's stream.subscribe observer's error callback / try-catch around await foreach on the client), not silently truncate the stream -- verify what the actual client-side behavior is and document it.
- Consider authorization for the entire duration of the stream, not just at the start -- if the caller's permissions could change mid-stream (e.g., a long-running stream on a resource the user could be unassigned from), decide and document whether re-checking is required.
- Consider reconnection: streams do not survive a dropped/reconnected connection, so the client must be prepared to restart the stream (potentially with a resume token/offset) after reconnecting -- note this in the implementation rather than leaving it implicit.

After approval, implement it, then write a test that starts the stream, consumes a few items, cancels early, and confirms server-side resources (DB reader, channel) are disposed/completed -- not just that the happy path emits the expected items.
