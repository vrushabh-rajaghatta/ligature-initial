# ADR-066 — The Deployment Is the Tenant

**Status:** Accepted · **Date:** 2026-08-17 ·
**Supersedes:** [ADR-013](ADR-013-ambient-tenant-context.md) (ambient tenant context),
[ADR-024](ADR-024-tenancy-is-derived-from-identity.md) (tenancy from identity),
[ADR-030](ADR-030-tenant-is-its-own-aggregate.md) (tenant aggregate),
[ADR-031](ADR-031-tenant-isolation-by-query-filters.md) (query filters),
[ADR-060](ADR-060-a-tenant-provisions-without-an-organization.md) (tenant provisioning) ·
**Narrows:** [ADR-021](ADR-021-email-is-globally-unique.md) (global email → per-deployment),
[ADR-033](ADR-033-three-roles-and-where-authority-lives.md) (three roles → two),
[ADR-038](ADR-038-organization-depth-roots-and-the-three-filter-shapes.md) (the three filter shapes, now zero) ·
**Related:** [ADR-016](ADR-016-persistence-access-model.md) (persistence access),
[ADR-027](ADR-027-invitation-is-a-consumable-grant.md) (how the first admin arrives),
[ADR-064](ADR-064-the-test-suite-provisions-its-own-schema.md) (test provisioning, now one database not two)

## Context

Acme isolates tenants by row. Every tenant-owned entity carries a `TenantId`,
`AcmeDbContext.ApplyTenantFilters` applies a fail-closed global query filter to
each, tenancy is derived from a signed claim, and a suite of architecture tests
exists to catch the entity that joins the model unfiltered. It works. It is also
the most intricate machinery in the codebase, and all of it exists to defend
against one thing: **a query that forgets its `.Where`.**

The deployment model is changing. Each customer will get their own database.
Once that is true, the entire apparatus is defending a boundary that the
infrastructure already enforces — and defending it *worse*, because a query
filter is a runtime construct that can be bypassed with `IgnoreQueryFilters()`
(three call sites do exactly that today, deliberately) while a separate database
cannot be bypassed at all.

The question this ADR settles is not whether to keep the filters. It is whether
the *concept* survives. A tenant identifier that is the same value in every row
of every table of a given database is not modelling anything; it is a constant
being carried through the domain, the token, the kernel and the UI, at the cost
of a nullable-tenant special case in `User`, a lenient accessor on
`ITenantContext` that exists for one consumer, and three named bypasses.

## Decision

**1. The deployment is the tenant, and the application has no tenant concept.**
One database per customer. Which customer a request belongs to is answered by
which database the process is connected to — never by a column, a claim, a
header, or a filter. `TenantId` leaves the kernel entirely; `ITenantContext`
and its implementations are deleted.

**2. Row-level isolation is deleted, not narrowed.** `ApplyTenantFilters` goes
in full, including the `UserDirectoryRow` filter that exists because `ToView`
read models do not inherit their aggregate's filter. There is nothing left for
them to isolate. `IgnoreQueryFilters()` disappears from `UserPolicy` for the
same reason: with no filter in place, "unique across Acme" and "unique here"
are the same query.

**3. The `Tenant` aggregate and its administration are removed from the
product.** `Tenant`, `TenantStatus`, `ITenantRepository`, the four tenant
commands, `GetTenants`, `GetTenantUsers`, `TenantEndpoints`, and the
`features/platform/tenants/` frontend area are deleted. A deployment cannot
create its own siblings, so provisioning is not an application capability.

This is a **real loss of function**, recorded plainly rather than glossed:
creating a customer stops being a form a support person fills in and becomes an
infrastructure pipeline — create database, migrate, seed the first
administrator, register a hostname, issue the invitation. An internal console
that does this across customers is a **separate control-plane application with
its own database**, and it is explicitly out of scope here. Building one inside
this codebase would reintroduce exactly what this ADR removes.

**4. Identity is per-deployment, and the unique index is the whole rule.**
ADR-021 made email globally unique so that authentication could resolve a user
before any tenant existed. That reasoning survives intact and gets simpler: the
unique index on `Users.Email` now means unique *in this deployment*, which is
the only scope there is. `User.TenantId` and both tenant-aware factories
(`CreateForTenant`, `CreatePlatformUser`) collapse into one `Create()`.

`LoginHandler` loses its tenant-status check outright. A retired customer is a
deployment that is switched off, which is a stronger statement than a status
column and needs no code to enforce.

**ADR-024's pattern outlives its subject, and code still cites it.** That ADR
said tenancy must be *proven* by a signed claim rather than *asserted* in a
request; the tenancy half is superseded here, but the same rule still governs
identity — `ChangePasswordCommand` and `RevokeSessionCommand` deliberately
carry no `UserId`, because the way to guarantee a caller acts only on
themselves is to leave them no way to name anyone else. Those citations are
correct and stay.

**5. Two roles, and the first administrator arrives by invitation.**
`UserRole.PlatformAdministrator` is removed — it named a person who operates
Acme across tenants, and there is no across. `TenantAdministrator` is renamed
`Administrator`. Policy gating (ADR-033 rule 4) is otherwise unchanged: two
roles still need one policy, and handlers still never check roles themselves.

The renamed claim value invalidates every issued token. Accepted, on the same
grounds ADR-030's claim rename was accepted — the tokens in existence belong to
seeded development accounts.

**Provisioning seeds one administrator, and never a password.** The admin's
email address comes from configuration; the user is created `Invited` and an
invitation is issued through the existing ADR-027 flow. **No known-password
account is ever created outside Development** — the guarantee
`Program.cs` already states at its dev-seeder call site, now load-bearing in
production. A password that lived in configuration, an environment variable or
an image layer would be one that every operator and every backup of that
configuration also holds.

Roles beyond these two are deliberately deferred. ADR-033's resistance to
growing a permission matrix before a feature needs one is unchanged by this ADR.

**6. Sequencing is part of the decision.** The code change must not reach a
database that still holds more than one customer's rows. Deleting the filters
there makes every customer's documents visible to every user immediately, with
nothing left to catch it. **Per-customer databases are provisioned and the data
split first; this change ships second.** Not the reverse, and not together.

## Consequences

**A class of defect becomes unwritable.** A forgotten `.Where`, an unfiltered
new entity, a `ToView` model that silently skipped its filter, a bypass that
outlived its justification — none of these can leak across customers any more,
because there is nothing to leak into. `TenantFilterArchitectureTests` is
deleted for the best possible reason: its premise is gone.

**The persistence layer gets substantially smaller.** `AcmeDbContext` loses its
optional tenant constructor parameter, both current-tenant accessors and all
three filters. Roughly 25 files are deleted and 50 simplified.

**Cross-customer questions stop being queries.** "How many customers are
there", usage, billing, and any support view over more than one customer now
require either a registry maintained outside the app or an aggregation job that
visits each database. This is the largest ongoing cost and it is permanent.

**A release becomes a fan-out with partial-failure semantics.** Migrating N
databases means N chances to fail, and a fleet can end up split across schema
versions. `Database:MigrateOnStartup` defaulting to `false` — an instance that
refuses to boot against an unmigrated database, naming the pending migrations —
becomes considerably more valuable than it was, and the idempotent-script
deployment path Program.cs already recommends is what gets fanned out. Schema
drift, which is currently impossible, becomes something that must be monitored.

**Onboarding latency goes from seconds to minutes**, and cannot be performed
from the UI.

**Support loses read access to customer data**, which strengthens the
compliance story and weakens time-to-diagnose. A per-deployment operational CLI
(reset a password, re-issue an invitation) is required, because no
platform-administrator account exists to do it.

**Per-customer backup, restore, residency and erasure become trivial** — the
last one a `DROP DATABASE` rather than a cascade that has to be correct.

**Local development gets simpler**: one database, no seeded tenants, no tenant
switching, no fixture that must decide which tenant it is acting as.

**File storage does not follow automatically, and this is a known gap.**
`LocalFileStorage` writes to `documents/{documentId}/…` with no customer
segment anywhere in the path. Until storage is partitioned per deployment,
"restore this customer" and "erase this customer" remain incomplete operations
and the isolation claim above stops at the database boundary. Closing it is a
follow-up, not part of this ADR.

## Revisit When

- A second customer must be served by **one** deployment — the model above has
  no answer, and reintroducing tenancy is a larger change than removing it was.
- Cross-customer reporting becomes a product requirement rather than an
  internal convenience, at which point the control-plane application deferred
  in decision 3 has to be built rather than avoided.
- The number of customers makes per-deployment migration fan-out the dominant
  cost of shipping.
