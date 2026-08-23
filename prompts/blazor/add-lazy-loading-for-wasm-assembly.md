# Configure Lazy Loading for a Blazor WebAssembly Assembly

**Category:** Blazor
**Use when:** A WebAssembly app's initial load is slow because of a large upfront download payload.

## Prompt

Before implementing, profile which assemblies actually contribute the most to initial load size (browser dev tools network tab against the `.wasm`/`.dll` downloads, or `dotnet-trace`/build output size analysis), and confirm with me which feature area(s) are good lazy-load candidates — good candidates are self-contained, infrequently used, route-gated features (an admin section, a reporting module) referenced from a limited set of entry points; poor candidates are assemblies referenced from the app's shell/layout that load on every page anyway, since lazy-loading those adds latency without saving anything.

In the WASM client project file, mark the target assembly (and any assembly-only-referenced-by-it that shouldn't load eagerly) with `<BlazorWebAssemblyLazyLoad Include="{AssemblyName}.dll" />` item entries. In `App.razor`, replace the plain `<Router AppAssembly="...">` with a `<Router AppAssembly="..." AdditionalAssemblies="@lazyLoadedAssemblies" OnNavigateAsync="OnNavigateAsync">` where `OnNavigateAsync` calls `LazyAssemblyLoader.LoadAssembliesAsync(assemblyNames)` for the specific route being navigated to, then adds the resulting `Assembly` objects into `lazyLoadedAssemblies` before the router resolves the route — the router needs the assembly's routable components registered before it can match the incoming route.

Show a loading indicator during `OnNavigateAsync` for routes that trigger a lazy load, since the network fetch for the assembly is a real, sometimes-multi-hundred-KB download that will otherwise look like a frozen navigation. Handle load failure (offline, blocked request) — wrap the `LoadAssembliesAsync` call in try/catch and surface a retry/error UI rather than leaving the router in a partially-navigated state.

Verify with a build (`dotnet publish`) that the lazy-loaded assembly is actually excluded from the initial `blazor.boot.json` eager list and only appears under the lazy-load manifest — a misconfigured item group silently falls back to eager loading with no error. Confirm any DI services or components in the lazy-loaded assembly aren't referenced eagerly elsewhere (a shared layout, a cascading value producer), which would force early loading regardless of the configuration. Test the actual UX: navigate to the lazy-loaded route on a throttled network profile and confirm the loading indicator shows, the route resolves after the download, and back/forward navigation with the assembly already cached doesn't re-download it.
