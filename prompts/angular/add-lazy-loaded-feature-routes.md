# Set Up Lazy-Loaded Feature Routes

**Category:** Angular
**Use when:** A feature area is currently bundled into the eager/main chunk and should load on demand, or a new feature area is being added and should be lazy from the start.

## Prompt

Analyze the current routing setup (`provideRouter(routes)` in `app.config.ts`, or an existing `app-routing.module.ts` if this app predates the standalone bootstrap style) and the feature's actual dependency footprint (how large its component tree is, whether it pulls in a heavy third-party library only it uses) before proposing the lazy-loading boundary — report the expected bundle-size impact qualitatively (e.g. "pulls in the charting library, worth splitting out") rather than assuming every feature is worth its own chunk.

For a single standalone component that's a natural route leaf, lazy-load it directly with `loadComponent: () => import('./feature.component').then(m => m.FeatureComponent)` — don't wrap a single component in an unnecessary feature-routes file. For a feature area with its own nested routes, create a dedicated `<feature>.routes.ts` exporting a `Routes` array, and reference it from the parent route with `loadChildren: () => import('./feature/feature.routes').then(m => m.FEATURE_ROUTES)`. Do not use `loadChildren` pointing at an `NgModule` for new work — that pattern is legacy from the pre-standalone router and shouldn't be introduced fresh.

If the feature needs its own guards, resolvers, or providers scoped only to it (a feature-specific state service that shouldn't be app-wide), attach them at the lazy route's boundary (`canActivate`, `resolve`, `providers` on the parent route object) rather than providing them in `root`, so they're created only when the feature is actually navigated to and destroyed when navigated away if scoped appropriately. Preserve any existing route data (`title`, `data`, breadcrumb metadata) used by the app shell when moving routes into the lazy config — don't drop it during the move.

Verify the split actually took effect: build the app for production (or run the dev server and inspect the network tab) and confirm a separate chunk is requested only on navigating to the feature, not on initial load. Run existing e2e/routing tests for this area and add a test (or navigation-based `RouterTestingHarness` check) confirming the route still resolves to the correct component/data after the change, since a lazy-loading refactor can silently break a route path or an aliased redirect without any compile-time error.
