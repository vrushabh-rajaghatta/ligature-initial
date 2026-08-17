# Acme

A single-tenant document-management platform — one deployment and one
database per customer (ADR-066) — and the first vertical of a product
that will grow one bounded context at a time. Derived from the RegOS codebase;
"Acme" is a placeholder name (see README § Rename me first).

.NET 10 · minimal APIs · EF Core / PostgreSQL · React + Vite + TanStack Query.

---

## Before writing code

**Read [docs/engineering/slice-conventions.md](docs/engineering/slice-conventions.md).**
It says where files go and what they are called, and every backend rule in it
is enforced by a test. Then run:

```bash
dotnet test tests/Architecture/Acme.Architecture.Tests
```

If it fails, the slice is not finished. **Never** add an entry to a
grandfathered list to make new code pass — every such list in this repo is
empty, and it stays that way.

## The rules most often broken

| | Rule | Not this |
|---|---|---|
| SC-001 | Every route starts `/api` | `/documents/{id}` |
| SC-002 | `I<X>Repository` in the **Domain** project, beside its aggregate | in `Application/Persistence/` |
| SC-003 | Query folder holds `<Name>Query.cs` | loose params on `HandleAsync` |
| SC-004 | Endpoint handler is a named static method | inline `async (…) =>` lambda |
| SC-005 | One handler per file, named after it | `DocumentQueries.cs` with three |

Frontend (reviewed, not linted): one file per API call, one file per hook,
a zod schema in `validation/` for every form, `Dialog` and `Form` as separate
components, every mutation's error state rendered (SC-106).

## Architecture canon

1. **[docs/adr/](docs/adr/)** — inherited from RegOS with original numbers
   (code cites them); numbering has gaps. Next number is **ADR-067**. Never
   edit an accepted ADR; supersede it.
2. **[docs/engineering/slice-conventions.md](docs/engineering/slice-conventions.md)** — mechanical file/folder rules.
3. Where code and docs disagree, **the code is the truth** — then fix the doc
   in the same PR.

### Decisions you will otherwise re-derive

- **ADR-016** — repositories for writes, `AcmeDbContext` + `AsNoTracking()`
  for reads. A query handler never loads an aggregate.
- **ADR-066** — **the deployment is the tenant.** One database per customer,
  so there is no `TenantId`, no `ITenantContext` and no query filter anywhere.
  `AcmeDbContext` has no global filters and that is the decision, not an
  omission. Supersedes ADR-013/024/030/031/060. If a second customer must ever
  share one deployment, read that ADR's *Revisit When* before adding a filter
  back.
- Provisioning is an infrastructure pipeline, not an application feature:
  create database, migrate, seed the administrator, register the hostname.
  `AdministratorSeeder` does the third step — from configuration, on an empty
  database only, and **never with a password** (the first admin is invited and
  chooses their own).
- **ADR-024 / ADR-025** — tenancy is derived from identity (a signed claim),
  never asserted by the caller; sessions are server-owned HttpOnly cookies.
- **ADR-012 / ADR-022** — four semantic exceptions map to 401/404/409/400 in
  middleware. Endpoints do not catch.
- **ADR-018** — duplicate twice, abstract on the third *demonstrated* need.
  Forbids speculative deletion as much as speculative creation.
- **ADR-064** — test suites provision their own database per assembly;
  `ACME_TEST_POSTGRES` points at the server.

## Aggregates

Frozen shape — private constructor, static `Create()` factory, behaviour
methods, no public setters. Aggregates reference each other **by id only**.

Identity is `sealed class <X>Id : StronglyTypedId` and the entity inherits
`AggregateRoot<TId>` or `Entity<TId>`. Copy
[DocumentId.cs](src/DocumentManagement/Acme.DocumentManagement.Domain/Aggregates/Documents/DocumentId.cs) —
never a `record struct` id. The aggregate folder is plural
(`Aggregates/Documents/` holds `Document`) so the namespace never equals the
type name. A shadow FK declared with a reference-type id becomes **optional**
unless `.IsRequired()` is added — an optional FK severs instead of deleting.

Lifecycle over deletion: entities move between statuses rather than being
removed.

## Layout

```
src/<Context>/Acme.<Context>.{Domain,Application,Infrastructure}
src/Host/Acme.Api                  minimal API host, endpoints + Program.cs
src/Persistence/Acme.Persistence   AcmeDbContext, all EF config + migrations
src/Shared/Acme.SharedKernel       kernel scope only (ADR-017)
src/Storage/Acme.Storage           IFileStorage port
web/acme-web                       React frontend, feature-first
tests/                             mirrors src/, plus tests/Architecture/
```

Contexts today: **Platform** (identity) · **DocumentManagement**.
The dependency graph between contexts is a whitelist in
`ContextDependencyTests` and currently holds **0 edges** — adding one is a
decision, made in an ADR, then declared there.

## Working agreements

- One story at a time, delivered as a vertical slice — domain through API
  through UI.
- Use the ubiquitous language: `Document`, `DocumentVersion`, `User` — never
  `Record`, `Item`, `Data`. `Tenant` is retired (ADR-066); a deployment serves
  one customer and does not name them.
- Generic folders (`Common`, `Shared`, `Helpers`, `Utils`, `Misc`) are
  prohibited in `src/` without an ADR.
- New bounded context, new cross-context dependency, or a change to an
  accepted decision → **ADR first**.
- Commit only when asked. Branch before committing if on `main`.

## Commands

```bash
dotnet build Acme.slnx
dotnet test Acme.slnx
dotnet test tests/Architecture/Acme.Architecture.Tests   # conventions

cd web/acme-web && npm run dev
npm run build && npm run lint

# EF migrations (local tool)
dotnet ef migrations add <Name> \
  --project src/Persistence/Acme.Persistence \
  --startup-project src/Host/Acme.Api
```
