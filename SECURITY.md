# Security notes

## Authentication

- Passwords created by the API are stored as versioned PBKDF2-SHA256 hashes with a random salt.
- Existing SHA-1 password hashes are still accepted at login and upgraded to PBKDF2 after successful authentication.
- Authentication cookies are HttpOnly and SameSite=Lax. Secure cookies are required outside Development.
- Sessions expire after 14 days with sliding renewal. API authentication failures use HTTP 401 and authorization failures use HTTP 403.
- Each cookie-authenticated request checks the server-side token and current user role. Logging out revokes the token, so a copied cookie cannot be reused afterward.
- `GET /api/v1/access/sessions` lists the current user's active sessions without exposing token values. `DELETE /api/v1/access/sessions/{id}` revokes one owned session; `DELETE /api/v1/access/sessions` revokes all.
- Never use the local bootstrap administrator defaults in a deployed environment. Set `ADMIN_LOGIN`, `ADMIN_PASSWORD`, and `ADMIN_EMAIL` through deployment secrets.

## Deployment

- Serve the API over HTTPS in every non-development environment.
- Keep database credentials and `SENTRY_DSN` in deployment secret storage, not in committed configuration.
- The Docker runtime runs as the non-root `app` user.
- `/health/live` reports process liveness; `/health/ready` checks PostgreSQL connectivity and returns 503 until it is available.
- Soft-deleted files and folders are eligible for permanent removal after 30 days. Physical file deletion is restricted to the configured upload root.
- `System.Diagnostics.Metrics` publishes `diskapi.http.requests` and `diskapi.http.request.duration` with method/status tags for OpenTelemetry or a metrics exporter.
- The CI workflow restores, builds and runs tests for the .NET 9 solution on pull requests and pushes to `main` or `master`.

## Reporting a vulnerability

Do not include credentials, session cookies, or personal data in issue reports. Provide a minimal reproduction and affected endpoint/configuration.
