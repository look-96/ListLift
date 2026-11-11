# ListLift security review

**Review date:** 11 November 2026

**Scope:** Current source and the user-reported local walkthrough. This is a development-project review, not a penetration test, compliance certification, or claim of ASVS conformance.

## Controls currently present

| Area | Implemented controls | Remaining risk or follow-up |
|---|---|---|
| Layer boundaries | Domain rules are framework-independent; Application handlers coordinate ports and use cases; API and Infrastructure provide HTTP, persistence, identity, file, and marketplace adapters. | Continue reviewing changes for inward-only dependencies and single-responsibility boundaries. |
| Authentication | ASP.NET Core Identity API endpoints, bearer access/refresh tokens, lockout after repeated failures, and rate limits. Tokens live in scoped in-memory client state. | Public registration is enabled for local development. No entitlement/payment gate, email delivery, password recovery delivery, breached-password screening, or client MFA setup is ready for public users. |
| Passwords | New-password policy uses a 15-character minimum without composition rules, consistent with current NIST guidance for single-factor passwords. | Consider MFA/passkeys and account recovery design before public access. Verify existing account policy behavior. |
| Authorization | Draft operations require authentication; handlers derive the owner from the current principal and repository access is owner-scoped. Admin settings additionally require a laptop-held credential and an Identity bearer token. | Add HTTP-level integration tests for anonymous access, cross-owner access, token refresh/revocation, and admin identity binding. The special key is a second gate; production should also bind administration to a dedicated role/subject. |
| SQL injection | EF Core LINQ and parameterized operations are used for user data. | Keep raw SQL out of user-driven paths or use parameters if raw SQL becomes necessary. |
| XSS | Blazor/Razor normal text rendering encodes user text; user values are not intentionally rendered as raw HTML. A Content Security Policy is present in the client HTML. | Do not introduce `MarkupString` for seller/AI text. A meta CSP is not a replacement for production response headers; review third-party scripts and CSP when deployed. |
| Request validation | API DTOs and Application commands use validation attributes; domain methods enforce core invariants. Price parsing/currency is UK-oriented. Quantity defaults to one and is range-checked. | Client-side validation is usability only. Keep API/domain validation authoritative and test new fields. |
| Photo upload | Bounded uploads; declared type/signature/decode checks; image decode, orientation, metadata stripping, and re-encoding; random stored names; private storage outside `wwwroot`; bounded image processing. | Add per-user aggregate storage/rate quotas, authorized retrieval/preview, and monitor cleanup failures. Pending local photo bytes are not saved to browser recovery storage. |
| Data recovery | Unsaved text form values are held in same-tab `sessionStorage`, keyed from the normalized sign-in email, to recover after reload and re-authentication. Sign-out removes the entry. Authentication tokens and photo bytes are never stored there. | Browser storage is accessible to same-origin JavaScript; it is not encrypted and must never contain passwords, tokens, secrets, or high-sensitivity data. Same-origin XSS could read it. Closing the tab clears it. |
| Admin secret | PowerShell setup generates a random credential and encrypts the local copy with Windows DPAPI; only a SHA-256 hash is placed in User Secrets; verifier comparison is constant-time. | The encrypted credential is tied to the Windows account/device. Rotate it after device/account loss. Do not reuse this local-only mechanism as cloud admin authentication. |
| eBay application credentials | Settings endpoint returns only configured state; credentials are protected using ASP.NET Core Data Protection before database storage. Category lookup is read-only and uses `EBAY_GB`. Azure mode stores the Data Protection key ring in private Blob storage and protects it with a Key Vault key. | Azure resources and managed-identity permissions are not provisioned yet; no seller-specific authorization or publishing exists. |
| Errors and throttling | Safe Problem Details responses; Identity, general API, and admin rate-limit policies. | Configure trusted proxy/forwarded headers before IP-based limits are relied on behind a reverse proxy. Add monitoring and per-user quotas. |
| Configuration and deployment | Development CORS is constrained; LocalDB configuration is development-only; local secret files and private uploaded data are ignored by Git. Azure mode uses managed identity for private Blob storage and Key Vault encryption, and rejects local photo storage on App Service. | Azure resources and permissions are not configured or exercised yet. TLS/HSTS/security headers, exact origin allowlist, Azure SQL, backups, secrets, observability, and deployment approvals remain to be configured. |
| Data retention | Drafts expire after 24 hours; a bounded hosted worker removes photos and rows, with retries. | Exercise actual expiry/restart/retry behavior and monitor failures. Retention and backup policy must also cover production database backups and photo storage. |

## Local form recovery behavior

The client stores only editable form text and numeric values needed to restore the current create/edit form. It does not store tokens, passwords, uploaded image bytes, or the browser file-picker selection. Values are scoped to the signed-in email via a hashed session-storage key and are cleared on sign-out. This is a convenience cache, not the system of record; a successful server save remains authoritative.

Users sharing a browser profile should sign out when finished. Closing the tab ends session storage. If storage is disabled or unavailable, the application should continue to work without recovery.

## Before public access or production

1. Add paid-access entitlement enforcement, account/email recovery, seller/admin role policy, and usable MFA/passkey protections.
2. Run and review dependency vulnerability scans; fix known vulnerable packages before release.
3. Add authenticated API pipeline and browser tests, including cross-owner, file upload, recovery, sign-out, and quantity cases.
4. Configure secure production response headers, HTTPS/HSTS, exact CORS, reverse-proxy trust, production database, the separate protected persistent Data Protection key ring, backups, quotas, logging, and alerts.
5. Select a hosting provider and provision isolated Development and Production resources/secrets. Gate production promotion with required approval.
6. Conduct a deployment-specific security review and an independent security test appropriate to the product's risk.

## References

- [OWASP ASVS 5.0.0](https://asvs.owasp.org/)
- [OWASP SQL Injection Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html)
- [OWASP Cross Site Scripting Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- [OWASP File Upload Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html)
- [NIST SP 800-63B-4](https://pages.nist.gov/800-63-4/sp800-63b.html)
- [Microsoft ASP.NET Core Identity API authorization](https://learn.microsoft.com/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0)
- [Microsoft Blazor WebAssembly security](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/?view=aspnetcore-10.0)
