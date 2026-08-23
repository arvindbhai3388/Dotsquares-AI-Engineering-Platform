# Evaluate Reusing a Blazor Component Library in a .NET MAUI Hybrid App

**Category:** Blazor
**Use when:** A client wants a mobile app that reuses an existing Blazor UI investment via .NET MAUI Blazor Hybrid.

## Prompt

Do not write implementation code yet — this is an analysis and planning task. Read through the existing component library's project structure and produce a written assessment covering: which components are pure Razor Class Library components with no server-only dependencies (good candidates for direct reuse in `BlazorWebView`), which depend on Blazor Server-specific mechanisms (SignalR circuit assumptions, `HttpContext`, cookie-based auth) that would need the same treatment as the Server-to-WASM conversion prompt, and which depend on browser-only JS interop that has no equivalent in a MAUI native shell (e.g. `window.location`, browser storage APIs, specific DOM/CSSOM calls) versus interop that MAUI's `BlazorWebView` can still service since it does host a real web view.

Identify authentication/authorization differences: a MAUI hybrid app typically needs a native or MSAL-based auth flow rather than cookie/session auth used server-side, and any `AuthenticationStateProvider` implementation will need a MAUI-specific version. Identify navigation differences: `NavigationManager` still works inside `BlazorWebView`, but deep linking, back-button behavior, and platform lifecycle events (backgrounding, resume) are MAUI concerns with no Blazor Server/WASM equivalent and need explicit handling.

Flag platform-specific rendering differences worth testing early rather than assuming parity: `BlazorWebView` renders via the platform's native web view component (WebView2 on Windows, WKWebView on iOS, an Android WebView), so CSS/JS behavior can differ subtly per platform in ways that never surface in a browser-hosted Blazor app — call out any component using advanced CSS features, custom fonts, or JS APIs that should be spot-checked per target platform before assuming a clean lift-and-shift.

Estimate the actual scope: which components can move with zero changes, which need the WASM-compatibility treatment first, and which are not worth porting versus rebuilding natively for this platform (e.g. anything relying heavily on hover states or precise mouse interaction is a poor fit for touch). Present this as a phased plan (a small pilot component first, then a go/no-go decision) rather than proposing a big-bang port, and get my sign-off on scope before any component code is touched.
