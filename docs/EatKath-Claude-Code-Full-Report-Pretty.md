# 🍽️ EatKath
## Full Implementation & Architecture Report

> **Source:** Complete Claude Code exploration of the EatKath repository  
> **Purpose:** Long-term technical reference for the EatKath project  
> **Status:** Read-only exploration report

---

## 📑 Contents

- [1. `package.json` — Stack & Versions](#1-packagejson-stack-versions)
- [2. Folder Structure under `src`](#2-folder-structure-under-src)
- [3. Routing](#3-routing)
- [4. Pages (`src/pages`)](#4-pages-srcpages)
- [5. Components](#5-components)
- [6. Hooks](#6-hooks)
- [7. API / Services Layer](#7-api-services-layer)
- [8. State Management](#8-state-management)
- [9. Authentication Handling](#9-authentication-handling)
- [10. Forms / Validation](#10-forms-validation)
- [11. Error / Loading State Handling Patterns](#11-error-loading-state-handling-patterns)
- [12. Types (`src/types`)](#12-types-srctypes)
- [Part A — Authentication / Authorization](#part-a-authentication-authorization)
- [Part B — Tests (`EatKath.API.Tests`)](#part-b-tests-eatkathapitests)
- [1. `ApplicationDbContext.cs` — DbSets and OnModelCreating](#1-applicationdbcontextcs-dbsets-and-onmodelcreating)
- [2. `Data/Seeders` folder](#2-dataseeders-folder)
- [3. `Entities` folder — every class](#3-entities-folder-every-class)
- [4. `Enums` folder](#4-enums-folder)
- [5. Migrations — chronological schema evolution](#5-migrations-chronological-schema-evolution)
- [6. Database provider](#6-database-provider)
- [7. Full relationship map](#7-full-relationship-map)
- [8. Inconsistencies / orphaned or notable items](#8-inconsistencies-orphaned-or-notable-items)
- [1. EatKath.API.csproj](#1-eatkathapicsproj)
- [2. Program.cs (`EatKath.API/Program.cs`)](#2-programcs-eatkathapiprogramcs)
- [3. Controllers (`EatKath.API/Controllers`) — all 16 read](#3-controllers-eatkathapicontrollers-all-16-read)
- [4. Interfaces / Services](#4-interfaces-services)
- [5. DTOs (`EatKath.API/DTOs`)](#5-dtos-eatkathapidtos)
- [6. Mappings (`EatKath.API/Mappings`)](#6-mappings-eatkathapimappings)
- [7. Validators (`EatKath.API/Validators`) and gaps](#7-validators-eatkathapivalidators-and-gaps)
- [8. Exception handling](#8-exception-handling)
- [9. Configurations / Extensions / Helpers](#9-configurations-extensions-helpers)
- [10. appsettings.json / appsettings.Development.json](#10-appsettingsjson-appsettingsdevelopmentjson)
- [11. Repositories folder](#11-repositories-folder)
- [1. Overall Architecture](#1-overall-architecture)
- [2. Frontend Architecture (`EatKath.Web`)](#2-frontend-architecture-eatkathweb)
- [3. Backend Architecture (`EatKath.API`)](#3-backend-architecture-eatkathapi)
- [4. Database / EF Core](#4-database-ef-core)
- [5. Authentication and Authorization](#5-authentication-and-authorization)
- [6. Business Domain](#6-business-domain)
- [7. API (grouped by domain)](#7-api-grouped-by-domain)
- [8. Tests (`EatKath.API.Tests`)](#8-tests-eatkathapitests)
- [9. Docker / Infrastructure](#9-docker-infrastructure)
- [10. Project Maturity](#10-project-maturity)
- [11. Architecture Concerns](#11-architecture-concerns)
- [12. EatKath Architecture Map](#12-eatkath-architecture-map)

---

# EatKath.Web Frontend — Implementation Report

## 1. `package.json` — Stack & Versions

File: `EatKath.Web/package.json`

| CategoryLibraryVersion       |                                                                                                                                                                                |                                                 |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------- |
| Framework                    | `react` / `react-dom`                                                                                                                                                          | `^19.2.7`                                       |
| Language                     | `typescript`                                                                                                                                                                   | `~6.0.2`                                        |
| Build tool                   | `vite`                                                                                                                                                                         | `^8.1.1` (via `@vitejs/plugin-react` `^6.0.3`)  |
| Routing                      | `react-router-dom`                                                                                                                                                             | `^7.18.2`                                       |
| UI library                   | `@mui/material` `^7.3.11`, `@mui/icons-material` `^7.3.11`, `@emotion/react`/`@emotion/styled` (MUI's styling engine)                                                          |                                                 |
| HTTP client                  | `axios`                                                                                                                                                                        | `^1.19.0`                                       |
| Forms                        | `react-hook-form`                                                                                                                                                              | `^7.84.0` (plus `@hookform/resolvers` `^5.7.1`) |
| Validation                   | `zod` `^4.4.3` — **present as a dependency but not actually used anywhere in ****`src`** (no `zod.object`, no `zodResolver` calls found; `@hookform/resolvers` is also unused) |                                                 |
| Toast notifications          | `notistack` `^3.0.2` — **present as a dependency but not used anywhere in ****`src`** (no `SnackbarProvider`, `useSnackbar`, or `enqueueSnackbar` found)                       |                                                 |
| Server state / data fetching | `@tanstack/react-query` `^5.101.4` — **present as a dependency but NOT wired up** (no `QueryClientProvider`, `useQuery`, or `useMutation` anywhere in `src`)                   |                                                 |
| Linting                      | ESLint 10 + `typescript-eslint` 8, `eslint-plugin-react-hooks`, `eslint-plugin-react-refresh`                                                                                  |                                                 |

> **💡 Important finding:** Several dependencies (`zod`, `notistack`, `@tanstack/react-query`, `@hookform/resolvers`) are installed but currently dead weight — the actual code doesn't use them yet. This strongly suggests the app is mid-migration/scaffolded for these patterns but hasn't adopted them yet. Data fetching is done with plain `useEffect` + `useState` + `try/catch`, not React Query.

`tsconfig.app.json` targets `es2023`, uses `verbatimModuleSyntax`, `noUnusedLocals`/`noUnusedParameters`, bundler module resolution — a strict, modern TS setup.

## 2. Folder Structure under `src`

```
src/
  api/            axios.ts                          — single shared axios instance + auth interceptor
  assets/         hero.png, react.svg
  components/
    deals/        DealCard.tsx, DealFormDialog.tsx (EMPTY FILE, 0 lines — unused/placeholder)
    reservations/ ReservationDialog.tsx
    restaurants/  RestaurantCard.tsx
  features/
    auth/         AuthContext.tsx, AuthProvider.tsx, LoginPage.tsx, ProtectedRoute.tsx, types.ts
  layouts/        MainLayout.tsx
  pages/           (14 page components — see section 4)
  routes/         AppRoutes.tsx
  services/       (14 service classes — see section 7)
  theme/          colors.ts, theme.ts
  types/          (23 type files, mirroring backend DTOs)
  utils/          imageUrl.ts
  App.tsx, main.tsx

```

Notably **absent**: no `src/hooks` folder (no custom hooks exist anywhere in the project), no `src/context` folder outside of `features/auth`, and the general `api` folder contains only the axios client — all actual endpoint calls live in `src/services`, not `src/api`.

## 3. Routing

File: `src/routes/AppRoutes.tsx`, entry: `src/App.tsx` (`App` just renders `<AppRoutes />`).

- Uses `react-router-dom` v7 with classic `BrowserRouter` + `<Routes>/<Route>` (not the new data-router / `createBrowserRouter` API).
- A layout route wraps most pages: `<Route element={<MainLayout />}>` containing nested `<Route>`s that render inside `MainLayout`'s `<Outlet />`.
- **Bug/inconsistency found**: `/owner/menu-items` and `/owner/menu-categories` routes (lines 138–155) are declared as **siblings of the layout route**, not nested inside it — so those two pages render without the `MainLayout` (no navbar).
- Role-based protection is done via a custom `ProtectedRoute` component (`src/features/auth/ProtectedRoute.tsx`), not a router-level loader/guard.

Full route map:

| PathPageProtection       |                           |                                   |
| ------------------------ | ------------------------- | --------------------------------- |
| `/`                      | `HomePage`                | public                            |
| `/restaurants`           | `RestaurantsPage`         | public                            |
| `/restaurants/:id`       | `RestaurantDetailsPage`   | public                            |
| `/favorites`             | `MyFavoritesPage`         | role="Customer"                   |
| `/my-reservations`       | `MyReservationsPage`      | role="Customer"                   |
| `/login`                 | `LoginPage`               | public                            |
| `/owner`                 | `OwnerDashboardPage`      | role="Owner"                      |
| `/owner/deals`           | `OwnerDealsPage`          | role="Owner"                      |
| `/owner/deals/new`       | `CreateDealPage`          | role="Owner"                      |
| `/owner/deals/edit/:id`  | `EditDealPage`            | role="Owner"                      |
| `/owner/restaurant`      | `OwnerRestaurantPage`     | role="Owner"                      |
| `/owner/opening-hours`   | `OwnerOpeningHoursPage`   | role="Owner"                      |
| `/admin`                 | `AdminDashboardPage`      | role="Admin"                      |
| `/owner/menu-items`      | `OwnerMenuItemsPage`      | role="Owner" (outside MainLayout) |
| `/owner/menu-categories` | `OwnerMenuCategoriesPage` | role="Owner" (outside MainLayout) |

`ProtectedRoute` logic (`src/features/auth/ProtectedRoute.tsx`):

```
if (!auth.user) return <Navigate to="/login" replace />;
if (role && auth.user.role !== role) return <Navigate to="/" replace />;
return <>{children}</>;

```

Simple exact-role match (not "has any of roles" — a single string comparison), redirecting unauthenticated users to `/login` and wrong-role users to `/`.

## 4. Pages (`src/pages`)

- **HomePage.tsx** — landing page.
- **RestaurantsPage.tsx** — full restaurant listing with client-side search/filter by area, cuisine, dining type; loads restaurants + favorites + filter lookup lists via 5 separate service calls in a single `loadData()` function; renders `RestaurantCard` grid; shows `CircularProgress` while `loading`.
- **RestaurantDetailsPage.tsx** — single restaurant detail (likely renders `DealCard`s, not fully inspected line-by-line but present).
- **MyFavoritesPage.tsx** — customer's favorited restaurants; notably hardcodes `https://localhost:7203${f.logoUrl}` directly rather than using the `getImageUrl` utility (inconsistent with `RestaurantCard`).
- **MyReservationsPage.tsx** — customer's own reservations.
- **OwnerDashboardPage.tsx** — owner's restaurant summary + reservations table with actions (Redeem/No-Show/Cancel), a redemption dialog capturing bill amount, calls `RedemptionService.complete()`. Handles 404 (no restaurant assigned) as a distinct empty state.
- **OwnerDealsPage.tsx** — list/manage owner's deals.
- **CreateDealPage.tsx** — plain controlled-form (not React Hook Form) building a `DealForm`; posts via `OwnerDealService.create()`; uses `alert()` for both success and error messages; normalizes `HH:MM` → `HH:MM:00` before submit.
- **EditDealPage.tsx** — same pattern as CreateDealPage for updates.
- **OwnerRestaurantPage.tsx** — edit owner's restaurant profile.
- **OwnerOpeningHoursPage.tsx** — manage opening hours.
- **OwnerMenuCategoriesPage.tsx** / **OwnerMenuItemsPage.tsx** — CRUD for menu categories/items (rendered outside MainLayout, per routing bug above).
- **OwnerReservationsPage.tsx** — owner reservation management (separate from dashboard's embedded table).
- **AdminDashboardPage.tsx** — currently a **stub**: just renders `<Typography variant="h4">Admin Dashboard</Typography>`, no real functionality yet.

## 5. Components

- `src/components/restaurants/RestaurantCard.tsx` — reusable restaurant tile (image, name, area, description, deal chips, favorite toggle via `UserFavoriteService`, "View Details" link). Used by `RestaurantsPage`.
- `src/components/deals/DealCard.tsx` — deal tile with a "Reserve" button that opens `ReservationDialog`.
- `src/components/deals/DealFormDialog.tsx` — **empty file (0 bytes)**, unused/scaffolded but never implemented; deal forms are instead hand-rolled directly inside `CreateDealPage`/`EditDealPage`.
- `src/components/reservations/ReservationDialog.tsx` — MUI `Dialog` form for creating a reservation against a deal; plain controlled inputs, `alert()`-based error/success feedback, handles HTTP 400 specially (`alert(error.response.data)`).

Organization is feature-grouped under `components/<feature>/`, mirrored by `pages/` (page-level) vs `components/` (reusable pieces) vs `features/auth/` (a self-contained auth module with its own context/provider/route-guard/page).

## 6. Hooks

**None.** There is no `src/hooks` directory and no custom hooks (`useXxx.ts`) anywhere in the codebase. All state/data-fetching is done inline in each page component with `useState`/`useEffect`. The only hook usage is React's built-ins and library hooks (`useForm`, `useContext`, `useNavigate`, `useState`, `useEffect`).

## 7. API / Services Layer

**`src/api/axios.ts`** — single shared axios instance with a request interceptor that injects the bearer token from `localStorage`:

```
const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
    headers: { "Content-Type": "application/json" }
});

api.interceptors.request.use((config) => {
    const storedUser = localStorage.getItem("user");
    if (storedUser) {
        const user = JSON.parse(storedUser);
        if (user.token) {
            config.headers.Authorization = `Bearer ${user.token}`;
        }
    }
    return config;
});
export default api;

```

- Only a **request** interceptor exists — there is **no response interceptor** (no automatic 401 handling/logout/refresh logic).

**Base URL configuration** (Vite env files, `VITE_` prefix required):

- `.env.development`: `VITE_API_URL=https://localhost:7203/api`
- `.env.production`: `VITE_API_URL=https://eatkath-api-eae5b9gyc6gehtby.australiaeast-01.azurewebsites.net/api`

**`src/services/`** — one class per resource, each instantiated as a singleton (`export default new XService()`), all built on the shared `api` axios instance. 14 services: `AuthService`, `AreaService`, `CuisineService`, `DealService`, `DiningTypeService`, `MenuCategoryService`, `MenuItemService`, `OwnerDealService`, `OwnerReservationService`, `OwnerRestaurantService`, `RedemptionService`, `ReservationService`, `RestaurantOpeningHourService`, `RestaurantService`, `UserFavoriteService`.

Example pattern (`src/services/OwnerDealService.ts`):

```
class OwnerDealService {
    async getMyDeals(): Promise<Deal[]> {
        const response = await api.get<Deal[]>("/deal/my");
        return response.data;
    }
    async create(deal: CreateDeal): Promise<Deal> {
        const response = await api.post<Deal>("/deal", deal);
        return response.data;
    }
    // update, delete, getById ...
}
export default new OwnerDealService();

```

Every method is strongly typed against a matching `src/types/*.ts` DTO (`Create*`, `Update*`, base entity).

## 8. State Management

- **No Redux, Zustand, or global store.**
- **React Context API** is used exactly once: authentication (`src/features/auth/AuthContext.tsx` + `AuthProvider.tsx`).
- **No React Query usage** despite `@tanstack/react-query` being a dependency — confirmed via repo-wide grep for `useQuery|useMutation|QueryClient`, zero matches, and `main.tsx` has no `QueryClientProvider`.
- Server data ("state") is fetched ad hoc per page via `useEffect` + local `useState`, with manual `loading` flags and manual refetch functions (e.g., `loadData()`, `loadFavorites()`) passed down as callbacks (e.g., `onFavoriteChanged={loadData}` in `RestaurantsPage`).

## 9. Authentication Handling

- **Token storage**: JWT + user info stored together as JSON in `localStorage` under the key `"user"` (see `AuthProvider.login()`).
- **Auth Context** (`src/features/auth/AuthContext.tsx`): exposes `{ user: AuthResponse | null; login(user); logout() }`.
- **AuthProvider** (`src/features/auth/AuthProvider.tsx`): initializes state by reading `localStorage.getItem("user")` on mount; `login()` writes to localStorage + sets state; `logout()` removes the key + clears state. Wraps the whole app in `main.tsx`.
- **Token attachment**: done centrally by the axios request interceptor in `src/api/axios.ts` (reads `localStorage`, sets `Authorization: Bearer <token>` header) — not via the Auth Context directly.
- **Login page**: `src/features/auth/LoginPage.tsx` — uses `react-hook-form`'s `useForm`/`register`/`handleSubmit` (no validation resolver/schema attached — no `required`, no zod), calls `AuthService.login()`, then `auth.login(response)` and `navigate("/")`. Errors are only `console.error`'d, no UI feedback shown to the user on failed login.
- **Protected route wrapper**: `src/features/auth/ProtectedRoute.tsx` (see section 3) — a component-based guard checked at render time via `useContext(AuthContext)`.
- **Role source of truth**: `AuthResponse.role` (string: `"Customer" | "Owner" | "Admin"`, per usage), from `src/features/auth/types.ts`, which documents it maps to backend `AuthResponseDto`.

`AuthResponse` type (`src/features/auth/types.ts`):

```
export interface AuthResponse {
    userId: number;
    firstName: string;
    lastName: string;
    email: string;
    role: string;
    token: string;
    expiresAt: string;
}

```

## 10. Forms / Validation

- **Form library**: `react-hook-form` is a dependency and is used in exactly **one place** — `LoginPage.tsx` (`useForm`, `register`, `handleSubmit`), with **no validation rules** applied (fields aren't marked `required`, no schema resolver wired).
- **Validation library**: `zod` and `@hookform/resolvers` are dependencies but are **not used anywhere** in the codebase (no `zodResolver`, no `z.object` schemas found).
- All other forms in the app (`CreateDealPage`, `EditDealPage`, `ReservationDialog`, likely `OwnerRestaurantPage`/`OwnerOpeningHoursPage`) are **plain controlled MUI ****`TextField`****s** driven by raw `useState` objects with manual `onChange` spread updates — no form library, no client-side validation at all beyond native `type="number"`/`type="date"` input constraints.

## 11. Error / Loading State Handling Patterns

There is **no toast library wired up** (`notistack` unused) and **no error boundary** found anywhere in `src`. The consistent pattern across pages is:

**Loading**: a boolean `loading` state gates a `CircularProgress` before the page content renders, e.g. (`RestaurantsPage.tsx`):

```
if (loading) return <CircularProgress />;

```

**Errors**: `try/catch/finally` around service calls, with `console.error(error)` for logging and either silent failure or a browser `alert()` for user-facing feedback. Example (`src/components/reservations/ReservationDialog.tsx`):

```
catch (error: any) {
    console.error(error);
    if (error.response?.status === 400) {
        alert(error.response.data);
    } else {
        alert("Unable to create reservation. Please try again.");
    }
}

```

And in `CreateDealPage.tsx`, on failure it dumps the raw API error body via `alert(JSON.stringify(error.response.data, null, 2))` — a developer-facing pattern rather than a polished UX.

**Special-case handling**: `OwnerDashboardPage.tsx` treats a `404` on "get my restaurant" as a legitimate empty state (owner has no restaurant assigned yet) rather than an error, rendering a dedicated "No Restaurant Assigned" panel instead of an error message.

No global error boundary, no interceptor-level error normalization, no snackbar/toast — every page/component reimplements its own `alert()`/`console.error` handling independently.

## 12. Types (`src/types`)

23 files, largely one-to-one with backend DTOs, several explicitly documented as mirroring ASP.NET Core DTOs (comment headers like `// Matches: EatKath.API.DTOs.Restaurant.RestaurantDto`). Pattern: base entity type (`Restaurant.ts`, `Deal.ts`, `Reservation.ts`, `MenuItem.ts`, `MenuCategory.ts`, `RestaurantOpeningHour.ts`, `Redemption.ts`, `OwnerReservation.ts`, `UserFavorite.ts`, `User.ts`, `Area.ts`, `Cuisine.ts`, `DiningType.ts`) plus separate `Create*`/`Update*` request-shape variants (`CreateDeal.ts`, `UpdateDeal.ts`, `CreateMenuCategory.ts`, `UpdateMenuCategory.ts`, `CreateMenuItem.ts`, `UpdateMenuItem.ts`, `CreateRestaurantOpeningHour.ts`, `UpdateRestaurantOpeningHour.ts`, `UpdateRestaurant.ts`, `CompleteRedemption.ts`), plus a form-only shape `DealForm.ts` used by `CreateDealPage`/`EditDealPage`.

Example (`src/types/Restaurant.ts`):

```
export interface Restaurant {
    id: number;
    name: string;
    description: string;
    address: string;
    phoneNumber: string;
    email: string;
    website: string;
    logoUrl: string;
    isActive: boolean;
    areaId: number;
    areaName: string;
    bestDiscount: number | null;
    activeDeals: number;
    isFavorite: boolean;
    cuisines: string[];
    diningTypes: string[];
}

```

Auth-specific types live separately in `src/features/auth/types.ts` (`LoginRequest`, `AuthResponse`) rather than in `src/types`.

---

### Summary of Notable Gaps / Inconsistencies Found in Current Code

1. `@tanstack/react-query`, `zod`, `@hookform/resolvers`, and `notistack` are installed dependencies but **not actually integrated** — data fetching is manual `useEffect`, forms are unvalidated, and there's no toast system in use.
2. `src/components/deals/DealFormDialog.tsx` is a **0-byte empty file**.
3. `/owner/menu-items` and `/owner/menu-categories` routes are declared outside the `MainLayout` wrapper in `AppRoutes.tsx`, so they render without the navbar (likely unintentional).
4. `MyFavoritesPage.tsx` hardcodes `https://localhost:7203` for images instead of using the `getImageUrl` utility used elsewhere (`RestaurantCard.tsx`).
5. No `src/hooks` folder exists — zero custom hooks anywhere.
6. Axios client has a request interceptor for auth but no response interceptor (no centralized 401/refresh handling).
7. `AdminDashboardPage.tsx` is a placeholder with no real functionality.

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API.Tests\Helpers\TestDbContextFactory.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API\Services\ReservationService.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API\Data\Seeders\RestaurantDiningTypeSeeder.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

**Bash **List frontend source directory tree

IN

```
pwd; find EatKath.Web/src -maxdepth 2 | sort
```

OUT

```
/c/Users/snjja/source/localrepos/EatKath/EatKath.Web
find: 'EatKath.Web/src': No such file or directory
```

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API.Tests\Services\RedemptionServiceTests.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

**Bash **List frontend source directory tree from EatKath.Web

IN

```
find src -maxdepth 2 | sort
```

OUT

```
src
src/App.tsx
src/api
src/api/axios.ts
src/assets
src/assets/hero.png
src/assets/react.svg
src/components
src/components/deals
src/components/reservations
src/components/restaurants
src/features
src/features/auth
src/hooks
src/layouts
src/layouts/MainLayout.tsx
src/main.tsx
src/pages
src/pages/AdminDashboardPage.tsx
src/pages/CreateDealPage.tsx
src/pages/EditDealPage.tsx
src/pages/HomePage.tsx
src/pages/MyFavoritesPage.tsx
src/pages/MyReservationsPage.tsx
src/pages/OwnerDashboardPage.tsx
src/pages/OwnerDealsPage.tsx
src/pages/OwnerMenuCategoriesPage.tsx
src/pages/OwnerMenuItemsPage.tsx
src/pages/OwnerOpeningHoursPage.tsx
src/pages/OwnerReservationsPage.tsx
src/pages/OwnerRestaurantPage.tsx
src/pages/RestaurantDetailsPage.tsx
src/pages/RestaurantsPage.tsx
src/routes
src/routes/AppRoutes.tsx
src/services
src/services/AreaService.ts
src/services/AuthService.ts
src/services/CuisineService.ts
src/services/DealService.ts
src/services/DiningTypeService.ts
src/services/MenuCategoryService.ts
src/services/MenuItemService.ts
src/services/OwnerDealService.ts
src/services/OwnerReservationService.ts
src/services/OwnerRestaurantService.ts
src/services/RedemptionService.ts
src/services/ReservationService.ts
src/services/RestaurantOpeningHourService.ts
src/services/RestaurantService.ts
src/services/UserFavoriteService.ts
src/theme
src/theme/colors.ts
src/theme/theme.ts
src/types
src/types/Area.ts
src/types/CompleteRedemption.ts
src/types/CreateDeal.ts
src/types/CreateMenuCategory.ts
src/types/CreateMenuItem.ts
src/types/CreateRestaurantOpeningHour.ts
src/types/Cuisine.ts
src/types/Deal.ts
src/types/DealForm.ts
src/types/DiningType.ts
src/types/MenuCategory.ts
src/types/MenuItem.ts
src/types/OwnerReservation.ts
src/types/Redemption.ts
src/types/Reservation.ts
src/types/Restaurant.ts
src/types/RestaurantOpeningHour.ts
src/types/RestaurantService.ts
src/types/UpdateDeal.ts
src/types/UpdateMenuCategory.ts
src/types/UpdateMenuItem.ts
src/types/UpdateRestaurant.ts
src/types/UpdateRestaurantOpeningHour.ts
src/types/User.ts
src/types/UserFavorite.ts
src/utils
src/utils/imageUrl.ts
```

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API\Migrations\20260725091007\_InitialCreate.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

This gives me a good top-level map while the frontend agent digs into content. I'll wait for the background agents to complete before compiling the full report.

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API\Migrations\20260804014114\_AddReservationLimit.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

**Bash **Check uploads folder and seeders

IN

```
cd /c/Users/snjja/source/localrepos/EatKath && find EatKath.API/wwwroot -maxdepth 2 | head -30; echo "---"; find EatKath.API/Data/Seeders -type f; echo "---uploads count---"; find EatKath.API/wwwroot/uploads -type f | wc -l
```

OUT

```
EatKath.API/wwwroot
EatKath.API/wwwroot/uploads
EatKath.API/wwwroot/uploads/menuitems
EatKath.API/wwwroot/uploads/restaurants
---
EatKath.API/Data/Seeders/AreaSeeder.cs
EatKath.API/Data/Seeders/CuisineSeeder.cs
EatKath.API/Data/Seeders/DatabaseSeeder.cs
EatKath.API/Data/Seeders/DealSeeder.cs
EatKath.API/Data/Seeders/DiningTypeSeeder.cs
EatKath.API/Data/Seeders/MenuCategorySeeder.cs
EatKath.API/Data/Seeders/MenuItemSeeder.cs
EatKath.API/Data/Seeders/MenuSeeder.cs
EatKath.API/Data/Seeders/RedemptionSeeder.cs
EatKath.API/Data/Seeders/RestaurantCuisineSeeder.cs
EatKath.API/Data/Seeders/RestaurantDiningTypeSeeder.cs
EatKath.API/Data/Seeders/RestaurantImageSeeder.cs
EatKath.API/Data/Seeders/RestaurantOpeningHourSeeder.cs
EatKath.API/Data/Seeders/RestaurantSeeder.cs
EatKath.API/Data/Seeders/RoleSeeder.cs
EatKath.API/Data/Seeders/UserFavoriteSeeder.cs
EatKath.API/Data/Seeders/UserSeeder.cs
---uploads count---
5
```

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API\Interfaces\IRestaurantService.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

# Auth & Tests Deep-Dive Report

## Part A — Authentication / Authorization

### A1. `EatKath.API/Controllers/AuthController.cs`

Only **two** endpoints exist — no refresh-token, logout, or password-reset endpoints:

| VerbRouteAttributeRequest DTOResponse |                     |                    |                                                        |                   |
| ------------------------------------- | ------------------- | ------------------ | ------------------------------------------------------ | ----------------- |
| POST                                  | `api/Auth/register` | `[AllowAnonymous]` | `RegisterDto` (`EatKath.API/DTOs/Auth/RegisterDto.cs`) | `AuthResponseDto` |
| POST                                  | `api/Auth/login`    | `[AllowAnonymous]` | `LoginDto` (`EatKath.API/DTOs/Auth/LoginDto.cs`)       | `AuthResponseDto` |

DTOs (`EatKath.API/DTOs/Auth/`):

- `RegisterDto`: FirstName, LastName, Email ([Required][EmailAddress]), Password ([Required][MinLength(8)]), optional PhoneNumber.
- `LoginDto`: Email, Password (both `[Required]`).
- `AuthResponseDto`: UserId, FirstName, LastName, Email, Role (string), Token, ExpiresAt.

There is no `RefreshTokenDto` anywhere in the DTOs/Auth folder — confirms no refresh-token flow exists.

### A2. `EatKath.API/Services/AuthService.cs` + `EatKath.API/Interfaces/IAuthService.cs` (interface actually lives in `EatKath.API.Services.Interfaces` namespace, in `Interfaces` folder despite the folder name mismatch)

`IAuthService` only exposes `RegisterAsync` and `LoginAsync` — no refresh-token method.

**Password hashing**: ASP.NET Identity's `PasswordHasher<Entities.User>` (`Microsoft.AspNetCore.Identity`), instantiated directly (not via DI) in the constructor. `HashPassword`/`VerifyPassword` are thin wrappers around it.

**JWT generation** (`GenerateJwtToken`, `AuthService.cs:112-151`):

```
var jwtSettings = _configuration.GetSection("JwtSettings");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"]));

var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Email, user.Email),
    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
    new Claim(ClaimTypes.Role, user.Role.Name)
};

var token = new JwtSecurityToken(
    issuer: jwtSettings["Issuer"],
    audience: jwtSettings["Audience"],
    claims: claims,
    expires: expires,
    signingCredentials: credentials);

```

- Signing key, issuer, audience, and expiry-in-minutes all come from configuration section `"JwtSettings"` (appsettings — value not read here, only the key names).
- Symmetric HMAC-SHA256 signing (no asymmetric/RSA key).
- Claims: `NameIdentifier` (user id), `email` (JWT registered name), `Name` (full name), `Role` (single role name string, e.g. "Admin"/"Owner"/"Customer").
- No refresh-token generation, storage, or rotation logic anywhere in `AuthService`.

`RegisterAsync` throws a plain `Exception("Email already exists.")` if the email is taken, always assigns the `"Customer"` role by looking it up via `_context.Roles.FirstAsync(r => r.Name == "Customer")`. `LoginAsync` throws plain `Exception`s for "Invalid email or password." and "User account is inactive." — no custom exception types (unlike `AreaService`, which uses `DuplicateEntityException`/`ValidationException`).

### A3. Role modeling — `EatKath.API/Entities/User.cs` and `Entities/Role.cs`

Roles are a **separate database table**, not an enum/string on `User`:

- `Role` entity: `Id` (int), `Name` (string), `ICollection<User> Users`.
- `User` entity has `RoleId` (int FK) + navigation property `Role Role` (non-nullable `= null!`).
- The JWT claim itself only carries `user.Role.Name` as a plain string (`"Admin"`, `"Owner"`, `"Customer"` seen in code/tests), so at token-validation/`[Authorize(Roles=...)]` time it behaves like string-based roles even though it's backed by a normalized table.

### A4. `EatKath.API/Program.cs` — JWT/authorization setup

```
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();

```

- `AddAuthorization()` is called with **no custom policies** — only the default policy and role-based `[Authorize(Roles="...")]` attributes are used directly on controllers/actions.
- Middleware order: `UseCors` → `UseAuthentication` → `UseAuthorization` → custom `ExceptionMiddleware` → `UseStaticFiles` → `MapControllers`.
- `ICurrentUserService` and `IHttpContextAccessor` are registered (`AddScoped<ICurrentUserService, CurrentUserService>()`, `AddHttpContextAccessor()`).

### A5. `[Authorize]` usage across controllers (grep of `EatKath.API/Controllers`)

| ControllerPattern                                   |                                                                                                                                                                                                     |
| --------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `UserController.cs`                                 | Class-level `[Authorize(Roles = "Admin")]` — entire controller Admin-only                                                                                                                           |
| `OwnerController.cs`                                | Class-level `[Authorize(Roles = "Owner")]`                                                                                                                                                          |
| `UserFavoriteController.cs`                         | Class-level `[Authorize]` (any authenticated user, no role restriction)                                                                                                                             |
| `RedemptionController.cs`                           | Class-level `[Authorize]`; specific actions add `[Authorize(Roles = "Admin,Owner")]` / `[Authorize(Roles = "Owner,Admin")]`                                                                         |
| `AreasController.cs`                                | GET(s) public (no attribute); Create/Update/Delete each `[Authorize(Roles = "Admin")]`                                                                                                              |
| `CuisineController.cs`                              | Same pattern as Areas — mutating actions `[Authorize(Roles = "Admin")]`                                                                                                                             |
| `DiningTypeController.cs`                           | Same pattern — mutating actions `[Authorize(Roles = "Admin")]`                                                                                                                                      |
| `DealController.cs`                                 | Mixed: some actions `[Authorize(Roles = "Admin,Owner")]`, one `[Authorize(Roles = "Owner")]` (create), reads public                                                                                 |
| `MenuItemController.cs`                             | All mutating actions `[Authorize(Roles = "Admin,Owner")]`                                                                                                                                           |
| `RestaurantOpeningHourController.cs`                | Mutating actions `[Authorize(Roles = "Admin,Owner")]`                                                                                                                                               |
| `RestaurantImageController.cs`                      | Mutating actions `[Authorize(Roles = "Admin,Owner")]`                                                                                                                                               |
| `RestaurantController.cs`                           | Create restaurant `[Authorize(Roles = "Owner")]`; most other mutating actions `[Authorize(Roles = "Admin,Owner")]`; one Admin-only (`Authorize(Roles = "Admin")` at line 89, likely delete/approve) |
| `ReservationController.cs`                          | Fine-grained per-action: Admin-only (list all/cancel), Owner-only (restaurant reservation management, 6 actions), Customer-only (create/cancel own reservation)                                     |
| `MenuCategoryController.cs`, `UploadsController.cs` | No `[Authorize]` matches found — appear to have no auth restriction (public), based on grep results (not present in the match list)                                                                 |

Overall pattern: reference/lookup data (Areas, Cuisines, DiningTypes) is publicly readable but Admin-only to mutate; restaurant-owned resources (Deals, MenuItems, RestaurantImages, OpeningHours, Restaurant itself) are Admin+Owner; Reservations split cleanly by role (Customer creates/cancels own, Owner manages their restaurant's, Admin oversees all); Redemption and UserFavorite require at least authentication, with Redemption completion further gated to Admin/Owner.

### A6. `EatKath.API/Services/CurrentUserService.cs` + `Interfaces/ICurrentUserService.cs`

```
public interface ICurrentUserService
{
    int UserId { get; }
    string Role { get; }
    bool IsAdmin { get; }
}

```

```
public int UserId
{
    get
    {
        var claims = _httpContextAccessor.HttpContext?.User?.Claims;
        // (debug logging of every claim to Console omitted here)
        var userId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userId, out var id) ? id : 0;
    }
}

public string Role
{
    get
    {
        return _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}

public bool IsAdmin => Role == "Admin";

```

- Reads directly off `HttpContext.User` claims via `IHttpContextAccessor` (no caching, computed on every access).
- `UserId` pulled from `ClaimTypes.NameIdentifier`, defaults to `0` if missing/unparsable (silent failure rather than throwing).
- `Role` pulled from `ClaimTypes.Role`, defaults to empty string.
- Notably, `UserId` getter has leftover `Console.WriteLine` debug statements dumping all claims and the resolved id/every request — this looks like debug logging left in production code.

### A7. Frontend auth (light pass) — `EatKath.Web/src`

- **Storage**: `EatKath.Web/src/features/auth/AuthProvider.tsx` stores the entire `AuthResponse` object (including `token`) as JSON in `localStorage` under the key `"user"`. `login()`/`logout()` write/remove that key and update React state. Initial state is lazily hydrated from `localStorage` on mount.
- **Context**: `EatKath.Web/src/features/auth/AuthContext.tsx` defines `{ user, login, logout }` via `createContext`.
- **Attaching JWT to requests**: `EatKath.Web/src/api/axios.ts` uses an axios request interceptor:

```
api.interceptors.request.use((config) => {
    const storedUser = localStorage.getItem("user");
    if (storedUser) {
        const user = JSON.parse(storedUser);
        if (user.token) {
            config.headers.Authorization = `Bearer ${user.token}`;
        }
    }
    return config;
});

```

- **Route protection**: `EatKath.Web/src/features/auth/ProtectedRoute.tsx` reads `AuthContext`; redirects to `/login` if no user, redirects to `/` if a `role` prop is given and doesn't match `auth.user.role` (single-role string comparison, matching the single-role JWT claim on the backend).
- **Auth API calls**: `EatKath.Web/src/services/AuthService.ts` only implements `login()` (`POST /auth/login`); a comment block lists `logout()/register()/refreshToken()/forgotPassword()/changePassword()` as "Future methods" — confirming register exists on the backend but apparently isn't yet wired up in this service file (may exist elsewhere in the app, but not in this dedicated auth service).

No token-refresh handling (401 interceptor / silent refresh) exists in `axios.ts` — matches the backend having no refresh-token endpoint.

---

## Part B — Tests (`EatKath.API.Tests`)

### B1. `EatKath.API.Tests/EatKath.API.Tests.csproj`

- Framework: **MSTest** (`MSTest` 3.6.4 package; `Using Include="Microsoft.VisualStudio.TestTools.UnitTesting"` global using).
- Mocking: **Moq** 4.20.72.
- Assertions: **FluentAssertions** 8.10.0.
- DB: `Microsoft.EntityFrameworkCore.InMemory` 8.0.29.
- Target: `net8.0`, references `EatKath.API.csproj` directly.
- Declares empty `Validators\`, `TestData\`, `Controllers\` folders via `<Folder Include>` — **these folders contain no files** (confirmed by directory listing); they exist only as placeholders/scaffolding for future tests.

### B2. Test helpers

- `EatKath.API.Tests/Helpers/MapperFactory.cs`: builds a **real** AutoMapper `MapperConfiguration` using the actual `MappingProfile` from the API project (`cfg.AddProfile<MappingProfile>()`) — not a mock, so mapping bugs would be caught.
- `EatKath.API.Tests/Helpers/TestDbContextFactory.cs`: creates a fresh `ApplicationDbContext` backed by `UseInMemoryDatabase(Guid.NewGuid().ToString())` per call — guarantees full isolation between tests (no shared state).

### B3. Existing test classes (only 3 test classes exist, all under `Services/`)

**`EatKath.API.Tests/Services/AreaServiceTests.cs`** (3 tests) — covers `AreaService.CreateAsync` only:

- Creates area successfully when valid.
- Throws `DuplicateEntityException` when area name already exists.
- Throws `FluentValidation.ValidationException` when the mocked validator returns failures. No coverage of `AreaService`'s Update/Delete/Get methods.

**`EatKath.API.Tests/Services/DealServiceTests.cs`** (2 tests) — covers `DealService.CreateAsync` only:

- Creates deal successfully when restaurant exists.
- Throws generic `Exception("Restaurant not found.")` when RestaurantId doesn't exist. No coverage of Update/Delete/Get/List, no coverage of validation, dates, or owner-restriction logic in `DealService`.

**`EatKath.API.Tests/Services/RedemptionServiceTests.cs`** (10 tests) — by far the most thorough, covers `RedemptionService.RedeemAsync` and `CompleteRedemptionAsync`:

- `RedeemAsync`: success path; offer not found; offer inactive; arrival date outside offer period; arrival time outside offer window; restaurant inactive; guest count exceeds `MaximumGuests`; validation failure (FluentValidation) blocks creation.
- `CompleteRedemptionAsync`: success path (verifies `DiscountAmount`/`FinalAmount`/`BillAmount` math and status transition to `Completed`); redemption not found; redemption already completed (double-completion guard). Good edge-case coverage for the core redemption business rules; no coverage of any "list/get redemptions" query methods if they exist on the service.

### B4. Coverage gap — services/controllers with ZERO tests

Given the full backend service/controller list (Area, Auth, Cuisine, Deal, DiningType, MenuCategory, MenuItem, Owner, Redemption, Reservation, RestaurantImage, RestaurantOpeningHour, Restaurant, UserFavorite, User) versus only `AreaServiceTests`, `DealServiceTests`, `RedemptionServiceTests` existing:

**Zero test coverage (no test file at all):**

- `AuthService` / `AuthController` — no tests for register, login, password hashing, or JWT generation, despite this being the most security-critical code path.
- `CuisineService` / `CuisineController`
- `DiningTypeService` / `DiningTypeController`
- `MenuCategoryService` / `MenuCategoryController`
- `MenuItemService` / `MenuItemController`
- `OwnerDashboardService` (`IOwnerDashboardService`) / `OwnerController`
- `ReservationService` / `ReservationController`
- `RestaurantImageService` / `RestaurantImageController`
- `RestaurantOpeningHourService` / `RestaurantOpeningHourController`
- `RestaurantService` / `RestaurantController`
- `UserFavoriteService` / `UserFavoriteController`
- `UserService` / `UserController`
- `CurrentUserService`
- `FileStorageService`
- `UploadsController`

**Also zero coverage:**

- Every Controller (all controllers — no tests exist under `EatKath.API.Tests/Controllers`, the folder is empty).
- Every Validator (FluentValidation validators such as `CreateAreaValidator` etc. — `EatKath.API.Tests/Validators` folder is empty; validators are only ever mocked, never tested directly, even for `Area` and `Redemption` where validators are used in the service tests).
- No `TestData` builders exist yet (`EatKath.API.Tests/TestData` folder empty) — each test manually constructs entities inline.

**Partial coverage (only** **`CreateAsync`****/one method tested, not the full service):**

- `AreaService` — only `CreateAsync`, no Update/Delete/GetAll/GetById.
- `DealService` — only `CreateAsync`, no Update/Delete/GetAll/GetById.
- `RedemptionService` — `RedeemAsync` and `CompleteRedemptionAsync` well covered, but any other methods (e.g., listing/cancelling redemptions) are untested if present.

Net effect: out of 15 services/controllers logically in scope, only 3 services have any tests, and even those 3 only test their creation/core-transition methods, not full CRUD. Auth (login/register/JWT) has no automated test coverage whatsoever.

Good, that's a comprehensive seed data setup. Now I'll wait for the background agents to finish before assembling the full structured report.

**ScheduleWakeup**

IN

```
{
  "delaySeconds": 180,
  "reason": "fallback check in case agent completion notifications are delayed while the four exploration agents finish",
  "prompt": "<<autonomous-loop-dynamic>>",
  "noop": true
}
```

OUT

```
Next wakeup scheduled for 11:52:00 (in 227s). Nothing more to do this turn — the harness re-invokes you when the wakeup fires or a task-notification arrives.
```

**Bash **Search model snapshot for precision, indexes, delete behavior

IN

```
grep -n "HasPrecision\|class Reservation\|Redemption\b\|MenuItem\b\|IX_\|OnDelete\|DeleteBehavior" "c:\Users\snjja\source\localrepos\EatKath\EatKath.API\Migrations\ApplicationDbContextModelSnapshot.cs" | head -100
```

OUT

```
81:                        .HasPrecision(18, 2)
181:            modelBuilder.Entity("EatKath.API.Entities.MenuItem", b =>
210:                        .HasPrecision(18, 2)
225:            modelBuilder.Entity("EatKath.API.Entities.Redemption", b =>
240:                        .HasPrecision(18, 2)
253:                        .HasPrecision(18, 2)
257:                        .HasPrecision(18, 2)
593:                        .OnDelete(DeleteBehavior.Cascade)
604:                        .OnDelete(DeleteBehavior.Cascade)
610:            modelBuilder.Entity("EatKath.API.Entities.MenuItem", b =>
615:                        .OnDelete(DeleteBehavior.Cascade)
621:                        .OnDelete(DeleteBehavior.NoAction)
629:            modelBuilder.Entity("EatKath.API.Entities.Redemption", b =>
634:                        .OnDelete(DeleteBehavior.Restrict)
640:                        .OnDelete(DeleteBehavior.Restrict)
653:                        .OnDelete(DeleteBehavior.Cascade)
659:                        .OnDelete(DeleteBehavior.Restrict)
672:                        .OnDelete(DeleteBehavior.Restrict)
678:                        .OnDelete(DeleteBehavior.Restrict)
691:                        .OnDelete(DeleteBehavior.Cascade)
697:                        .OnDelete(DeleteBehavior.Cascade)
710:                        .OnDelete(DeleteBehavior.Cascade)
716:                        .OnDelete(DeleteBehavior.Cascade)
729:                        .OnDelete(DeleteBehavior.Cascade)
740:                        .OnDelete(DeleteBehavior.Cascade)
751:                        .OnDelete(DeleteBehavior.Restrict)
762:                        .OnDelete(DeleteBehavior.Cascade)
768:                        .OnDelete(DeleteBehavior.Cascade)
```

That scheduling tool is meant for `/loop` sessions specifically, not for general background-agent waiting — this isn't a loop context, so let me cancel that to avoid confusion.

**ScheduleWakeup**

IN

```
{
  "stop": true
}
```

OUT

```
Loop stopped — cancelled 1 pending wakeup(s); no further dynamic-loop wakeups scheduled. If you armed a Monitor for this loop, TaskStop it now; otherwise nothing more to do this turn.
```

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API\Validators\RestaurantImage\CreateRestaurantImageValidator.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

I'll simply wait for the four research agents to report back — they'll notify me automatically when done.

**Grep** "class Reservation$|Reservation\\", b =>|HasIndex|IX\_MenuItems\_RestaurantId" (in c:\Users\snjja\source\localrepos\EatKath\EatKath.API\Migrations\ApplicationDbContextModelSnapshot.cs)

19 lines of output

# Database / EF Core Implementation Report — EatKath.API

## 1. `ApplicationDbContext.cs` — DbSets and OnModelCreating

Location: `EatKath.API/Data/ApplicationDbContext.cs`

**DbSets** (grouped by the file's own comments):

- Lookup: `Roles`, `Areas`, `Cuisines`, `DiningTypes`
- Core: `Users`, `Restaurants`
- Restaurant: `RestaurantImages`, `RestaurantOpeningHours`, `RestaurantCuisines`, `RestaurantDiningTypes`
- Menu: `MenuCategories`, `MenuItems`
- Business: `Deals`, `Redemptions`, `UserFavorites`
- `Reservations` (declared as a plain `{ get; set; }` auto-property — the only DbSet not exposed via the `Set<T>()` pattern used by all others; inconsistent style but functionally equivalent)

**Key** **`OnModelCreating`** **configuration** (verbatim structure):

```
// Composite Keys
modelBuilder.Entity<RestaurantCuisine>().HasKey(x => new { x.RestaurantId, x.CuisineId });
modelBuilder.Entity<RestaurantDiningType>().HasKey(x => new { x.RestaurantId, x.DiningTypeId });
modelBuilder.Entity<UserFavorite>().HasKey(x => new { x.UserId, x.RestaurantId });

// User -> Role (Restrict)
modelBuilder.Entity<User>().HasOne(x => x.Role).WithMany(x => x.Users)
    .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);

// Restaurant -> Owner (User, Restrict), Restaurant -> Area (Restrict)
// RestaurantImage -> Restaurant (Cascade)
// RestaurantOpeningHour -> Restaurant (Cascade)
// RestaurantCuisine -> Restaurant/Cuisine (no explicit OnDelete -> EF default Cascade, confirmed in snapshot)
// RestaurantDiningType -> Restaurant/DiningType (same, default Cascade)

// MenuItem -> Restaurant (NoAction)   <-- deliberately NOT cascade
// MenuItem -> MenuCategory (Cascade)

// Deal -> Restaurant (Cascade)

// Redemption -> User (Restrict), Redemption -> Deal (Restrict)

// UserFavorite -> User / Restaurant (no explicit OnDelete -> default Cascade)

// Decimal precision (18,2) explicitly set for:
//   Deal.DiscountPercentage, MenuItem.Price,
//   Redemption.BillAmount, Redemption.DiscountAmount, Redemption.FinalAmount

// Reservation -> User (Restrict)
// Reservation -> Deal: .HasOne(x => x.Deal).WithMany() [no inverse collection] .OnDelete(Cascade)

// Unique index
modelBuilder.Entity<Area>().HasIndex(x => x.Name).IsUnique();

```

Notable: this is the **only** unique index defined anywhere in the model (no unique index on `User.Email`, `Restaurant.Name`, `Cuisine.Name`, `DiningType.Name`, `Reservation.ConfirmationCode`, etc.).

## 2. `Data/Seeders` folder

16 files, all static classes with a single `SeedAsync(ApplicationDbContext context)` method, each idempotent (`if (context.X.Any()) return;`). Orchestrated by `Data/Seeders/DatabaseSeeder.cs`, which is called from **`Program.cs`** (not from a migration) inside a startup block with a SQL-Server-not-ready retry loop (`context.Database.MigrateAsync()` then seed).

Seeding order in `DatabaseSeeder.SeedAsync`:

1. `RoleSeeder` — Admin, Owner, Customer
2. `UserSeeder` — 1 admin, 4 owners, 5 customers, all password `"Password77"` hashed via `Microsoft.AspNetCore.Identity.PasswordHasher<User>` (note: `ApplicationDbContext`/entities don't use ASP.NET Identity elsewhere — this hasher is used purely as a convenient bcrypt-like utility; actual login flow uses `BCrypt.Net-Next`, see `AuthService`/`.csproj`, so there is a hashing-scheme mismatch between seeded demo users and real registration — seeded users likely can't log in via `AuthService.Login` if it expects BCrypt hashes)
3. `AreaSeeder` — 20 Kathmandu-valley area names
4. `CuisineSeeder` — 20 cuisines
5. `DiningTypeSeeder` — 10 dining types
6. `RestaurantSeeder` — 50 demo businesses (restaurants/bakeries/cafes) split across 2 owner accounts, using `picsum.photos` placeholder images
7. `RestaurantOpeningHourSeeder` — 7 days/week per restaurant with bakery/cafe/restaurant-specific hours
8. `RestaurantCuisineSeeder` — name-heuristic cuisine tagging
9. `RestaurantDiningTypeSeeder` — name-heuristic dining-type tagging
10. `DealSeeder` — 2 deals/restaurant (Lunch Special dine-in, Evening Takeaway)
11. `MenuCategorySeeder` — 4 categories/restaurant (varies by bakery/cafe/other)
12. `MenuItemSeeder` — 3 items per category
13. `RestaurantImageSeeder` — 3 gallery images per restaurant
14. `UserFavoriteSeeder` — hardcoded favorites for 3 customers
15. `RedemptionSeeder` — 5 hardcoded redemption records across statuses (Completed/Cancelled/Redeemed/Expired)

No seeder exists for `Reservation` entities.

## 3. `Entities` folder — every class

- **`BaseEntity`** (abstract): `Id (int)`, `CreatedAt (DateTime, default UtcNow)`, `UpdatedAt (DateTime?)`. Inherited by `Restaurant`, `User`, `Deal`, `MenuCategory`, `RestaurantImage`, `Redemption`. **Not** inherited by `Area`, `Cuisine`, `DiningType`, `Role`, `MenuItem`, `RestaurantCuisine`, `RestaurantDiningType`, `RestaurantOpeningHour`, `UserFavorite`, `Reservation` (these define their own `Id`/timestamps or none at all).
- **`Area`**: `Id`, `Name`; nav `Restaurants` (1-many). No `BaseEntity`.
- **`Cuisine`**: `Id`, `Name`; nav `RestaurantCuisines` (many-many join).
- **`DiningType`**: `Id`, `Name`; nav `RestaurantDiningTypes` (many-many join).
- **`Role`**: `Id`, `Name`; nav `Users` (1-many).
- **`Deal`** (: `BaseEntity`): FK `RestaurantId`; `Title`, `Description`, `DiscountPercentage (decimal)`, `OfferType (enum)`, `PromoImageUrl`, `TermsAndConditions`, `StartDate/EndDate (DateOnly)`, `StartTime/EndTime (TimeOnly)`, `MaximumGuests`, `ReservationLimit (default 0 = unlimited)`, `DailyRedemptionLimit (default 0 = unlimited)`, `IsActive`; nav `Restaurant`, `Redemptions`.
- **`MenuCategory`** (: `BaseEntity`): FK `RestaurantId`; `Name`, `DisplayOrder`; nav `Restaurant`, `MenuItems`.
- **`MenuItem`** (plain, no `BaseEntity` — no timestamps): `Id`, FK `MenuCategoryId`, `Name`, `Description`, `Price (decimal)`, `IsFeatured`, `IsAvailable`, nav `MenuCategory`, nav `Restaurant` + FK `RestaurantId` (denormalized direct FK to Restaurant in addition to MenuCategory), `ImageUrl (string?)`.
- **`Redemption`** (: `BaseEntity`): FK `DealId`, FK `UserId`; `ArrivalDate (DateOnly)`, `ArrivalTime (TimeOnly)`, `GuestCount`, `BillAmount/DiscountAmount/FinalAmount (decimal?, nullable)`, `Status (RedemptionStatus enum, default Redeemed)`, `RedeemedAt (default UtcNow)`, `CompletedAt (DateTime?)`; nav `Deal`, `User`.
- **`Reservation`** (plain, no `BaseEntity`, uses DataAnnotations instead of fluent config for most validation): `Id`, FK `DealId` (`[Required]`), nav `Deal`, FK `UserId` (no `[Required]` attribute but non-nullable int), nav `User`, `CustomerName [Required, MaxLength(100)]`, `PhoneNumber [Required, MaxLength(20)]`, `Email [MaxLength(100)]?`, `ReservationDate (DateOnly)`, `ReservationTime (TimeOnly)`, `GuestCount [Range(1,20)]`, `Status (string, MaxLength(20), default "Pending")`, `ConfirmationCode (string, MaxLength(20), default = random 8-char hex from a GUID generated at class-instantiation time)`, `CreatedAt (default UtcNow)`.
- **`ReservationStatus`**: static class of `const string` values (Pending, Confirmed, Rejected, Arrived, Completed, NoShow, Cancelled) — **not a real enum**, stored as `string` on `Reservation.Status`.
- **`Restaurant`** (: `BaseEntity`): FK `OwnerId` (→ User), `Name`, `Description`, `Address`, FK `AreaId`, `PhoneNumber`, `Email`, `Website`, `LogoUrl`, `IsActive`; nav `Owner`, `Area`, `Images`, `OpeningHours`, `RestaurantCuisines`, `RestaurantDiningTypes`, `MenuCategories`, `MenuItems`, `Deals`, `UserFavorites`; plus `CoverImageUrl?`, `MenuPdfUrl?` (added later via migration, sit below the nav-property block in source).
- **`RestaurantCuisine`** (join entity, plain, composite PK via fluent config): `RestaurantId`, `CuisineId`; nav `Restaurant`, `Cuisine`.
- **`RestaurantDiningType`** (join entity, plain, composite PK via fluent config): `RestaurantId`, `DiningTypeId`; nav `Restaurant`, `DiningType`.
- **`RestaurantImage`** (: `BaseEntity`): FK `RestaurantId`, `ImageUrl`, `Caption?`, `DisplayOrder`, `IsPrimary`; nav `Restaurant`.
- **`RestaurantOpeningHour`** (plain, no `BaseEntity`): `Id`, FK `RestaurantId`, `DayOfWeek (enum System.DayOfWeek)`, `OpenTime/CloseTime (TimeOnly)`, `IsClosed`; nav `Restaurant`.
- **`User`** (: `BaseEntity`): `FirstName`, `LastName`, `Email`, `PasswordHash`, `PhoneNumber`, FK `RoleId`, `IsActive`; nav `Role`, `Restaurants` (as owner), `UserFavorites`, `Redemptions`, `Reservations`.
- **`UserFavorite`** (join entity, plain, composite PK via fluent config): `UserId`, `RestaurantId`, `CreatedAt (DateTime, added by a later migration)`; nav `User`, `Restaurant`.

## 4. `Enums` folder

- **`OfferType`** (`EatKath.API.Enums`): `DineIn = 1`, `Takeaway = 2`, `Delivery = 3`. Used on `Deal.OfferType`.
- **`RedemptionStatus`** (`EatKath.API.Enums`): `Redeemed = 1`, `Completed = 2`, `Cancelled = 3`, `Expired = 4`. Used on `Redemption.Status`. Note file has a redundant `using EatKath.API.Enums;` inside its own `EatKath.API.Enums` namespace.

(Note there is also a completely separate, unrelated static-string `ReservationStatus` class inside the `Entities` namespace — not an enum, easy to confuse with `RedemptionStatus`.)

## 5. Migrations — chronological schema evolution

From `EatKath.API/Migrations`:

1. **`20260725091007_InitialCreate`** — creates all base tables: `Areas`, `Cuisines`, `DiningTypes`, `Roles`, `Users`, `Restaurants`, `Deals` (with old `OriginalPrice`/`DiscountedPrice` decimal columns and `DateTime` Start/EndDate), `MenuCategories`, `RestaurantCuisines`, `RestaurantDiningTypes`, `RestaurantImages`, `RestaurantOpeningHours`, `UserFavorites`, `Redemptions` (with old single `RedemptionAmount` decimal column), `MenuItems`. Establishes the unique index on `Areas.Name` and standard FK indexes. `MenuItems.RestaurantId` FK created with **no explicit onDelete** (defaults to NoAction at DB-generation time for this original migration — later reconfirmed via fluent config as NoAction).
2. **`20260725225259_UpdateRestaurantImage`** — drops `CreatedAt`/`UpdatedAt` from `RestaurantImages`, renames `IsPrimary` → `IsLogo`, adds `DisplayOrder`.
3. **`20260726070403_AddCreatedAtToUserFavorite`** — adds `CreatedAt` to `UserFavorites`.
4. **`20260726104127_RefactorOfferModel`** — major Deal/Redemption rework: drops `RedemptionAmount` (Redemptions) and `DiscountedPrice` (Deals); renames `Deals.OriginalPrice` → `DiscountPercentage`; adds redemption workflow columns (`ArrivalDate`, `ArrivalTime`, `BillAmount`, `CompletedAt`, `DiscountAmount`, `FinalAmount`, `GuestCount`, `Status`); converts `Deals.StartDate/EndDate` from `datetime2` to `date`; adds `Deals.AdvanceRedeemMinutes`, `DailyRedemptionLimit`, `EndTime`, `StartTime`, `IsFeatured`, `MaximumGuests`, `OfferType`.
5. **`20260726111413_OfferModelRefactor`** — drops `Deals.IsFeatured` (reverses part of previous migration).
6. **`20260727001823_RemoveAdvanceRedeemMinutes`** — drops `Deals.AdvanceRedeemMinutes`.
7. **`20260728223747_ConfigureDecimalPrecision`** — **empty Up/Down** (model-only change; adds `HasPrecision(18,2)` fluent config for decimal columns without altering the DB schema, since SQL Server default already stored decimal(18,2) — a "silence the EF Core precision warning" migration).
8. **`20260802132114_AddRestaurantAndMenuImages`** — renames `RestaurantImages.IsLogo` back to `IsPrimary`; adds `Restaurants.CoverImageUrl`, `Restaurants.MenuPdfUrl`; re-adds `RestaurantImages.Caption`, `CreatedAt`, `UpdatedAt`; adds `MenuItems.ImageUrl`.
9. **`20260803024454_AddReservations`** — creates `Reservations` table (Deal-only FK at this point, cascade delete), with `ConfirmationCode`, `Status` as strings.
10. **`20260804001238_AddReservationUser`** — adds `Reservations.UserId` FK → `Users` (Restrict), with index.
11. **`20260804014114_AddReservationLimit`** — adds `Deals.ReservationLimit`.

`ApplicationDbContextModelSnapshot.cs` confirms current final shape matches entity/DbContext code exactly: decimal(18,2) precision on `Deal.DiscountPercentage`, `MenuItem.Price`, `Redemption.BillAmount/DiscountAmount/FinalAmount`; unique index only on `Areas.Name`; `MenuItem→Restaurant` is `DeleteBehavior.NoAction`; `Reservation→Deal` is `Cascade` while `Reservation→User` is `Restrict`; `RestaurantCuisine`/`RestaurantDiningType`/`UserFavorite` relations all resolve to `Cascade` (EF Core default for required FKs, since the DbContext never explicitly restricts them).

## 6. Database provider

- **SQL Server**, confirmed by:
  - `EatKath.API.csproj`: `Microsoft.EntityFrameworkCore.SqlServer` v8.0.29 (alongside `Microsoft.EntityFrameworkCore` and `.Design` v8.0.29, and `.Tools` v10.0.10 — a version mismatch worth flagging, Tools is EF 10 while the rest of the stack is EF 8).
  - `Program.cs`: `builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(...))`.
  - `appsettings.json` connection string: `"Server=(localdb)\\MSSQLLocalDB;Database=EatKathDB;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;"` — LocalDB, Windows-integrated auth, MARS enabled.
  - Migration column types (`nvarchar`, `datetime2`, `decimal(18,2)`, `time`, `date`, `bit`, `SqlServer:Identity` annotations) are SQL-Server-specific.
  - `Program.cs` also explicitly catches `Microsoft.Data.SqlClient.SqlException` during the startup retry loop.

## 7. Full relationship map

**One-to-many:**

- `Role` 1—\* `User` (`User.RoleId`, Restrict)
- `Area` 1—\* `Restaurant` (`Restaurant.AreaId`, Restrict)
- `User` 1—\* `Restaurant` as Owner (`Restaurant.OwnerId`, Restrict)
- `Restaurant` 1—\* `RestaurantImage` (`RestaurantImage.RestaurantId`, Cascade)
- `Restaurant` 1—\* `RestaurantOpeningHour` (`RestaurantOpeningHour.RestaurantId`, Cascade)
- `Restaurant` 1—\* `MenuCategory` (`MenuCategory.RestaurantId`, Cascade)
- `Restaurant` 1—\* `MenuItem` (`MenuItem.RestaurantId`, **NoAction** — deliberate, likely to avoid multiple-cascade-path SQL Server error since `MenuItem` also cascades from `MenuCategory`)
- `MenuCategory` 1—\* `MenuItem` (`MenuItem.MenuCategoryId`, Cascade)
- `Restaurant` 1—\* `Deal` (`Deal.RestaurantId`, Cascade)
- `Deal` 1—\* `Redemption` (`Redemption.DealId`, Restrict)
- `User` 1—\* `Redemption` (`Redemption.UserId`, Restrict)
- `User` 1—\* `Reservation` (`Reservation.UserId`, Restrict)
- `Deal` 1—\* `Reservation` (`Reservation.DealId`, Cascade; no inverse `Deal.Reservations` collection — configured with `.WithMany()`, i.e., unidirectional from EF's perspective)

**Many-to-many (via explicit join entities):**

- `Restaurant` *—* `Cuisine` via `RestaurantCuisine` (composite PK `RestaurantId+CuisineId`; both FKs Cascade)
- `Restaurant` *—* `DiningType` via `RestaurantDiningType` (composite PK `RestaurantId+DiningTypeId`; both FKs Cascade)
- `User` *—* `Restaurant` via `UserFavorite` (composite PK `UserId+RestaurantId`; both FKs Cascade)

**No true one-to-one relationships exist** anywhere in the model.

**FK ownership summary:** the "many" side always owns the FK column, consistent with standard EF conventions — e.g., `Restaurant` owns `OwnerId`/`AreaId`; `MenuItem` owns both `MenuCategoryId` and a separate (denormalized) `RestaurantId`; `Redemption` owns `DealId`/`UserId`; `Reservation` owns `DealId`/`UserId`.

## 8. Inconsistencies / orphaned or notable items

- **`MenuItem`**** has a redundant/denormalized FK**: it stores both `MenuCategoryId` (cascade to category) and `RestaurantId` (NoAction to restaurant) even though the restaurant is derivable via `MenuCategory.RestaurantId`. This double-parentage is unusual and forces `NoAction` to sidestep SQL Server's "multiple cascade paths" restriction.
- **`Reservation.Status`**** is a free-text ****`string`** governed only by the non-EF-tracked `ReservationStatus` static-const helper class, not a real enum or DB constraint/check — no fluent config enforces the allowed values at the DB level.
- **`ReservationStatus`**** (Entities) vs ****`RedemptionStatus`**** (Enums)** — two similarly named but structurally different status representations (string constants vs. int enum) for parallel "redeem/reserve" workflows; easy to confuse.
- **No unique index anywhere except ****`Area.Name`****.** Notably missing: unique index on `User.Email` (login uniqueness is only enforced, if at all, in application code/service layer — not the DB), no unique constraint on `Cuisine.Name`/`DiningType.Name` (seeder relies on there being no duplicates, but nothing prevents it), no unique constraint on `Reservation.ConfirmationCode`.
- **`Deal.ReservationLimit`****/****`DailyRedemptionLimit`** use `0` as a magic number for "unlimited" — not enforced by any DB check constraint, just a code comment convention.
- **Entity design inconsistency**: some entities inherit `BaseEntity` (get `Id`, `CreatedAt`, `UpdatedAt` for free) while structurally similar entities (`MenuItem`, `RestaurantOpeningHour`, `Reservation`, join tables) declare their own `Id`/timestamps or omit timestamps entirely. `RestaurantOpeningHour` and `MenuItem` have no `CreatedAt`/`UpdatedAt` at all, unlike `RestaurantImage`/`MenuCategory`.
- **`Reservations`**** DbSet property style differs** from all other DbSets in `ApplicationDbContext` (auto-property vs. `Set<T>()` expression-bodied property) — cosmetic but inconsistent.
- **Package version mismatch** in the `.csproj`: `Microsoft.EntityFrameworkCore.Tools` is pinned to `10.0.10` while `Microsoft.EntityFrameworkCore`, `.Design`, and `.SqlServer` are all `8.0.29` — a potential tooling/runtime version mismatch risk (though `.Tools` is dev-time only).
- **Seed-time password hashing mismatch**: `UserSeeder` hashes demo user passwords using ASP.NET Identity's `PasswordHasher<User>`, whereas the app's real registration/login (`AuthService`, per `.csproj`'s `BCrypt.Net-Next` dependency) most likely uses BCrypt — meaning seeded demo accounts (`owner@eatkath.com`, `emma.johnson@eatkath.com`, etc.) may not actually authenticate correctly through the real login flow (would need to check `AuthService.cs` to confirm, but the hasher choice is clearly inconsistent between seeding and the app's auth dependency).
- **No seeder exists for ****`Reservation`** — the entity/table exists and is fully wired into the DbContext/relationships, but is never populated by `DatabaseSeeder`, so it starts empty in every fresh environment.
- **`Migration 20260728223747_ConfigureDecimalPrecision`** has empty `Up`/`Down` bodies — a model-only migration with no actual DB schema effect, just documents/synchronizes the EF model snapshot with `HasPrecision` calls already matching the underlying SQL Server decimal(18,2) default.

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API\DTOs\Reservation\CreateReservationDto.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

**Read **[c:\Users\snjja\source\localrepos\EatKath\EatKath.API\DTOs\Auth\LoginDto.cs](vscode-webview://0toss23s4mq8vnml0msg4cfn11t4esc9ttubvk6gfdtohmcs9b5e/index.html?id=dcde6fd9-7114-4670-bf41-ac63e91e4073\&parentId=1\&origin=3a7f7d61-a236-466b-b477-25ec6cbd545d\&swVersion=6\&extensionId=Anthropic.claude-code\&platform=electron\&vscode-resource-base-authority=vscode-resource.vscode-cdn.net\&parentOrigin=vscode-file%3A%2F%2Fvscode-app\&purpose=webviewView\&session=5a02a44a-502a-48dc-8160-73bb3bae4fdd#)

Frontend research is in — very detailed findings, including some real gaps (unused deps, empty file, routing bug). Waiting on the backend, database, and auth/tests agents now.

**Bash **List Enums folder files

IN

```
cd "c:\Users\snjja\source\localrepos\EatKath\EatKath.API\Enums" && ls
```

OUT

```
OfferType.cs
RedemptionStatus.cs
```

Auth/tests findings are in — notably: leftover debug `Console.WriteLine` in `CurrentUserService`, no refresh-token flow, and auth itself has zero test coverage. Waiting on backend and database agents now.

**Bash **Resolve conflict between agents about password hashing mechanism

IN

```
grep -n "BCrypt\|PasswordHasher" EatKath.API/EatKath.API.csproj EatKath.API/Services/AuthService.cs EatKath.API/Data/Seeders/UserSeeder.cs
```

OUT

```
EatKath.API/EatKath.API.csproj:11:    <PackageReference Include="BCrypt.Net-Next" Version="4.2.0" />
EatKath.API/Services/AuthService.cs:20:        private readonly PasswordHasher<Entities.User> _passwordHasher;
EatKath.API/Services/AuthService.cs:31:            _passwordHasher = new PasswordHasher<Entities.User>();
EatKath.API/Data/Seeders/UserSeeder.cs:13:            var hasher = new PasswordHasher<User>();
```

Resolved a conflict between two agents: both `AuthService` and `UserSeeder` actually use ASP.NET Identity's `PasswordHasher<User>` consistently — so seeded demo accounts *can* log in. `BCrypt.Net-Next` is a referenced but unused package (same "installed but dead" pattern as the frontend's unused zod/react-query/notistack). Now waiting on the backend controllers/services agent, the last one still running.

I now have a complete picture. Here is the structured report.

## 1. EatKath.API.csproj

Path: `EatKath.API/EatKath.API.csproj`

- SDK: `Microsoft.NET.Sdk.Web`, `TargetFramework` = **net8.0**, `Nullable` enabled, `ImplicitUsings` enabled.
- Key packages (all pinned to 8.0.29 for EF/JWT, except EF Tools which is inconsistently pinned to `10.0.10`):
  - `AutoMapper.Extensions.Microsoft.DependencyInjection` 12.0.1
  - `BCrypt.Net-Next` 4.2.0 (referenced but **not actually used** — `AuthService`/`UserService` use ASP.NET Core Identity's `PasswordHasher<T>`, not BCrypt)
  - `FluentValidation` 12.1.1 and `FluentValidation.DependencyInjectionExtensions` 12.1.1
  - `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.29
  - `Microsoft.EntityFrameworkCore` / `.SqlServer` / `.Design` 8.0.29
  - `Microsoft.EntityFrameworkCore.Tools` **10.0.10** (mismatched major version vs the rest, likely a typo/floating install)
  - `Swashbuckle.AspNetCore` 6.6.2 (Swagger)
- Explicit empty `<Folder Include>` entries for `Helpers`, `Configurations`, `Extensions`, `Repositories`, `Validators\MenuItem`, `wwwroot\uploads\restaurants` — these exist only as placeholders in the project file; on disk they are empty (see §9/§11).

## 2. Program.cs (`EatKath.API/Program.cs`)

Startup order:

1. `AddControllers()`
2. CORS policy `"ReactPolicy"` — allows origins `http://localhost:5173` and `https://yellow-ocean-0fc06c300.7.azurestaticapps.net`, any header/method (lines 38-50).
3. `AddDbContext<ApplicationDbContext>` using SQL Server, connection string `DefaultConnection` (line 56-58).
4. DI registrations — every domain service is registered `Scoped` against its interface (lines 64-79): `IAreaService/AreaService`, `ICuisineService`, `IDiningTypeService`, `IRestaurantService`, `IUserService`, `IAuthService`, `IDealService`, `IMenuCategoryService`, `IMenuItemService`, `IRestaurantImageService`, `IRestaurantOpeningHourService`, `IUserFavoriteService`, `IRedemptionService`, `IOwnerDashboardService`, `ICurrentUserService`, plus `FileStorageService` (registered as concrete class, no interface). `IReservationService` is registered separately, out of alphabetical order, at line 87.
5. `AddAutoMapper(typeof(MappingProfile))` — note only `MappingProfile` is passed as the marker type, but AutoMapper's DI extension scans the *whole assembly* for `Profile` subclasses, so `RestaurantProfile` and `UserProfile` are picked up too (confirmed both exist and are used).
6. `AddHttpContextAccessor()`.
7. `AddValidatorsFromAssemblyContaining<CreateAreaValidator>()` — assembly-scan registration of all FluentValidation validators.
8. Swagger: `AddEndpointsApiExplorer()` + `AddSwaggerGen` with a Bearer JWT security definition/requirement (lines 93-120).
9. JWT auth: `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` reading `Issuer`/`Audience`/`SecretKey` from `JwtSettings` config, validating issuer/audience/lifetime/signing key (lines 126-144). `AddAuthorization()` with no policies (role checks done purely via `[Authorize(Roles=...)]` on controllers).

Middleware pipeline (`app` — lines 152-181), in this exact order:

```
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();   // <-- placed AFTER auth/authorization
app.UseStaticFiles();

app.MapControllers();

```

Note: the commented-out `if (app.Environment.IsDevelopment())` guard (lines 158-162) shows Swagger was originally meant to be dev-only but is now unconditionally enabled in all environments. Also notable: `ExceptionMiddleware` runs *after* `UseAuthentication`/`UseAuthorization`, meaning it won't catch exceptions thrown by the auth/authorization middleware itself, only exceptions from later middleware/controllers (it does still wrap `UseStaticFiles` and `MapControllers`).

Startup also seeds the DB (`DatabaseSeeder.SeedAsync`) in a retry loop (up to 10 attempts, 5s apart) inside a `using (var scope = ...)` block after `app` is built but before `app.Run()` (lines 187-225), specifically to tolerate SQL Server not being ready yet (e.g. containerized DB).

## 3. Controllers (`EatKath.API/Controllers`) — all 16 read

**AuthController** (`api/Auth`) — no class-level `[Authorize]`.

- `POST register` `[AllowAnonymous]` → `IAuthService.RegisterAsync` → `AuthResponseDto`
- `POST login` `[AllowAnonymous]` → `IAuthService.LoginAsync` → `AuthResponseDto`

**AreasController** (`api/Areas`)

- `GET` `[AllowAnonymous]` → `IAreaService.GetAllAsync`
- `GET {id}` `[AllowAnonymous]` → `GetByIdAsync` → 404 if null
- `POST` `[Authorize(Roles="Admin")]` → `CreateAsync` → `CreatedAtAction`
- `PUT {id}` `[Authorize(Roles="Admin")]` → `UpdateAsync` → 404/200
- `DELETE {id}` `[Authorize(Roles="Admin")]` → `DeleteAsync` → 404/204

**CuisineController** (`api/Cuisine`) — identical CRUD shape to Areas: public GET/GetById, Admin-only Create/Update/Delete.

**DiningTypeController** (`api/DiningType`) — identical CRUD shape to Areas: public GET/GetById, Admin-only Create/Update/Delete.

**UserController** (`api/User`) — class-level `[Authorize(Roles="Admin")]` (entire controller Admin-only).

- `GET`, `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}` → `IUserService` CRUD methods.

**MenuCategoryController** (`api/MenuCategory`) — **no** **`[Authorize]`** **anywhere** (fully anonymous-accessible, including Create/Update/Delete at the controller level — authorization is instead enforced deep inside `MenuCategoryService` via `ICurrentUserService.IsAdmin`/`UserId` checks that throw `BusinessRuleException`).

- `GET`, `GET {id}`, `GET restaurant/{restaurantId}`, `POST`, `PUT {id}`, `DELETE {id}` → `IMenuCategoryService`.

**RestaurantOpeningHourController** (`api/RestaurantOpeningHour`)

- `GET`, `GET {id:int}`, `GET restaurant/{restaurantId:int}` — public.
- `POST`, `PUT {id:int}`, `DELETE {id:int}` — `[Authorize(Roles="Admin,Owner")]`, no ownership check in the service itself.

**UserFavoriteController** (`api/UserFavorite`) — class-level `[Authorize]` (any authenticated user).

- `GET` → `GetMyFavoritesAsync` (uses `ICurrentUserService`)
- `POST` → `AddAsync`
- `DELETE` (body-based) → `RemoveAsync`

**OwnerController** (`api/Owner`) — class-level `[Authorize(Roles="Owner")]`.

- `GET dashboard` → `IOwnerDashboardService.GetDashboardAsync` → `OwnerDashboardDto`

**UploadsController** (`api/Uploads`) — **no** **`[Authorize]`** **at all** — fully anonymous file upload endpoint.

- `POST image` → saves any uploaded file (no extension whitelist, no service layer, writes directly to `wwwroot/uploads/restaurants` using `IWebHostEnvironment` directly in the controller) → returns `{ imageUrl }`. This is a separate/duplicate upload path from `FileStorageService`, with no content-type/extension validation, unlike `FileStorageService.SaveImageAsync`.

**RestaurantImageController** (`api/RestaurantImage`)

- `GET`, `GET {id}`, `GET restaurant/{restaurantId}` — public.
- `POST`, `PUT {id}`, `POST upload` (`[FromForm]`), `DELETE {id}` — `[Authorize(Roles="Admin,Owner")]`.

**MenuItemController** (`api/MenuItem`)

- `GET`, `GET {id}`, `GET restaurant/{restaurantId}`, `GET category/{categoryId}` — public.
- `POST`, `PUT {id}`, `POST {id}/image`, `DELETE {id}`, `DELETE {id}/image` — `[Authorize(Roles="Admin,Owner")]`.

**DealController** (`api/Deal`)

- `GET`, `GET {id}` — public.
- `POST`, `PUT {id}`, `DELETE {id}` — `[Authorize(Roles="Admin,Owner")]`.
- `GET my` — `[Authorize(Roles="Owner")]`, reads `ClaimTypes.NameIdentifier` directly from `User` in the controller (not via `ICurrentUserService`) → `GetByOwnerAsync`.
- `GET restaurant/{restaurantId}` — public.

**ReservationController** (`api/Reservation`)

- `GET` `[Authorize(Roles="Admin")]` → all reservations.
- `GET owner` `[Authorize(Roles="Owner")]` → reads NameIdentifier claim directly.
- `GET my` `[Authorize(Roles="Customer")]` → reads NameIdentifier claim directly.
- `GET {id}` — **no authorize attribute** (any caller, even anonymous, can fetch any reservation by id).
- `POST` `[Authorize(Roles="Customer")]` — has a duplicated `[HttpPost]` attribute (lines 89-90, harmless but sloppy) and wraps the call in try/catch returning `BadRequest(ex.Message)` (the only controller that catches exceptions itself instead of relying on `ExceptionMiddleware`).
- `PUT {id}/confirm`, `/reject`, `/arrived`, `/completed`, `/no-show`, `/cancel` — all `[Authorize(Roles="Owner")]`, no ownership verification (any Owner can transition any restaurant's reservations).
- `DELETE {id}` `[Authorize(Roles="Admin")]`.

**RedemptionController** (`api/Redemption`) — class-level `[Authorize]`.

- `POST` → `RedeemAsync` (any authenticated user/role).
- `GET my-history` → `GetMyHistoryAsync`.
- `GET restaurant/{restaurantId}` `[Authorize(Roles="Admin,Owner")]`.
- `GET {id}` — inherits class-level `[Authorize]` only (no role restriction — any authenticated user can view any redemption by id).
- `POST {id}/complete` `[Authorize(Roles="Owner,Admin")]`.

**RestaurantController** (`EatKath.API/Controllers/RestaurantController.cs`) — **file name says** **`RestaurantController.cs`** **but the class inside is actually named** **`RestaurantsController`** (plural), so with `[Route("api/[controller]")]` the real route is `api/Restaurants`, not `api/Restaurant`.

- `GET`, `GET {id}` `[AllowAnonymous]` — public browsing.
- `GET my` `[Authorize(Roles="Owner")]` → reads NameIdentifier claim directly (not via `ICurrentUserService`) → `GetByOwnerIdAsync`.
- `POST`, `PUT {id}` `[Authorize(Roles="Admin,Owner")]` — no ownership check (any Owner can update any restaurant).
- `DELETE {id}` `[Authorize(Roles="Admin")]` only.
- `POST {id}/logo`, `POST {id}/cover`, `POST {id}/menu-pdf`, `DELETE {id}/logo`, `DELETE {id}/cover`, `DELETE {id}/menu-pdf` — all `[Authorize(Roles="Admin,Owner")]`.

Representative action (`AreasController.Create`, `EatKath.API/Controllers/AreasController.cs:44-51`):

```
[Authorize(Roles = "Admin")]
[HttpPost]
public async Task<IActionResult> Create(CreateAreaDto dto)
{
    var area = await _areaService.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = area.Id }, area);
}

```

## 4. Interfaces / Services

All services live flat in `EatKath.API/Services`, each implementing a same-named interface from `EatKath.API/Interfaces` (except `FileStorageService`, which has no interface and is registered/injected as a concrete class).

| ServiceResponsibility          |                                                                                                                                                                                                                                                                                  |
| ------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `AreaService`                  | CRUD for `Area` lookup table, name-uniqueness check via FluentValidation + duplicate check → `DuplicateEntityException`                                                                                                                                                          |
| `CuisineService`               | Same pattern as Area, for `Cuisine`                                                                                                                                                                                                                                              |
| `DiningTypeService`            | Same pattern as Area, for `DiningType`                                                                                                                                                                                                                                           |
| `UserService`                  | Admin user CRUD; hashes password with `PasswordHasher<User>`; **no FluentValidation validators injected** despite `CreateUserValidator`/`UpdateUserValidator` existing                                                                                                           |
| `AuthService`                  | Register/Login; issues JWT (see below); duplicate-email and inactive-user checks throw plain `Exception` (not `BusinessRuleException`)                                                                                                                                           |
| `CurrentUserService`           | Reads current user identity from `HttpContext` (see detail below)                                                                                                                                                                                                                |
| `MenuCategoryService`          | CRUD + restaurant-ownership authorization performed manually via `ICurrentUserService` (throws `BusinessRuleException` if non-admin and not the restaurant's owner); duplicate name and "category has items" business rule on delete                                             |
| `MenuItemService`              | CRUD + image upload/delete via `FileStorageService`; validators injected and used                                                                                                                                                                                                |
| < `RestaurantService`          | CRUD + logo/cover/menu-PDF upload & delete via `FileStorageService`; **no FluentValidation validators injected** despite validators existing; builds `RestaurantDto` manually (not via AutoMapper) in Get/Create/Update, duplicating a large DTO-construction block 4 times      |
| `RestaurantOpeningHourService` | CRUD, validators used, checks restaurant exists on create                                                                                                                                                                                                                        |
| `RestaurantImageService`       | CRUD + gallery upload (`UploadAsync`), deletes underlying file via `FileStorageService` on delete                                                                                                                                                                                |
| `UserFavoriteService`          | Add/remove favorites scoped to `ICurrentUserService.UserId`; validators used                                                                                                                                                                                                     |
| `OwnerDashboardService`        | Aggregates dashboard metrics (active deals, pending redemptions, today's completed/revenue/customers) scoped to `ICurrentUserService.UserId` as owner id                                                                                                                         |
| `DealService`                  | CRUD for deals; soft-delete (`IsActive=false`) instead of physical delete; **no FluentValidation validators injected** despite `CreateDealValidator`/`UpdateDealDtoValidator` existing; injects `IHttpContextAccessor` but never actually uses it for anything (dead dependency) |
| `RedemptionService`            | Business rules for redeeming a deal (active window, arrival date/time range, guest-count limit), completes redemption with discount math, also flips a matching `Reservation` to `Completed` if found                                                                            |
| `ReservationService`           | Creates reservation + auto-creates a matching `Redemption`; reservation-limit check per deal; status transition methods (confirm/reject/arrive/complete/no-show/cancel) with no ownership verification                                                                           |
| `FileStorageService`           | No interface; extension-whitelist image/PDF saving under `wwwroot`, and file deletion; used by Restaurant/RestaurantImage/MenuItem services                                                                                                                                      |

**CurrentUserService** (`EatKath.API/Services/CurrentUserService.cs`) and **ICurrentUserService** (`EatKath.API/Interfaces/ICurrentUserService.cs`):

```
public interface ICurrentUserService
{
    int UserId { get; }
    string Role { get; }
    bool IsAdmin { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    ...
    public int UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var id) ? id : 0;
        }
    }

    public string Role => _httpContextAccessor.HttpContext?.User?
        .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    public bool IsAdmin => Role == "Admin";
}

```

- Identity comes purely from the current `HttpContext.User` ClaimsPrincipal set up by the JWT bearer handler.
- `UserId` reads `ClaimTypes.NameIdentifier` (mapped from the JWT's `NameIdentifier` claim, set to `user.Id` in `AuthService.GenerateJwtToken`) and parses it as `int`; if missing/unparsable it silently falls back to `0` rather than throwing.
- `Role` reads `ClaimTypes.Role` (set to `user.Role.Name` in the JWT).
- `IsAdmin` is a simple string comparison against the literal `"Admin"`.
- The `UserId` getter also has **debug ****`Console.WriteLine`**** logging of every claim** on every access (lines 22-34) — leftover diagnostic logging, not gated by environment.
- It depends on `IHttpContextAccessor`, registered via `AddHttpContextAccessor()` in `Program.cs`.

**AuthService JWT generation** (`EatKath.API/Services/AuthService.cs:112-151`): builds claims `NameIdentifier` (user id), `JwtRegisteredClaimNames.Email`, `ClaimTypes.Name` (full name), `ClaimTypes.Role` (role name); signs with `HmacSha256` using `JwtSettings:SecretKey`; expiry from `JwtSettings:ExpiryInMinutes`.

## 5. DTOs (`EatKath.API/DTOs`)

Consistent per-domain folder pattern: `Create{X}Dto`, `Update{X}Dto`, `{X}Dto` (response). Deviations/extras:

- **Auth**: `LoginDto`, `RegisterDto`, `AuthResponseDto` (no `Create/Update` naming, and no plain `AuthDto`).
- **Owner**: only `OwnerDashboardDto` (read-only aggregate, no Create/Update — makes sense, it's a dashboard).
- **Redemption**: `CreateRedemptionDto`, `RedemptionDto`, `CompleteRedemptionDto` (an action-specific DTO instead of `UpdateRedemptionDto`).
- **Reservation**: `CreateReservationDto`, `ReservationDto`, plus `OwnerReservationDto` (a specialized read DTO for the owner's reservation list, distinct from `ReservationDto`). No `UpdateReservationDto` (status changes are done via dedicated action endpoints, no body).
- **RestaurantImage**: `CreateRestaurantImageDto`, `UpdateRestaurantImageDto`, `RestaurantImageDto`, plus `UploadRestaurantImageDto` (multipart form: `RestaurantId` + `IFormFile File`).
- **Restaurant**: `CreateRestaurantDto`, `UpdateRestaurantDto`, `RestaurantDto`, plus `UploadRestaurantFileDto` (shared multipart DTO reused for logo/cover/menu-pdf uploads).
- **MenuItem**: `CreateMenuItemDto`, `UpdateMenuItemDto`, `MenuItemDto`, plus `UploadMenuItemImageDto`.
- Validation-attribute usage is inconsistent across DTOs: `Deal`, `Auth`, `Reservation` DTOs use `System.ComponentModel.DataAnnotations` (`[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`) directly on the DTO, which ASP.NET Core's `[ApiController]` model binding auto-validates and returns 400 for. `User` and `Restaurant` Create/Update DTOs have **no attributes at all** and rely entirely on FluentValidation validators that, as noted in §4/§7, are never actually invoked by their services — meaning these two domains currently have effectively **no server-side input validation**.

## 6. Mappings (`EatKath.API/Mappings`)

Three `Profile` classes, all picked up because `AddAutoMapper(typeof(MappingProfile))` scans the whole assembly:

- **MappingProfile.cs** — the bulk of mappings: `User↔UserDto/CreateUserDto/UpdateUserDto`, `MenuItem`, `MenuCategory`, `Area`, `Cuisine`, `DiningType`, `Deal` (with `RestaurantName` flattened from `src.Restaurant.Name`), `RestaurantImage`, `RestaurantOpeningHour`, `UserFavorite` (flattens `RestaurantName`/`LogoUrl` from `src.Restaurant`), `Redemption` (flattens `DealTitle` from `src.Deal.Title`, `CustomerName` concatenated from `src.User.FirstName`/`LastName`), `Reservation`.
- **RestaurantProfile.cs** — `Restaurant→RestaurantDto` (flattens `AreaName` from `src.Area.Name`), `CreateRestaurantDto→Restaurant`, `UpdateRestaurantDto→Restaurant`. Note: `RestaurantService` actually bypasses this profile for `GetAll`/`GetById`/`GetByOwnerId`/`Update` and hand-builds `RestaurantDto` instead (only `CreateAsync` uses `_mapper.Map<RestaurantDto>`), so this profile is only partially exercised.
- **UserProfile.cs** — duplicates/overrides part of `MappingProfile`'s `User→UserDto` mapping, this time flattening `RoleName` from `src.Role.Name` (the `MappingProfile` version does not set `RoleName`). Because AutoMapper merges profiles for the same type pair, the `RoleName` mapping from `UserProfile` effectively wins/adds onto the plain `CreateMap<User,UserDto>()` in `MappingProfile` — a redundant/overlapping definition across two profiles for the same DTO.

## 7. Validators (`EatKath.API/Validators`) and gaps

Validators present (all `AbstractValidator<T>`, all registered automatically via assembly scan):

- `Area`: Create/Update — name required + max length.
- `Cuisine`: Create/Update — (not printed in full but present, same pattern).
- `DiningType`: Create/Update.
- `Restaurant`: `CreateRestaurantValidator`/(`UpdateRestaurantValidator` exists) — validates OwnerId, Name, Address, AreaId, Phone, Email, Website, LogoUrl. **Not injected/used by ****`RestaurantService`****.**
- `User`: `CreateUserValidator`/`UpdateUserValidator` — validates names, email, password length, phone, RoleId. **Not injected/used by ****`UserService`****.**
- `MenuCategory`: Create/Update — used by `MenuCategoryService`.
- MenuItem validators (`CreateMenuItemValidator`, `UpdateMenuItemValidator`) are physically located under `Validators/MenuCategory/` (mis-filed) rather than a `Validators/MenuItem/` folder (that folder is declared empty in the `.csproj` but never populated) — used by `MenuItemService`.
- `RestaurantImage`: Create/Update — used by `RestaurantImageService`.
- `RestaurantOpeningHour`: Create/Update — used by `RestaurantOpeningHourService`. Validates `CloseTime > OpenTime` only `.When(!IsClosed)`.
- `UserFavorite`: Create/Remove — used by `UserFavoriteService`.
- `Redemption`: `CreateRedemptionValidator` only — used by `RedemptionService`. **`CompleteRedemptionDto`**** has no validator** and `CompleteRedemptionAsync` performs no upfront validation of `BillAmount`.
- `Deal`: `CreateDealValidator`/`UpdateDealDtoValidator` — thorough (title, description, discount range, offer type enum, date/time ordering, guest/redemption limits, url/terms length). **Not injected/used by ****`DealService`** — dead code from a validation standpoint; only the DataAnnotations on `CreateDealDto` (Required/MaxLength/Range) provide any actual enforcement, and `UpdateDealDto` isn't shown to carry the same annotations reliably.

**DTOs with no FluentValidation validator at all**: `Auth` (`LoginDto`, `RegisterDto` — DataAnnotations only), `Reservation` (`CreateReservationDto` — DataAnnotations only, no validator folder exists), `Redemption`'s `CompleteRedemptionDto`, `Owner` (read-only, expected).

**Validators that exist but are never wired into their service** (registered in DI but effectively dead): `CreateRestaurantValidator`/`UpdateRestaurantValidator` (Restaurant), `CreateUserValidator`/`UpdateUserValidator` (User), `CreateDealValidator`/`UpdateDealDtoValidator` (Deal).

## 8. Exception handling

`EatKath.API/Middleware/ExceptionMiddleware.cs` wraps `_next(context)` in try/catch and maps exceptions to JSON `{ StatusCode, Message }`:

- `FluentValidation.ValidationException` → 400, message = all `ErrorMessage`s joined with `" | "`.
- `EatKath.API.Exceptions.DuplicateEntityException` → 409 Conflict, `ex.Message`.
- `EatKath.API.Exceptions.BusinessRuleException` → 400 BadRequest, `ex.Message`.
- Anything else (including plain `Exception`, which `AuthService`, `UserService`, `RestaurantService`, `MenuItemService`, `RedemptionService`, `ReservationService`, `FileStorageService` all throw liberally for things like "not found" or "already exists") → falls through to 500 Internal Server Error with the generic message `"An unexpected error occurred."` (the real message is only logged via `ILogger`, not returned to the client) — meaning most domain "not found"/"duplicate"/"invalid" errors that aren't `Duplicate/BusinessRuleException` surface to callers as opaque 500s rather than 400/404/409.
- `Exceptions/BusinessRuleException.cs` and `Exceptions/DuplicateEntityException.cs` are simple `Exception` subclasses with only a message constructor, no additional data/status metadata.
- Reminder: because of pipeline placement (§2), this middleware only guards `UseStaticFiles`/`MapControllers`, not `UseAuthentication`/`UseAuthorization`.

## 9. Configurations / Extensions / Helpers

All three folders (`EatKath.API/Configurations`, `EatKath.API/Extensions`, `EatKath.API/Helpers`) exist on disk but are **completely empty** — no `.cs` files in any of them. They are declared as placeholder `<Folder Include>` entries in the `.csproj` (§1) but contain no actual code; all configuration/DI wiring lives directly in `Program.cs`.

## 10. appsettings.json / appsettings.Development.json

`EatKath.API/appsettings.json`:

- `ConnectionStrings:DefaultConnection` — SQL Server LocalDB connection string (`Server=(localdb)\MSSQLLocalDB;Database=EatKathDB;...TrustServerCertificate=True`). This looks like a local-dev-only value committed directly (no user-secrets/environment-variable indirection shown here), i.e. non-sensitive for LocalDB but the same block is presumably overridden per-environment for real deployments (no such override file is present in this repo though).
- `JwtSettings` — object with `SecretKey`, `Issuer` (`"EatKath.API"`), `Audience` (`"EatKath.Web"`), `ExpiryInMinutes` (60). The `SecretKey` value present in the file is an obviously-labelled placeholder/dev-only key (contains "ReplaceThisWithA256Bit..." wording) rather than a real secret — but it is still committed in plaintext in source control as the value actually used for JWT signing.
- `Logging:LogLevel` — Default `Information`, `Microsoft.AspNetCore` `Warning`.
- `AllowedHosts`: `"*"`.

`EatKath.API/appsettings.Development.json` — only contains a `Logging` section (same as above); no overrides for connection string or JWT settings, so Development uses the same values as the base `appsettings.json`.

## 11. Repositories folder

`EatKath.API/Repositories` exists (declared in the `.csproj` as a `<Folder Include>`) but **contains zero files** — it is entirely vestigial. There is no repository pattern anywhere in the codebase; every service (`AreaService`, `UserService`, `RestaurantService`, etc.) injects `ApplicationDbContext` directly and queries `DbSet<T>`s inline (e.g. `_context.Areas.FindAsync(id)`), so `Interfaces`/`Services` act as a thin service layer directly over EF Core with no repository/unit-of-work abstraction.

---

### Cross-cutting observations worth flagging

- **Namespace split**: interfaces/services for `Auth`, `User`, `Restaurant`, `RestaurantImage`, `MenuItem` live under namespace `EatKath.API.Services.Interfaces`, while the rest (`Area`, `Cuisine`, `DiningType`, `Deal`, `MenuCategory`, `RestaurantOpeningHour`, `UserFavorite`, `Redemption`, `Reservation`, `OwnerDashboard`) use `EatKath.API.Interfaces` — both sets physically live in the same `Interfaces` folder, but the namespace is inconsistent, forcing controllers to mix `using EatKath.API.Interfaces;` and `using EatKath.API.Services.Interfaces;`.
- **RestaurantController class name mismatch**: file `Controllers/RestaurantController.cs` declares `class RestaurantsController` (plural), so the actual route is `api/Restaurants`, not `api/Restaurant`.
- **Validators registered but unused**: `Deal`, `Restaurant`, and `User` domains have full FluentValidation validator classes wired into DI, but their services never inject/call them — those two domains (`User`, `Restaurant`) also lack DataAnnotations on their Create/Update DTOs, so they currently have no effective server-side validation at all.
- **Debug ****`Console.WriteLine`**** logging** left in production code paths: `Program.cs` startup banner, `CurrentUserService.UserId` (dumps every JWT claim on every access), `MenuCategoryService.UpdateAsync`, `DealService.CreateAsync`.
- **Authorization bypass pattern**: `MenuCategoryController` and `UploadsController` have no `[Authorize]` at all; `MenuCategoryController` instead relies on manual ownership checks deep inside `MenuCategoryService`, while `UploadsController` has no auth or ownership checks whatsoever.

All four research agents have completed. Here's the full architecture report for EatKath, based only on what actually exists in the code.

---

# EatKath — Architecture Report

## 1. Overall Architecture

```
React 19 + TS SPA (EatKath.Web, Vite)
        ↓  axios (Bearer JWT)
ASP.NET Core 8 Web API (EatKath.API)
        ↓
Controllers (16, api/[controller] routes)
        ↓
Service layer (Interfaces/ + Services/, injected via DI)
        ↓
ApplicationDbContext (EF Core 8, DbContext injected directly — no repository layer)
        ↓
SQL Server (LocalDB in dev, containerized SQL Server 2022 in Docker)

```

Solution structure: `EatKath.sln` contains only **`EatKath.API`** and **`EatKath.API.Tests`** — the frontend `EatKath.Web` is a fully separate Vite project, not part of the .NET solution, connected purely over HTTP.

There is **no repository/unit-of-work layer** — `EatKath.API/Repositories` exists as an empty folder. Services inject `ApplicationDbContext` directly and query `DbSet<T>`s inline. There's also no MediatR/CQRS — it's a classic Controller → Service → DbContext three-tier design.

## 2. Frontend Architecture (`EatKath.Web`)

**Stack**: React `19.2.7`, TypeScript `~6.0.2`, Vite `8.1.1`, MUI `7.3.11` (+Emotion), react-router-dom `7.18.2`, axios `1.19.0`, react-hook-form `7.84.0`.

**Installed but unused**: `zod`, `@hookform/resolvers`, `@tanstack/react-query`, `notistack` are all in `package.json` but never actually invoked anywhere in `src` — no `zodResolver`, no `useQuery`/`QueryClientProvider`, no `SnackbarProvider`. The app looks scaffolded for these patterns but currently does plain manual data-fetching/validation instead.

**Folder structure**: `api/` (just `axios.ts`), `services/` (14 per-domain classes, this is where actual endpoint calls live), `pages/` (14 route-level pages), `components/{deals,reservations,restaurants}/`, `features/auth/` (self-contained: context, provider, login page, route guard), `layouts/MainLayout.tsx`, `routes/AppRoutes.tsx`, `types/` (23 files mirroring backend DTOs), `theme/`, `utils/imageUrl.ts`.

**No** **`hooks/`** **folder exists** — zero custom hooks anywhere; all state is `useState`/`useEffect` per page.

**Routing** (`src/routes/AppRoutes.tsx`, react-router v7, classic `<Routes>/<Route>`, not the data-router API): public pages (`/`, `/restaurants`, `/restaurants/:id`, `/login`), Customer-only (`/favorites`, `/my-reservations`), Owner-only (`/owner`, `/owner/deals`, `/owner/deals/new`, `/owner/deals/edit/:id`, `/owner/restaurant`, `/owner/opening-hours`, `/owner/menu-items`, `/owner/menu-categories`), Admin-only (`/admin`, currently a stub — just a heading, no functionality).

**Auth handling**: `AuthContext`/`AuthProvider` (`src/features/auth/`) hold `{user, login, logout}`; the whole JWT+user payload is stored as JSON in `localStorage["user"]`. `src/api/axios.ts` has one shared axios instance whose **request interceptor** reads `localStorage` and attaches `Authorization: Bearer <token>`. There is **no response interceptor** — no centralized 401 handling/auto-logout/refresh. `ProtectedRoute.tsx` does a single-string role comparison, redirecting unauthenticated users to `/login` and wrong-role users to `/`.

**Forms/validation**: react-hook-form is used in exactly one place (`LoginPage.tsx`), with no validation rules attached. Every other form (`CreateDealPage`, `EditDealPage`, `ReservationDialog`, owner CRUD pages) is a hand-rolled controlled-component form with raw `useState` — no client-side validation library in actual use.

**Error/loading handling**: consistent but unpolished pattern — `loading` boolean gates a `CircularProgress`; errors are `console.error` + `alert()` (including one spot that `alert()`s a raw JSON-stringified API error body). No toasts, no error boundary.

## 3. Backend Architecture (`EatKath.API`)

**.NET 8** (`net8.0`), ASP.NET Core Web API. Key packages: AutoMapper, FluentValidation (+DI extensions), `Microsoft.AspNetCore.Authentication.JwtBearer`, EF Core 8 + SqlServer + Design (EF Tools is oddly pinned to `10.0.10` — a version mismatch), Swashbuckle (Swagger), `BCrypt.Net-Next` (referenced but **unused** — actual hashing uses ASP.NET Identity's `PasswordHasher<User>`).

**Program.cs pipeline** (order matters — see §11 for the bug this creates):

```
UseSwagger/UseSwaggerUI (unconditional, not just Development)
UseHttpsRedirection
UseCors("ReactPolicy")     // localhost:5173 + one Azure Static Web App origin
UseAuthentication
UseAuthorization
UseMiddleware<ExceptionMiddleware>()   ← placed AFTER auth
UseStaticFiles
MapControllers

```

DB is migrated + seeded at startup with a 10-attempt/5s retry loop, tolerating a not-yet-ready SQL Server container.

**DI**: every domain service registered `Scoped` against its interface; `FileStorageService` has no interface and is injected as a concrete class; `Repositories/`, `Configurations/`, `Extensions/`, `Helpers/` folders all exist but are **completely empty**.

**16 controllers**, thin — each delegates to one injected service. Request flow example: `AreasController.Create` → `IAreaService.CreateAsync` (validates via FluentValidation, checks name uniqueness, throws `DuplicateEntityException` on conflict) → `ApplicationDbContext.Areas.Add` + `SaveChangesAsync` → mapped to `AreaDto` via AutoMapper → 201 Created.

**AutoMapper**: 3 profiles (`MappingProfile`, `RestaurantProfile`, `UserProfile`) all auto-discovered via assembly scan; some overlap/duplication exists between `MappingProfile`'s and `UserProfile`'s `User→UserDto` mapping.

**FluentValidation**: validators exist per-domain but are **inconsistently wired up** — `Deal`, `User`, and `Restaurant` domains have full validator classes registered in DI that their own services never actually call, meaning those three domains currently have little-to-no effective server-side validation (Restaurant/User DTOs also lack DataAnnotations as a backstop).

**Swagger**: enabled unconditionally (not gated to Development), with a Bearer JWT auth definition wired in.

**Current-user handling**: `ICurrentUserService`/`CurrentUserService` reads `ClaimTypes.NameIdentifier` and `ClaimTypes.Role` off `HttpContext.User` via `IHttpContextAccessor`. Several controllers (Deal, Restaurant, Reservation) bypass this service and read `ClaimTypes.NameIdentifier` directly from `User` in the controller instead — an inconsistent pattern.

## 4. Database / EF Core

**Provider**: SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`), LocalDB in dev (`(localdb)\MSSQLLocalDB`), containerized SQL Server 2022 in Docker.

**Actual entities found** (confirming/correcting the user's list): `Area`, `Cuisine`, `DiningType`, `Role`, `User`, `Restaurant`, `RestaurantImage`, `RestaurantOpeningHour`, `RestaurantCuisine` (join), `RestaurantDiningType` (join), `MenuCategory`, `MenuItem`, `Deal`, `Redemption`, `UserFavorite` (join), `Reservation`. **No entity literally named "MenuCategory items list" or extra ones beyond this** — every entity the user asked about exists except there is no separate "Owner" entity (ownership is `Restaurant.OwnerId → User`) and no separate "Customer" entity (Customer is just a `Role`).

**Relationships**:

- One-to-many: Role→User, Area→Restaurant, User(Owner)→Restaurant, Restaurant→RestaurantImage/OpeningHour/MenuCategory/Deal, MenuCategory→MenuItem, Deal→Redemption, Deal→Reservation, User→Redemption/Reservation.
- Many-to-many via explicit join entities: Restaurant↔Cuisine (`RestaurantCuisine`), Restaurant↔DiningType (`RestaurantDiningType`), User↔Restaurant (`UserFavorite`, i.e. favorites).
- `MenuItem` has a denormalized second FK direct to `Restaurant` (in addition to via `MenuCategory`), forcing `DeleteBehavior.NoAction` to avoid SQL Server's multiple-cascade-path error.
- Only **one unique index in the entire schema**: `Area.Name`. Notably no unique index on `User.Email`.

**Migrations**: 11 migrations from `InitialCreate` (2026-07-25) through `AddReservationLimit` (2026-08-04), telling a clear evolution story: initial schema → RestaurantImage rework → Deal/Redemption model refactored twice (from simple price fields to a full discount/offer-type/time-window model) → decimal precision cleanup (empty migration, model-only) → restaurant/menu image additions → Reservations added and linked to User → reservation limits added.

**Seed data**: extensive — `Data/Seeders/DatabaseSeeder.cs` orchestrates 16 seeders run from `Program.cs` at startup (idempotent, checked via `.Any()`): roles, 1 admin + 4 owners + 5 customers (all password `Password77`), 20 areas, 20 cuisines, 10 dining types, 50 demo restaurants across 2 owners, opening hours, cuisine/dining-type tagging, 2 deals/restaurant, menu categories/items, gallery images, hardcoded favorites, and 5 hardcoded redemption records. **No seeder for** **`Reservation`** — that table starts empty.

## 5. Authentication and Authorization

**Login/Register**: `AuthController` has exactly two endpoints, `POST api/Auth/register` and `POST api/Auth/login`, both `[AllowAnonymous]`. **No refresh-token, logout, or password-reset endpoints exist anywhere.**

**Password hashing**: ASP.NET Identity's `PasswordHasher<User>`, used consistently by both `AuthService` and `UserSeeder` (confirmed by direct read — the referenced `BCrypt.Net-Next` package is unused dead weight).

**JWT generation** (`AuthService.GenerateJwtToken`): HMAC-SHA256, signing key/issuer/audience/expiry from config section `JwtSettings` (in `appsettings.json` — Issuer `"EatKath.API"`, Audience `"EatKath.Web"`, 60-minute expiry). Claims: `NameIdentifier` (user id), `email`, `Name` (full name), `Role` (single role name string).

**Roles**: a real `Role` table (`Admin`/`Owner`/`Customer`), FK'd from `User.RoleId` — but the JWT only carries the role *name* as a string claim, so authorization checks behave as simple string comparisons regardless of the normalized table.

**Authorization**: no custom policies — `AddAuthorization()` is called bare; everything is `[Authorize(Roles="...")]` attributes, class-level or per-action. Pattern: lookup data (Area/Cuisine/DiningType) public-read/Admin-write; restaurant-owned resources (Deal, MenuItem, RestaurantImage, OpeningHour, Restaurant) Admin+Owner; Reservation is finely split by role; **`MenuCategoryController`** **and** **`UploadsController`** **have no** **`[Authorize]`** **at all**.

**Current-user identification**: `CurrentUserService` reads `ClaimTypes.NameIdentifier`/`ClaimTypes.Role` off `HttpContext.User`, with a leftover debug `Console.WriteLine` dumping every claim on every access.

**Frontend**: JWT + user payload stored as one JSON blob in `localStorage["user"]`; attached via axios request interceptor; `ProtectedRoute` does client-side role gating.

## 6. Business Domain

The core model is an **EatClub-style time-boxed deal + redemption workflow** (confirmed directly by the developer's own `ProjectNotes.md.txt`):

- A **Restaurant** (owned by a `User` with role Owner) belongs to one **Area**, has many **Cuisines**/**DiningTypes** (tags), **MenuCategories** → **MenuItems**, **OpeningHours**, and **Images**.
- A **Deal** belongs to a Restaurant: has a discount %, `OfferType` (DineIn/Takeaway/Delivery), a date range + daily time window, max guests, and optional reservation/daily-redemption limits.
- A **Customer** either **reserves** a Deal (`Reservation` — arrival date/time, guest count, status string Pending→Confirmed→Arrived→Completed/NoShow/Cancelled/Rejected, with an auto-generated confirmation code) or directly **redeems** it (`Redemption` — arrival date/time, guest count, status enum Redeemed→Completed/Cancelled/Expired). Interestingly, `ReservationService.Create` also auto-creates a matching `Redemption`, and completing a `Redemption` will flip a matching `Reservation` to Completed too — the two workflows are linked in code even though they're structurally parallel/duplicated concepts.
- Discount math (bill amount → discount amount → final amount) is calculated **server-side** at redemption-completion time, never trusted from the client (an explicit design decision per the project notes).
- **Favorites** (`UserFavorite`) let a Customer bookmark a Restaurant.
- **Admin** role exists (manages Areas/Cuisines/DiningTypes/Users, oversees all data) but the admin frontend page is a stub with no real functionality yet.

## 7. API (grouped by domain)

| DomainMethod → RouteAuthService                                                                                      |                                                                                                                                                                               |                                                                              |                                 |
| -------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- | ------------------------------- |
| **Auth**                                                                                                             | POST `/api/Auth/register`, `/login`                                                                                                                                           | AllowAnonymous                                                               | `AuthService`                   |
| **Areas**                                                                                                            | GET (list/id) / POST / PUT / DELETE `/api/Areas`                                                                                                                              | public read, Admin write                                                     | `AreaService`                   |
| **Cuisine**                                                                                                          | same CRUD shape `/api/Cuisine`                                                                                                                                                | public read, Admin write                                                     | `CuisineService`                |
| **DiningType**                                                                                                       | same CRUD shape `/api/DiningType`                                                                                                                                             | public read, Admin write                                                     | `DiningTypeService`             |
| **Restaurants** (route is `api/Restaurants`, plural — class name mismatches the file name `RestaurantController.cs`) | GET list/id (public), GET `my` (Owner), POST/PUT (Admin,Owner), DELETE (Admin), logo/cover/menu-pdf upload+delete (Admin,Owner)                                               | mixed                                                                        | `RestaurantService`             |
| **Deal**                                                                                                             | GET list/id/restaurant/{id} (public), GET `my` (Owner), POST (Owner), PUT/DELETE (Admin,Owner)                                                                                | mixed                                                                        | `DealService`                   |
| **MenuCategory**                                                                                                     | GET/POST/PUT/DELETE `/api/MenuCategory`                                                                                                                                       | **no ****`[Authorize]`**** — ownership enforced inside the service instead** | `MenuCategoryService`           |
| **MenuItem**                                                                                                         | GET (public), POST/PUT/DELETE/image ops (Admin,Owner)                                                                                                                         | mixed                                                                        | `MenuItemService`               |
| **RestaurantOpeningHour**                                                                                            | GET (public), POST/PUT/DELETE (Admin,Owner)                                                                                                                                   | mixed                                                                        | `RestaurantOpeningHourService`  |
| **RestaurantImage**                                                                                                  | GET (public), POST/PUT/upload/DELETE (Admin,Owner)                                                                                                                            | mixed                                                                        | `RestaurantImageService`        |
| **Uploads**                                                                                                          | POST `/api/Uploads/image`                                                                                                                                                     | **none — fully anonymous, no ownership/extension whitelist**                 | direct file write in controller |
| **UserFavorite**                                                                                                     | GET/POST/DELETE `/api/UserFavorite`                                                                                                                                           | any authenticated user                                                       | `UserFavoriteService`           |
| **User**                                                                                                             | full CRUD `/api/User`                                                                                                                                                         | Admin-only (class-level)                                                     | `UserService`                   |
| **Owner**                                                                                                            | GET `/api/Owner/dashboard`                                                                                                                                                    | Owner                                                                        | `OwnerDashboardService`         |
| **Redemption**                                                                                                       | POST (redeem, any authenticated), GET `my-history`, GET `restaurant/{id}` (Admin,Owner), GET `{id}` (any authenticated), POST `{id}/complete` (Owner,Admin)                   | mixed                                                                        | `RedemptionService`             |
| **Reservation**                                                                                                      | GET all (Admin), GET `owner` (Owner), GET `my` (Customer), GET `{id}` (**unrestricted**), POST (Customer), PUT status transitions (Owner, no ownership check), DELETE (Admin) | mixed                                                                        | `ReservationService`            |

## 8. Tests (`EatKath.API.Tests`)

**Framework**: MSTest 3.6.4 + Moq (mocking) + FluentAssertions (assertions) + EF Core InMemory (DB isolation, fresh `Guid`-named DB per test via `TestDbContextFactory`). `MapperFactory` builds a real `MapperConfiguration` from the actual `MappingProfile`, so mapping bugs are caught.

**Only 3 test classes exist, all under** **`Services/`**:

- `AreaServiceTests` (3 tests) — `CreateAsync` only (success, duplicate name, validation failure).
- `DealServiceTests` (2 tests) — `CreateAsync` only (success, restaurant-not-found).
- `RedemptionServiceTests` (10 tests) — the most thorough: full coverage of `RedeemAsync` edge cases (offer not found/inactive, date/time window, restaurant inactive, guest count exceeded, validation failure) and `CompleteRedemptionAsync` (success with discount math verification, not-found, double-completion guard).

**`Controllers/`****,** **`Validators/`****,** **`TestData/`** **folders exist but are entirely empty** — declared in the `.csproj` as placeholders, never populated.

**Zero test coverage**: `AuthService`/`AuthController` (including JWT generation and login/register — the most security-sensitive path), every other service (`Cuisine`, `DiningType`, `MenuCategory`, `MenuItem`, `OwnerDashboard`, `Reservation`, `RestaurantImage`, `RestaurantOpeningHour`, `Restaurant`, `UserFavorite`, `User`), `CurrentUserService`, `FileStorageService`, every controller, every validator.

## 9. Docker / Infrastructure

`docker-compose.yml` (heavily commented, clearly written as a learning artifact) defines two services:

- **`sqlserver`**: `mcr.microsoft.com/mssql/server:2022-latest`, port `1433:1433`, password from `${SA_PASSWORD}` env var (`.env` file), persistent named volume `sql_data:/var/opt/mssql`, healthcheck via `sqlcmd SELECT 1` (10s interval, 10 retries, 30s start period).
- **`api`**: built from `EatKath.API/Dockerfile` (multi-stage: `dotnet/sdk:8.0` build → `dotnet/aspnet:8.0` runtime), port `8080:8080`, `ASPNETCORE_ENVIRONMENT=Development` (so Swagger/dev features stay on even in the container), connection string injected via `ConnectionStrings__DefaultConnection` env var pointing at the `sqlserver` service name, `depends_on: sqlserver: condition: service_healthy` so the API waits for the DB healthcheck before starting.

A separate `azure-pipelines.yml` (Azure DevOps, `windows-latest` pool) does Build → Test (`**/*Tests.csproj`) → Publish → Deploy to an Azure Web App (`eatkath-api`) on push to `main` — this is the actual CI/CD path, distinct from the Docker Compose local-dev setup.

## 10. Project Maturity

| AreaMaturityReasoning |                             |                                                                                                                                                                                                                                            |
| --------------------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Frontend              | **Partially implemented**   | Full page/routing coverage for Customer and Owner flows, but Admin page is a stub, several installed libraries (React Query, Zod, notistack) unused, no hooks abstraction, ad hoc error handling                                           |
| Backend/API           | **Partially implemented**   | Broad endpoint coverage across all domains, but validation is inconsistently wired (Deal/User/Restaurant validators registered but unused), inconsistent auth attribute coverage (Uploads/MenuCategory unprotected), debug logging left in |
| Database              | **Mostly complete**         | Rich, evolved schema (11 migrations), full seed data, but only one unique index in the whole DB, some FK/cascade inconsistencies, no Reservation seed data                                                                                 |
| Authentication        | **Partially implemented**   | Login/register + JWT works and is used consistently across the app, but no refresh token, no logout endpoint, no password reset, debug claim-logging left in                                                                               |
| Admin                 | **Placeholder**             | Backend has Admin-gated CRUD on lookup data and Users; frontend `AdminDashboardPage` is literally just a heading                                                                                                                           |
| Restaurant Owner      | **Mostly complete**         | Dashboard, deals, menu, opening hours, images, reservations management all implemented on both frontend and backend                                                                                                                        |
| Customer              | **Mostly complete**         | Browse restaurants/deals, favorites, reservations, redemption history all implemented                                                                                                                                                      |
| Deals                 | **Mostly complete**         | Full deal lifecycle modeled and iterated across multiple migrations, core business rules (date/time window, guest limits) enforced and unit-tested                                                                                         |
| Menus                 | **Mostly complete**         | Categories/items CRUD with images implemented, though MenuCategoryController has no auth attribute                                                                                                                                         |
| Testing               | **Placeholder/early-stage** | 15 tests total across 3 of 15+ services; zero controller/validator/auth tests                                                                                                                                                              |
| Infrastructure        | **Mostly complete**         | Working Docker Compose (API+DB, healthcheck, volume) and a real Azure DevOps CI/CD pipeline exist and appear functional                                                                                                                    |

## 11. Architecture Concerns

**Definite problems:**

- `RestaurantController.cs` file declares class `RestaurantsController` (plural) — actual route is `api/Restaurants`, not `api/Restaurant` as the filename implies.
- `MenuCategoryController` and `UploadsController` have **no ****`[Authorize]`**** attribute at all** — `Uploads` accepts anonymous file uploads with no extension/content-type whitelist, writing directly to `wwwroot`.
- FluentValidation validators for **Deal, User, and Restaurant** are registered in DI but never injected/called by their services — those DTOs (especially `User`/`Restaurant` Create/Update, which also lack DataAnnotations) currently have **no effective server-side validation**.
- `ExceptionMiddleware` is registered **after** `UseAuthentication`/`UseAuthorization` in `Program.cs`, so it cannot catch exceptions thrown during authentication/authorization.
- Generic `Exception` (not `BusinessRuleException`/`DuplicateEntityException`) is thrown throughout `AuthService`, `UserService`, `RestaurantService`, `MenuItemService`, `RedemptionService`, `ReservationService` for things like "not found"/"already exists" — these fall through to a generic 500 response instead of a proper 400/404/409, hiding real client errors as server errors.
- Debug `Console.WriteLine` logging left in `CurrentUserService.UserId` (dumps every JWT claim on every request), `Program.cs` startup, `MenuCategoryService.UpdateAsync`, `DealService.CreateAsync`.
- No ownership verification on several Owner-role endpoints: `Reservation` status-transition endpoints, `Restaurant` PUT, `RestaurantOpeningHour` POST/PUT/DELETE — any user with the Owner role can act on **any** restaurant's data, not just their own.
- `GET /api/Reservation/{id}` and `GET /api/Redemption/{id}` have no role/ownership restriction beyond bare authentication (Redemption) or none at all (Reservation) — any user can view any other user's reservation/redemption by guessing an id.
- Swagger UI is enabled unconditionally in all environments (including the containerized "Development" deployment), not gated behind an environment check as a commented-out block in `Program.cs` suggests was originally intended.
- Only one unique DB index in the entire schema (`Area.Name`) — no unique constraint on `User.Email`, meaning duplicate-email prevention is enforced only in application code (`AuthService`), which is a race-condition risk under concurrent registration.
- `src/components/deals/DealFormDialog.tsx` (frontend) is a 0-byte empty file.
- Two `/owner/menu-items` and `/owner/menu-categories` frontend routes are declared as siblings of, not nested inside, the `MainLayout` route — they render without the site navbar.
- Zero automated test coverage for `AuthService`/`AuthController` — the single most security-critical code path in the app.

**Worth reviewing, not necessarily bugs:**

- `MenuItem` carries a denormalized direct FK to `Restaurant` in addition to via `MenuCategory`, forcing `DeleteBehavior.NoAction` to dodge SQL Server's cascade-path restriction — workable but adds coupling/redundancy.
- `Reservation` and `Redemption` are two structurally parallel but differently-modeled workflows (string-const status vs. int enum status) that are cross-linked in service code (creating a Reservation also creates a Redemption; completing a Redemption can complete a matching Reservation) — this dual-workflow design is unusual and worth understanding fully before extending it.
- Several installed frontend dependencies (`@tanstack/react-query`, `zod`, `@hookform/resolvers`, `notistack`) and one backend package (`BCrypt.Net-Next`) are unused — dead weight, or evidence of an in-progress/abandoned migration.
- `RestaurantService` hand-builds `RestaurantDto` manually in 3 of 4 methods instead of using the `RestaurantProfile` AutoMapper profile that exists for exactly this purpose — duplicated mapping logic.
- Namespace inconsistency: some service interfaces live in `EatKath.API.Interfaces`, others in `EatKath.API.Services.Interfaces`, despite all physically sitting in the same folder.
- `EatKath.API.csproj` pins `Microsoft.EntityFrameworkCore.Tools` to `10.0.10` while every other EF package is `8.0.29` — likely an unintentional floating-version install.
- No refresh-token/logout/password-reset flow exists — sessions are single 60-minute JWTs with no revocation mechanism.

## 12. EatKath Architecture Map

```
┌─────────────────────────────── EatKath.Web (React 19 + TS, Vite) ───────────────────────────────┐
│  pages/ (HomePage, RestaurantsPage, OwnerDashboard*, AdminDashboardPage(stub), ...)               │
│       ↓ uses                                                                                      │
│  services/*.ts (AuthService, RestaurantService, DealService, RedemptionService, ...)              │
│       ↓ calls through                                                                             │
│  api/axios.ts  ──(Bearer JWT via request interceptor, token from localStorage["user"])──►         │
│  features/auth/ (AuthContext, AuthProvider, ProtectedRoute — role string match)                   │
└──────────────────────────────────────────┬────────────────────────────────────────────────────────┘
                                            │ HTTPS / JSON
┌───────────────────────────────────────────▼──────────────────────────────────────────────────────┐
│                                  EatKath.API (ASP.NET Core 8)                                     │
│  Program.cs: CORS → JWT Auth → Authorization → ExceptionMiddleware → StaticFiles → Controllers    │
│                                                                                                     │
│  Controllers (16, [Authorize(Roles=...)] per action) ── AutoMapper (entity↔DTO) ── FluentValidation │
│       ↓ injects                                                                                    │
│  Services (Interfaces/ + Services/, Scoped DI) — business rules, ICurrentUserService for identity  │
│       ↓ injects ApplicationDbContext directly (no repository layer)                                │
│  EF Core 8 (ApplicationDbContext) — DbSets, fluent config, 11 migrations                           │
└──────────────────────────────────────────┬────────────────────────────────────────────────────────┘
                                            ▼
                          SQL Server (LocalDB dev / SQL Server 2022 container)
                          EatKathDB — seeded via DatabaseSeeder at startup

Business domain graph:
  Area ──┐                                        Role ──► User ──► Restaurant (Owner)
  Cuisine ┼──*  Restaurant ──┬── MenuCategory ──► MenuItem       │       │
  DiningType ┘               ├── OpeningHour                    │       ├── Deal ──┬──► Redemption (server-calculated discount)
                              ├── RestaurantImage                │       │          └──► Reservation (status workflow)
                              └── Deal ◄──────────────────────────┘       └── UserFavorite ◄── User

Auth: AuthController → AuthService (PasswordHasher<User>, JWT: NameIdentifier/Email/Name/Role claims,
      HMAC-SHA256, 60min expiry) → CurrentUserService (reads HttpContext.User claims per-request)

```

That covers all 12 requested sections. No files were modified, created, or deleted, and no build/DB commands were run — this was pure read-only exploration across `EatKath.API`, `EatKath.Web`, and `EatKath.API.Tests`.

---

## 📌 Document note

This document is a formatted version of the complete Claude Code exploration report.
The technical findings are preserved from the supplied source report. Items identified
as bugs, gaps, or concerns should be verified against the current code before changes
are made.
