# DiskAPI portfolio roadmap

Continue with `$continue-project`; do not ask the user to repeat “continue”. Mark each item complete only after implementation and relevant verification.

1. [ ] Validate EF migrations against a clean database and an existing database; document the safe upgrade path.
2. [ ] Add database constraints and query indexes for active users, tokens, files, and folders; verify migration upgrade and rollback.
3. [x] Add unit tests for password hashing, legacy hash migration, repository filtering/pagination, and account password changes.
4. [ ] Add HTTP integration tests for authentication, authorization, validation, errors, and CRUD endpoints.
5. [ ] Run PostgreSQL integration tests with Testcontainers in CI.
6. [x] Add per-IP rate limiting to login and verify 429 behavior.
7. [ ] Add persistent login-attempt tracking, progressive delay/lockout, and security audit events.
8. [ ] Complete account recovery with one-time, expiring reset tokens and a delivery abstraction; password change is already implemented.
9. [x] Complete session management: list sessions, revoke one session, revoke all sessions, and test expiry/revocation.
10. [ ] Add file/folder ownership and enforce owner-scoped reads, updates, downloads, and deletion.
11. [x] Stream downloads and support HTTP range requests without buffering whole files.
12. [ ] Introduce a storage abstraction with local development storage and an S3-compatible implementation.
13. [x] Harden uploads with size/type checks, content-signature validation, normalized display names, and safe download headers.
14. [ ] Make uploads atomic across disk and database, with cleanup/reconciliation for partial failures.
15. [ ] Add a recycle bin with restore and scheduled permanent deletion.
16. [x] Complete search, sorting, and filters for files/folders with documented, bounded query parameters.
17. [x] Return consistent JSON for unhandled API errors and include a correlation identifier.
18. [x] Complete observability with exported metrics, request tracing, health/readiness checks, and secret-safe structured logs.
19. [ ] Complete CI with tests, formatting/static analysis, dependency vulnerability checks, and Docker image build.
20. [x] Finish the portfolio README with architecture/ERD diagrams, reproducible setup, API examples, and demo scenarios.

## Verification notes

- The solution has built successfully with `dotnet build src/App/App.sln --no-restore -m:1`.
- Twenty-two tests passed with `DOTNET_ROLL_FORWARD=Major dotnet test src/App/App.sln --no-restore -m:1`.
- PostgreSQL and Docker integration have not been verified because the Docker daemon is unavailable in the current environment.
- Item 2 now includes active-user/token constraints plus composite indexes for files by folder/deleted state, folders by parent/deleted state, and tokens by user/expiry. Clean/existing database apply and rollback still need PostgreSQL verification.
- README now documents migration preflight duplicate checks, backup/apply steps, clean-database setup, and rollback selection. Migration execution remains unverified without PostgreSQL.
- Item 17 has middleware unit tests for safe error JSON, correlation IDs, and invalid header replacement.
- Item 6 uses a shared per-IP fixed-window policy; TestServer integration verifies ten requests pass and the eleventh returns HTTP 429.
- TestServer integration now exercises the real access controller with SQLite for generic invalid-credential responses and successful cookie/session issuance. Broader authorization and CRUD endpoint coverage remains for item 4.
- Session list/revoke endpoints have SQLite service tests for expiry, deleted tokens, owner-scoped revoke, and revoke-all. Revoke-all uses a single filtered soft-delete update, avoiding per-entity tracking behavior.
- CI includes build, tests, dependency audit, Docker build, curated whitespace checks, and solution-wide analyzer checks. The local dependency audit is clean; workflow execution and solution-wide whitespace formatting remain for item 19.
- HTTP request counters and duration histograms are exported in Prometheus text format at `/metrics`, with bounded method/status labels. Request logs are scoped with trace/correlation IDs; the response exposes `X-Trace-ID`, while unhandled errors stay secret-safe.
- Upload restrictions, known-format signature checks, and rollback cleanup exist; crash-reconciliation remains for item 14.
- Upload validation now uses an explicit extension allowlist, size limits, signature checks for binary formats, control-byte checks for text formats, and Unicode NFC display names. Unit tests cover format mismatch and filename normalization.
- Failed uploads clean up created files; the hosted cleanup now also removes local storage objects older than one hour that have no matching AppFile record. The reconciliation policy builds but needs crash-simulation/disk integration tests before item 14 is complete.
- Files and folders now have authenticated trash-list and restore endpoints plus a 30-day hosted cleanup job; API-level and PostgreSQL/disk integration tests remain for item 15.
- Folder and file listings support bounded name search; folders also filter by parent. Sorting is now implemented and documented; HTTP-level filter tests remain for item 16.
- The cleanup worker and search/filter changes build cleanly; seventeen tests pass, though the worker itself needs a PostgreSQL integration run.
- Item 16 now supports whitelisted sorting: files by id/name/created_at, folders by id/name, with asc/desc and a stable id tie-breaker. Query options and bounds are documented; repository sorting has a SQLite test. HTTP invalid-parameter behavior still depends on the existing request validation path.
- Item 9's revoke-all regression test now passes after switching the bulk operation to a filtered SQL soft-delete.
- Item 20 now documents reproducible setup, architecture/ERD, bounded list-query examples, and an end-to-end local demo path including range downloads and trash restore.
- The local vulnerability audit found a High advisory in test-only SQLitePCLRaw 2.1.10. A direct 2.1.13 override removes the advisory; the audit now reports no vulnerable packages across all six projects.
- Item 12 now has an `IFileStorage` boundary with a local implementation; uploads, range-capable downloads, rollback cleanup, and scheduled trash deletion use it. An S3-compatible adapter and storage-specific tests remain before closing the item.
- Local storage tests now cover store/read/list/delete and traversal rejection. Item 14 adds orphan reconciliation after a one-hour grace period; worker-level crash simulation remains unverified.
