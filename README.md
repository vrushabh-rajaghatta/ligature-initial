# Acme — Document Management Platform

A single-tenant B2B SaaS starter — **one deployment and one database per
customer** (ADR-066) — derived from the RegOS codebase: its platform chassis
(identity, sessions), its document-management vertical, and —
most importantly — its **executable architecture conventions**. The first
product surface is document management; further contexts (Products, …) are
meant to be built on top, one bounded context at a time.

.NET 10 · minimal APIs · EF Core / PostgreSQL · React 19 + Vite + TanStack Query.

---

## Rename me first

**"Acme" is a placeholder.** Rebranding is one mechanical pass, best done
before the first commit:

```bash
# 1. File contents (case-sensitive, three casings)
grep -rl 'Acme\|acme\|ACME' --exclude-dir=node_modules --exclude-dir=bin --exclude-dir=obj . \
  | xargs sed -i '' -e 's/Acme/NewName/g' -e 's/acme/newname/g' -e 's/ACME/NEWNAME/g'

# 2. File and directory names, deepest first
find . -depth -name '*Acme*' -not -path '*/node_modules/*' | while read -r p; do
  mv "$p" "$(dirname "$p")/$(basename "$p" | sed 's/Acme/NewName/g')"
done
mv web/acme-web "web/newname-web"
```

Then rebuild (`dotnet build Acme.slnx` → new name) and regenerate nothing —
namespaces, the DbContext, the test-database prefix and the `ACME_TEST_POSTGRES`
variable all follow the rename.

## Quick start

```bash
# A PostgreSQL server on 5432. These credentials are what the dev config
# expects (src/Host/Acme.Api/appsettings.Development.json).
docker run -d --name acme-postgres -p 5432:5432 \
  -e POSTGRES_USER=admin -e POSTGRES_PASSWORD=password123 postgres:18

dotnet build Acme.slnx
dotnet run --project src/Host/Acme.Api        # → http://localhost:5225
                                              # migrates + seeds in Development

cd web/acme-web && npm install && npm run dev  # → http://localhost:5173
```

Development seeds known accounts (see `src/Host/Acme.Api/Development/`), and
they exist **only** when the environment is Development:

| Account | Password | Role |
|---|---|---|
| `dev@acme.local` | `development-password` | Administrator |

### Already running a PostgreSQL on 5432?

Nothing needs editing — override the two connection strings instead. Both are
verified working:

```bash
# The app
export ConnectionStrings__Acme="Host=localhost;Port=5432;Database=acme;Username=postgres;Password=postgres"

# The test suites (they create and drop their own databases on this server)
export ACME_TEST_POSTGRES="Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres"
```

To change them permanently instead, edit
`src/Host/Acme.Api/appsettings.Development.json` and the `LocalDefault`
constant in `tests/TestSupport/Acme.TestSupport/TestPostgres.cs`.

### Three ports that must agree

`web/acme-web/.env.development` points the SPA at the API (`:5225`), and
`Program.cs` allows CORS **with credentials** from the Vite dev server
(`:5173`) only. Change one and change the others, or the session cookie is
silently withheld and every request looks unauthenticated.

### Tests

```bash
dotnet test Acme.slnx
```

Database-touching suites provision **their own database per test assembly**
from the migration chain (ADR-064) and drop it afterwards. They default to
`localhost:5432` / `admin` / `password123`; point them elsewhere with:

```bash
export ACME_TEST_POSTGRES="Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres"
```

All 351 tests are green (378 before ADR-066, which deleted 27 whose subject
it removed). `npm run build` and `npm run lint` are clean.

---

## What this codebase is

Two bounded contexts, zero edges between them — the dependency graph is a
whitelist asserted by a test, and it currently says `0`:

| | |
|---|---|
| `src/Platform` | Identity: `User`, `UserCredential`, `Session`, `RefreshToken`, `Invitation`, `PasswordReset`. Cookie sessions (ADR-025). No `Tenant` — ADR-066 made the deployment the customer. Reaches the product domain only through `Platform.Contracts` (a single `UserId`). |
| `src/DocumentManagement` | The first product context: `Document` → immutable `DocumentVersion`s, Draft → Active → Archived lifecycle, bytes behind the `IFileStorage` port. |
| `src/Shared/Acme.SharedKernel` | `AggregateRoot<TId>`, `Entity<TId>`, `StronglyTypedId`, the four-exception hierarchy. ADR-017 scope — concepts, not patterns. |
| `src/Storage` | `IFileStorage` + `LocalFileStorage`. A driven port, not a context. |
| `src/Persistence` | The one `AcmeDbContext`, all EF configuration, migrations, seed initializers. **No global query filters** — isolation between customers is the database boundary (ADR-066). |
| `src/Host/Acme.Api` | Composition root. Endpoint-per-file, semantic exceptions mapped to status codes in middleware, endpoints never catch. |
| `tests/Architecture` | The immune system: 44 facts enforcing routes, layout, identity, the dependency graph, deterministic ordering, client/API route alignment. **Grandfathered lists are empty and must stay empty.** |

The frontend (`web/acme-web`) is feature-first: `auth`, `platform`, `settings`,
`documents`, with the conventions in
[docs/engineering/slice-conventions.md](docs/engineering/slice-conventions.md)
(§ Frontend).

## Provenance, and how to read `docs/`

- **`docs/adr/`** — a curated subset of RegOS's ADR series, copied with their
  original numbers because code comments cite them (`// ADR-031`). Numbering
  therefore has gaps; some prose still describes RegOS's regulatory domain —
  read them for the *decision*, not the examples. New ADRs continue from
  **ADR-066**.
- **`docs/engineering/slice-conventions.md`** — the mechanical rules (SC-001…),
  each enforced by a test in `tests/Architecture`. Reference citations point at
  files in this repo.
- Deliberately **not** carried over: RegOS's eleven regulatory contexts, its 92
  migrations (this repo starts from one `InitialCreate`), every grandfathered
  exception, its 15 legacy record-struct ids, and the Playwright browser suite.

## Adding the next context

1. ADR first (ADR-001, ADR-018 — a new context is a decision).
2. `src/<Context>/Acme.<Context>.{Domain,Application,Infrastructure}` per the
   slice shape in the conventions doc. Copy
   [DocumentId.cs](src/DocumentManagement/Acme.DocumentManagement.Domain/Aggregates/Documents/DocumentId.cs)
   for identity — never a record struct.
3. Entity registration in `AcmeDbContext`, EF config under
   `Persistence/Configurations/<Context>/`, one migration.
4. Add the context to `DomainMayReference` in `ContextDependencyTests` (edge
   count updates with it) and run
   `dotnet test tests/Architecture/Acme.Architecture.Tests` — the slice is not
   finished until it passes.

## Known gaps at derivation time

- **Provisioning has no home yet.** ADR-066 moved it out of the application:
  creating a customer means create database → migrate → seed administrator →
  register hostname, and only the third step exists (`AdministratorSeeder`).
  The rest is a deployment pipeline nobody has written.
- **File storage is not partitioned per customer.** `LocalFileStorage` writes
  to `documents/{documentId}/…` with no customer segment, so "restore this
  customer" and "erase this customer" stop at the database boundary.
- No file **download** endpoint yet — uploads store bytes and metadata, the
  detail view shows versions; serving bytes back is the natural next story.
- `LocalFileStorage` writes to a local directory; a blob-storage adapter slots
  in behind `IFileStorage` without touching any context.
- Documents are hard-limited to name + versions: no folders, tags, or types.
  A `DocumentType` lookup, when needed, should arrive with an ADR (the
  master-data shape, ADR-043 §2).
