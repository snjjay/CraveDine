---
name: trace-cross-stack-field
description: Investigate whether a specific field or entity property is correctly connected end-to-end across the EatKath stack (React/TypeScript frontend -> ASP.NET Core API -> EF Core -> SQL Server). Use when asked to trace a field, debug why a value "disappears" or "resets", check a property before modifying it, or audit a feature area for missing/inconsistent mappings. Read-only investigation only - never modifies files.
---

# Trace Cross-Stack Field (EatKath)

## Purpose

EatKath has no generated/shared contract between its hand-written TypeScript
types and its hand-written C# DTOs. A field can exist on the EF Core entity
and be silently missing from a DTO, a mapped type, or a form — and nothing
will fail at compile time or show an error at runtime. It will just quietly
lose data (this exact pattern caused the `Deal.ReservationLimit` bug: the
entity and `CreateDealDto` had it, but `DealDto` and the frontend `UpdateDeal`
type did not, so editing a Deal silently reset the value to `0`).

This Skill packages the investigation method used to find that bug into a
repeatable checklist, so the same rigor is applied every time a field is
traced, instead of being reconstructed ad hoc.

## Hard rules

- **Read-only.** Use `Read`, `Grep`, `Glob`, and read-only `Bash` (`find`,
  `git diff`, `git log`, `dotnet build`/`test` for verification only) —
  never `Edit`, `Write`, or any command that changes files, the database, or
  git state.
- **Do not implement fixes.** Propose them; do not apply them, even if the
  fix looks trivial or obvious.
- **Report what actually exists in the code**, not what you'd expect a
  well-designed app to have. If a layer is missing the field, say so
  plainly — don't assume convention will save you.
- If the user's request for this skill implies they want the fix applied,
  stop and confirm scope before writing any code — this skill's job ends at
  the report.

## Inputs

This skill expects a **field name** (e.g. `ReservationLimit` /
`reservationLimit`) and/or an **entity/domain name** (e.g. `Deal`,
`MenuItem`, `RestaurantOpeningHour`). If only one is given, infer the other
from context (ask the user if it's ambiguous — e.g. the field name exists on
multiple entities).

## EatKath conventions (use these to locate files quickly)

Backend (`EatKath.API/`), per domain `<X>`:
- Entity: `Entities/<X>.cs`
- DTOs: `DTOs/<X>/<X>Dto.cs`, `Create<X>Dto.cs`, `Update<X>Dto.cs` (some
  domains add action-specific DTOs, e.g. `CompleteRedemptionDto`,
  `UploadRestaurantFileDto` — check the folder, don't assume only these three)
- Controller: `Controllers/<X>Controller.cs` (routes are `api/[controller]`;
  **verify the actual class name inside the file** — `RestaurantController.cs`
  actually declares `RestaurantsController`, so the route is `api/Restaurants`)
- Service + interface: `Services/<X>Service.cs`, `Interfaces/I<X>Service.cs`
- AutoMapper: `Mappings/MappingProfile.cs` is where most `CreateMap<X, XDto>()`
  calls live, but also check `Mappings/RestaurantProfile.cs` and
  `Mappings/UserProfile.cs` — mappings for the same type pair can be split
  across multiple profiles
- Validators (if any): `Validators/<X>/Create<X>Validator.cs` /
  `Update<X>Validator.cs` — **note some validators exist but are never
  injected into their service** (confirmed pattern for `Deal`, `User`,
  `Restaurant` as of the last full architecture review); check the service
  constructor to see if the validator is actually used, don't assume a
  validator file being present means it's enforced
- EF config: `Data/ApplicationDbContext.cs` (`OnModelCreating`, fluent config,
  `DbSet<X>`), plus `Migrations/` for schema history

Frontend (`EatKath.Web/src/`), per domain `<X>`:
- Types: `types/<X>.ts` (response shape), `types/Create<X>.ts`,
  `types/Update<X>.ts` — **these frequently diverge from each other and from
  the backend DTOs**; never assume they're kept in sync
- Service: `services/<X>Service.ts` and/or `services/Owner<X>Service.ts` —
  EatKath sometimes splits a public read-only service from an
  authenticated/Owner CRUD service for the same domain (see `DealService.ts`
  vs `OwnerDealService.ts`)
- Axios client: `api/axios.ts` (shared instance, JWT attached via request
  interceptor — check here only if the issue looks like an auth/header
  problem, not a field-mapping problem)
- Pages/forms: `pages/*.tsx` — EatKath forms are mostly hand-rolled
  controlled components (`useState` + `onChange`), not a shared form
  library; a field can be dropped simply by not being included in a page's
  local state object, with no type error resulting if the state's inferred
  shape just happens not to include it
- Components: `components/<domain>/*.tsx`

Tests:
- `EatKath.API.Tests/Services/<X>ServiceTests.cs` — only 3 domains have any
  tests as of the last review (Area, Deal, Redemption), and even those only
  cover a subset of methods (mostly `CreateAsync`). `EatKath.API.Tests/Controllers/`
  and `EatKath.API.Tests/Validators/` exist as empty scaffold folders — do
  not assume tests exist just because the folder does; check for actual
  `[TestMethod]`s.

## Investigation procedure

Work outward from the database to the UI, or inward from the UI to the
database — either direction is fine, but check **every** layer below, even
if earlier layers look fine. Missing/inconsistent fields are rarely visible
from a single layer.

1. **EF Core entity** — does the property exist on `Entities/<X>.cs`? Note
   its type, nullability, and default value.
2. **ApplicationDbContext / configuration** — is there any fluent config for
   this property (precision, required, index, cascade behavior)? Confirm via
   `Migrations/ApplicationDbContextModelSnapshot.cs` that the column actually
   exists in the current schema (a property can exist on the entity without
   a corresponding migration if someone forgot to add one).
3. **C# DTOs** — check every DTO in `DTOs/<X>/`: is the property present on
   the response DTO (`<X>Dto`)? On `Create<X>Dto`? On `Update<X>Dto`? On any
   action-specific DTO? A property missing from the **response** DTO means
   clients can never read the current value. A property missing from an
   **input** DTO means clients can never set/preserve it.
4. **AutoMapper / service mapping** — find the `CreateMap<...>()` calls for
   this entity/DTO pair across `Mappings/*.cs`. Confirm there's no explicit
   `.Ignore()` or conflicting `.ForMember()` for this property. If the
   service builds the DTO by hand instead of via AutoMapper (this happens —
   e.g. `RestaurantService` does this for most of its methods), check that
   hand-built object literal directly instead.
5. **Service business logic** — in `Services/<X>Service.cs`, check
   `CreateAsync`/`UpdateAsync` (or equivalents): is the property read from
   the incoming DTO and applied to the entity, or could it be silently
   defaulted/overwritten? Pay attention to full-entity `_mapper.Map(dto,
   entity)` calls — these overwrite every property the mapper touches,
   including with the DTO's default value if the caller never set it.
6. **Controller** — does the endpoint that returns/accepts this DTO exist,
   and is there any controller-level shaping (rare in EatKath, but check)
   that would strip the field before it reaches the client?
7. **Frontend TypeScript types** — check `types/<X>.ts`, `types/Create<X>.ts`,
   `types/Update<X>.ts` for the field. Compare field-for-field against the
   backend DTOs from step 3 — these are hand-written and not generated, so
   drift is the default expectation, not the exception.
8. **Frontend API/service layer** — in `services/<X>Service.ts` /
   `services/Owner<X>Service.ts`, confirm the method signatures use the
   types checked in step 7 and don't transform/strip the field in transit.
9. **Frontend components/forms** — in the relevant `pages/*.tsx` /
   `components/**/*.tsx`, confirm the field is: read from any loaded data,
   included in local form state, rendered as an input/display, and included
   in the object sent back on submit. A field can be present in the
   TypeScript type but still dropped here if the component's initial
   `useState` literal simply omits it (TypeScript won't catch this unless
   the state type is explicitly annotated and strict).
10. **Tests** — search `EatKath.API.Tests/` for any test that already
    exercises this field (by property name). Note whether existing tests
    would have caught an inconsistency here, or whether the gap is
    untested (this is usually the case).

## Output format

Report back with:

1. **End-to-end data flow diagram** (text arrows are fine), showing the
   field's journey through every layer it actually appears in, e.g.:
   ```
   Entity.Prop --config--> DbContext/Migration --DTO--> ServiceX --Controller--> ...
   ```
   and explicitly mark where the chain **breaks** (layer that's missing it).

2. **Layer-by-layer table**, one row per layer from the checklist above,
   marking each ✅ present / ❌ missing / ⚠️ present but inconsistent
   (wrong type, different default, transformed), with the file path and
   (if relevant) line reference for each.

3. **Root cause explanation** in prose — which specific gap(s) cause the
   observed symptom, and why (e.g. "missing from the response DTO means the
   edit form has nothing to pre-populate from; missing from the update DTO's
   frontend type means the field is never sent back, so the backend
   receives no value and defaults it to 0/null/false on save").

4. **Suggested fix** — the specific files and changes that would close the
   gap, following existing conventions in the codebase (e.g. "add the
   property to `DealDto.cs` next to its neighboring fields; AutoMapper will
   pick it up by name convention, no profile change needed"). Describe the
   fix; **do not apply it** unless the user explicitly asks in a follow-up.

5. **Test coverage note** — whether a regression test exists or should be
   added, and roughly what it should assert.

## Worked example (for calibration)

`Deal.ReservationLimit`: present on `Entities/Deal.cs`, `CreateDealDto`,
`UpdateDealDto`, and the frontend `CreateDeal`/`DealForm` types — but
**absent from `DealDto` (response)** and **absent from the frontend
`UpdateDeal` type and `EditDealPage.tsx` form state**. Result: the edit page
could never display the current value, and every PUT request omitted the
field, so ASP.NET Core model-bound it to `0` and `DealService.UpdateAsync`'s
`_mapper.Map(dto, deal)` overwrote the real value with `0` on every save.
This is the shape of bug this skill is designed to catch before it ships,
or diagnose quickly after it's reported.
