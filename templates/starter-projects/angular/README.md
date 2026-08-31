# Starter Scaffold — Angular

> Template outline for bootstrapping a new Angular application (paired with an ASP.NET Core
> Web API backend — the platform's default pairing). This is a folder-structure and setup
> guide, not a working demo — see `demos/` for a runnable example.

## Recommended Folder Structure

```text
<project-name>/
├── angular.json                       # CLI project config — build/test/serve targets
├── package.json
├── tsconfig.json / tsconfig.app.json / tsconfig.spec.json
├── src/
│   ├── main.ts                        # bootstrapApplication() for a standalone app
│   ├── index.html
│   ├── styles.css                     # Or .scss per project convention
│   ├── environments/
│   │   ├── environment.ts             # Dev API base URL — no secrets, ever
│   │   └── environment.prod.ts
│   └── app/
│       ├── app.config.ts              # providers: router, HttpClient + interceptors
│       ├── app.routes.ts              # Route definitions, lazy-loaded feature routes
│       ├── app.component.ts           # Root standalone component
│       ├── core/                      # App-wide singletons — one-time setup
│       │   ├── interceptors/
│       │   │   ├── auth.interceptor.ts       # Attaches the auth token
│       │   │   └── error.interceptor.ts      # Centralized HTTP error handling
│       │   └── services/
│       │       └── auth.service.ts           # providedIn: 'root'
│       ├── shared/                    # Reusable, feature-agnostic components/pipes/directives
│       │   └── components/
│       │       └── <shared-widget>/
│       │           ├── <shared-widget>.component.ts
│       │           ├── <shared-widget>.component.html
│       │           └── <shared-widget>.component.spec.ts
│       └── features/
│           └── <feature>/
│               ├── <feature>.routes.ts        # Feature's own lazy-loaded route config
│               ├── services/
│               │   └── <feature>.service.ts   # API calls + typed response models live here
│               ├── models/
│               │   └── <feature>.model.ts     # Interfaces matching the API's DTO shapes
│               └── components/
│                   └── <feature>-list/
│                       ├── <feature>-list.component.ts
│                       ├── <feature>-list.component.html
│                       └── <feature>-list.component.spec.ts
```

Organize by feature module/folder (`app/features/<feature>/`) rather than by type
(`components/`, `services/`, `models/` as flat top-level folders) once the app grows past a
handful of components — this keeps a feature's component, service, and model together and
scales better than a type-first layout. Standalone components make feature folders even more
natural since there's no `NgModule` file forcing a different grouping.

## Key npm Packages

| Package | Purpose |
|---|---|
| `@angular/core`, `@angular/common`, `@angular/platform-browser` | Framework core |
| `@angular/router` | Routing, including lazy-loaded feature routes |
| `@angular/forms` | Reactive Forms (`ReactiveFormsModule`) and Template-driven Forms |
| `@angular/common/http` | `HttpClient`, interceptors, `HttpClientTestingModule` |
| `rxjs` | Observables backing `HttpClient`, forms, and any RxJS-based state |
| `@angular/cli` (dev) | Project scaffolding, build, serve, test |
| `typescript` (dev) | Language/compiler — keep the version aligned with the Angular major version |
| `jasmine-core`, `karma`, `karma-chrome-launcher`, `karma-jasmine`, `karma-jasmine-html-reporter` (dev) | Angular's traditional default test stack — or `jest`/`jest-preset-angular` (dev) if the project has migrated |
| `eslint`, `angular-eslint` (dev) | Linting — Angular's CLI-integrated default since the deprecation of `tslint` |
| `prettier` (dev, optional) | Formatting, if the project wants it layered on top of ESLint |

Don't add a state-management library (NgRx, Akita, Elf) unless component-local signals/RxJS
and a couple of well-known app-wide services are genuinely insufficient — most CRUD-style
features don't need one, the same restraint this platform applies to Blazor's Fluxor guidance.

## First Things to Configure

1. Set the API base URL per environment in `src/environments/environment.ts` /
   `environment.prod.ts` — never a secret or credential, since these files are bundled into
   the public client-side JavaScript at build time; a public base URL or feature flag only.
2. Register an `HttpInterceptor` (`HTTP_INTERCEPTORS` provider, or the functional
   `HttpInterceptorFn` wired via `withInterceptors()` in `app.config.ts`) for attaching the
   auth token to outgoing requests and for centralized HTTP error handling, before writing the
   first feature service that calls the API.
3. Coordinate CORS with the backend team up front — `AddCors`/`UseCors` on the ASP.NET Core
   Web API must allow the Angular dev server's origin (and the deployed origin later); this is
   a backend-configuration fix, not something resolvable from the Angular side.
4. Decide standalone components (default for a new Angular 17+ project) vs. NgModules before
   the first feature is built, and apply it consistently — don't let the app end up half
   standalone, half NgModule-based without a deliberate reason.
5. Decide signals vs. RxJS-backed services for component-local and cross-component state up
   front, for the same reason — introducing both patterns for the same kind of state makes the
   codebase harder to reason about than committing to one.
6. Set up ESLint (`ng add @angular-eslint/schematics` for a CLI-based project) — or confirm the
   project's existing lint configuration — before the first non-trivial component, along with
   the paired test runner (Jasmine + Karma via `ng test`, or Jest if already migrated).
7. Never commit real values into `environment.prod.ts` beyond a public base URL — document
   placeholder shapes only, matching the platform's `appsettings.json` policy on the backend.
