# Starter Scaffold — React

> Template outline for bootstrapping a new React frontend, paired with an
> ASP.NET Core Web API backend (this platform's most common backend pairing).
> This is a folder-structure and setup guide, not a working demo — see
> `demos/` for a runnable example of the paired API side.

## Recommended Folder Structure

```text
<ProjectName>/
├── package.json
├── tsconfig.json                     # Omit only if the project is deliberately plain JS
├── vite.config.ts                    # Or CRA's react-scripts / next.config.js — pick one bundler
├── .env.development                  # Shape only — no real values committed
├── .env.production                   # Shape only — no real values committed
├── index.html                        # Vite entry point (CRA/Next.js differ — see their own conventions)
├── src/
│   ├── main.tsx                      # App bootstrap, providers (QueryClientProvider, router)
│   ├── App.tsx
│   ├── components/                   # Small, reusable, single-purpose components
│   │   └── <Feature>/
│   │       ├── <Feature>Card.tsx
│   │       └── <Feature>Card.test.tsx
│   ├── pages/                        # Or "routes/" per project convention — pick one
│   │   └── <Feature>/
│   │       └── <Feature>Page.tsx
│   ├── api/
│   │   ├── client.ts                 # Thin fetch/axios wrapper — base URL, auth header, error handling
│   │   └── <feature>Api.ts           # TanStack Query hooks calling the ASP.NET Core API
│   ├── context/
│   │   └── <Feature>Context.tsx      # Only for genuinely cross-cutting values (theme, current user)
│   ├── types/
│   │   └── <feature>.ts              # Shared TypeScript types/interfaces
│   └── test/
│       └── setup.ts                  # RTL/jest-dom setup
└── public/
```

## Key npm Packages

| Package | Purpose |
|---|---|
| `react`, `react-dom` | Core library |
| `typescript` (default assumption; omit only for a deliberately plain-JS project) | Static typing |
| `vite` (or the project's actual bundler — Create React App's `react-scripts`, or `next`) | Build tooling and dev server — check the project's real setup before assuming Vite |
| `react-router-dom` (if the app has more than one view) | Client-side routing |
| `@tanstack/react-query` | Server-state fetching/caching against the ASP.NET Core API — don't hand-roll fetch + `useState` + `useEffect` for this |
| `axios` (optional — `fetch` is also fine) | HTTP client, if the project prefers interceptor-based auth-header/error handling over a `fetch` wrapper |
| `@testing-library/react`, `@testing-library/user-event`, `@testing-library/jest-dom` | Component testing |
| `vitest` (with Vite) or `jest` (with CRA) | Test runner — match whichever the bundler already implies |
| `eslint`, `prettier`, `eslint-config-prettier` | Linting and formatting |

Don't add a global state library (Redux Toolkit, Zustand) unless plain hooks,
Context, and TanStack Query are genuinely insufficient — most CRUD-style
features don't need one.

## First Things to Configure

1. **API base URL.** Read it from an environment variable
   (`VITE_API_BASE_URL` / `REACT_APP_API_BASE_URL` / `NEXT_PUBLIC_API_BASE_URL`
   depending on bundler), never hardcoded, and never put a real secret behind
   one of these client-exposed prefixes — anything prefixed for client
   exposure is bundled into public JavaScript.
2. **CORS setup, coordinated with the backend team.** The React dev server
   and the ASP.NET Core API run on different origins by default
   (`localhost:5173` vs `localhost:5000`/`7000+`) — confirm the API's
   `AddCors`/`UseCors` policy allows the frontend's actual origin(s) before
   assuming a failing request is a frontend bug.
3. **Environment variable strategy.** Decide per-environment `.env` files
   (`.env.development`, `.env.production`) up front, document their shape
   without committing real values, and confirm which prefix the bundler
   requires for client exposure.
4. **Linting/formatting setup.** ESLint (with the React/hooks plugin,
   `eslint-plugin-react-hooks`, to catch incomplete `useEffect` dependency
   arrays) plus Prettier, wired into the project before the first
   non-trivial component is written — not retrofitted after the codebase has
   grown inconsistent.
5. **Auth token attachment.** Decide how the token issued by the API is
   stored and attached (an axios interceptor or a `fetch` wrapper adding the
   `Authorization` header) in one central place, matching whatever the
   backend's auth scheme expects, before scattering API calls across
   components.
6. Set up the paired test setup (Testing Library + Jest/Vitest) before
   writing the first non-trivial component (Test-First).
7. Never commit real values into `.env.*` files — document placeholder
   shapes only.
