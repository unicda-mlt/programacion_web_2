# LaVozEstudiantil — Project Analysis & Reusable Blueprint

## Stack & Architecture

| Layer | Technology |
|-------|-----------|
| Framework | Next.js 16 (App Router), React 19, TypeScript 5 |
| Styling | Tailwind CSS 4 — utility-only, no component library |
| API Client | `openapi-fetch` + auto-generated types via `openapi-typescript` |
| Auth | JWT stored in `httpOnly` cookies (never exposed to client JS) |
| Real-time | `@microsoft/signalr` WebSocket hub |
| Forms | Controlled local state; `react-hook-form` + `zod` for complex forms |
| Notifications | `sonner` (toast) |
| Icons | `lucide-react` |
| Dates | `date-fns` |
| React compiler | Enabled via `reactCompiler: true` in `next.config.ts` |

---

## Key Architectural Patterns

### Route Groups

| Group | Purpose |
|-------|---------|
| `(public)/` | Unauthenticated pages — home, scrutinies listing, login |
| `(dashboard)/` | Admin-only pages — layout-level server-side auth guard |

### Auth Flow

1. `POST /api/auth/login` → Next.js route handler calls backend, sets `httpOnly` cookie
2. Dashboard `layout.tsx` (Server Component) reads cookie, decodes JWT, checks role → redirects if invalid
3. Client components call `GET /api/auth/me` to obtain session via `useAuth` hook

### API Proxy

`/api/proxy/[...path]/route.ts` — catch-all that forwards all HTTP methods to the upstream backend, injecting the `Bearer` token from the cookie. The real backend URL never reaches the browser.

### Environment Validation

Zod schemas enforced at startup — throw at boot if any variable is missing or malformed. Split into `private.ts` (server-only) and `public.ts` (client-safe).

---

## Prompt to Build Similar Projects

```
Build a full-stack web application using Next.js 16 (App Router) and a separate REST API backend
(e.g. ASP.NET Core, FastAPI, or Express). The frontend must follow this architecture:

---

### Stack

- Framework: Next.js 16 with App Router and TypeScript 5
- Styling: Tailwind CSS 4 — utility-only, no component library
- API Client: openapi-fetch + openapi-typescript for a type-safe client generated from an OpenAPI spec
- Auth: JWT stored in httpOnly cookies (never exposed to client JS)
- Real-time (if needed): @microsoft/signalr WebSocket hub
- Forms: controlled React state for simple forms; react-hook-form + zod for complex forms
- Notifications: sonner toast
- Icons: lucide-react
- Dates: date-fns
- React compiler: enabled in next.config.ts via reactCompiler: true

---

### Project Structure

src/
  app/
    (public)/
      layout.tsx          # Server Component: reads cookie, renders nav with Login/Logout/Dashboard links
      page.tsx            # Home — SSR
      login/page.tsx      # Client Component login form
    (dashboard)/
      layout.tsx          # Server Component: reads auth-token cookie, decodes JWT, redirects if invalid
      DashboardClientLayout.tsx  # Client shell: Sidebar + DashboardHeader + Toaster
      dashboard/
        page.tsx          # SSR stats overview — parallel fetches with Promise.all
        [feature]/page.tsx  # One client page per resource (CRUD with DataTable + Modal)
    api/
      auth/
        login/route.ts    # POST — call backend auth, set httpOnly cookie
        logout/route.ts   # POST — clear cookie
        me/route.ts       # GET — decode cookie, return session JSON
        token/route.ts    # GET — return raw token (for SignalR auth)
      proxy/
        [...path]/route.ts  # Catch-all proxy: forwards all methods, injects Bearer token
  components/
    dashboard/
      layout/DashboardHeader.tsx
      sidebar/Sidebar.tsx          # Collapsible, mobile drawer, localStorage persistence
      sidebar/SidebarNavItem.tsx
      sidebar/SidebarGroup.tsx
      table/DataTable.tsx          # Generic paginated table with toolbar slot
    shared/
      Modal.tsx            # Overlay modal with title + close button
      Skeleton.tsx         # TableSkeleton and other loading states
      ErrorBoundary.tsx
  lib/
    api/
      client.ts            # openapi-fetch clients — browser uses /api/proxy, server uses API_BASE_URL
      errors.ts            # parseProblemDetails(body): ApiError
      schema.d.ts          # Auto-generated: openapi-typescript <swagger-url> -o src/lib/api/schema.d.ts
    auth/
      session.ts           # decodeJwt(token), isTokenExpired(session), getRole(session) — pure functions
    hooks/
      useAuth.ts           # Fetches /api/auth/me → { session, loading, refetch }
      useResource.ts       # Generic paginated CRUD hook
      useVoteStatus.ts     # SignalR hook (optional — only if real-time is needed)
  types/
    auth.ts                # Session interface, Role enum
    api.ts                 # PaginatedResponse<T>, ApiError
  utils/
    environment/
      private.ts           # Zod-validated server-only vars (API_BASE_URL)
      public.ts            # Zod-validated public vars (NEXT_PUBLIC_*)

---

### Auth Rules

1. Server-side guard in (dashboard)/layout.tsx:
   - Read auth-token cookie with cookies() from next/headers
   - Call decodeJwt(token) and isTokenExpired(session) — pure functions, no side effects
   - If missing, expired, or wrong role → redirect('/login')
   - Pass session down to DashboardClientLayout

2. Cookie hygiene on login:
   httpOnly: true
   secure: process.env.NODE_ENV === 'production'
   sameSite: 'strict'
   maxAge: 86400  // 24 hours

3. Delete cookie on logout by setting maxAge: 0.

4. Role-based redirect after login:
   After POST to /api/auth/login, fetch /api/auth/me and redirect:
   ADMIN → /dashboard, others → /scrutinies (or your equivalent default)

---

### API Proxy Pattern

app/api/proxy/[...path]/route.ts must:
- Read the auth-token cookie
- Build upstream URL: env.API_BASE_URL + '/' + params.path.join('/') + request.nextUrl.search
- Forward all safe headers — skip hop-by-hop: host, connection, keep-alive, transfer-encoding, upgrade, te, trailers
- Inject Authorization: Bearer <token> when cookie is present
- Pass body as arrayBuffer for non-GET/HEAD methods
- Use redirect: 'manual' — do not follow redirects
- Export GET, POST, PATCH, PUT, DELETE all delegating to the same proxyRequest() function

The client-side openapi-fetch base URL must be /api/proxy so the real backend never leaks to the browser.

---

### useResource<T> Hook

Signature:
  function useResource<T>({ path, pageSize? }: { path: string; pageSize?: number })
  returns: { data, total, page, setPage, loading, error, create, update, refetch }

Behavior:
- GET with { Page, PageSize } query params
- Handles both { data: T[], pagination: { records: number } } and { items: T[], total: number } response shapes
- Shows toast.success on create/update
- Throws parsed ApiError on failure — caller wraps in try/catch and shows toast.error
- Sets a friendly message on 403

---

### DataTable Component

Props:
  columns: Array<{ key: keyof T | string; label: string; render?: (row: T) => ReactNode }>
  data: T[]
  loading?: boolean
  error?: string | null
  page: number
  pageSize?: number   // default 10
  total: number
  onPageChange: (page: number) => void
  toolbar?: ReactNode  // slot for "New …" button and other actions
  emptyLabel?: string

Behavior:
- Shows TableSkeleton while loading
- Shows inline error banner on error
- Shows dashed empty state with emptyLabel text
- Pagination rendered only when totalPages > 1
- Columns with no render function fall back to String(row[key])

---

### Environment Variables

Validate at module load time with Zod. Throw with a descriptive error if validation fails.

Server-only (src/utils/environment/private.ts):
  API_BASE_URL=https://your-backend-host    # Direct backend — never sent to browser

Public (src/utils/environment/public.ts):
  NEXT_PUBLIC_API_BASE_URL=/api/proxy       # Proxied URL for client fetch
  NEXT_PUBLIC_APP_URL=http://localhost:3000

---

### Generate API Types

Add to package.json scripts:
  "generate:api": "openapi-typescript <swagger-url> --insecure -o src/lib/api/schema.d.ts"

Run after every backend schema change. --insecure allows local HTTPS with self-signed certs.

---

### Good Practices

- Never expose the backend URL or auth token to client-side JS — always use the proxy pattern
- JWT decoded client-side only for display; authorization enforced server-side in layouts
- Auth cookie is httpOnly — prevents XSS token theft
- Proxy strips hop-by-hop headers before forwarding to the upstream
- useResource is the single abstraction for paginated CRUD — do not duplicate fetch logic per page
- Each admin CRUD page follows the same pattern: useResource + DataTable + Modal for create/edit
- SignalR lifecycle (connect, subscribe, reconnect, cleanup) fully encapsulated in one hook
- Sidebar collapse state persisted to localStorage
- Public layout reads session server-side — no client flicker on Login/Logout links
- All environment variables validated at startup with Zod — fail fast, fail loudly
```
