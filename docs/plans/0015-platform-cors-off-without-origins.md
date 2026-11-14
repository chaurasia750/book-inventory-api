# Plan 0015: platform - no CORS headers when no origin is configured

- **Number:** 0015
- **Module:** `platform` (primary) ; also touches: none
- **Spec:** [docs/specs/platform.md](../specs/platform.md) - P-rules on the API host
- **Status:** Done
- **Created:** 2026-09-27   **Completed:** 2026-09-27

## 1. Goal

The API sends no CORS header at all unless `Cors:AllowedOrigins` lists origins (owner decision on backlog B40). Same-origin hosting needs none, and no other website can read an answer.

## 2. Out of scope

B43 (the field code of a unique-violation race), B4, B6 and B29, which stay in the backlog.

## 3. Approach

`CorsPolicyExtension` (BuildingBlocks): with no configured origin the CORS middleware is not added; with origins the policy is unchanged (those origins, with credentials). One method changed, its documentation rewritten.

## 4. Tasks

- [x] 1. `UseCorsPolicy` skips the middleware when no origin is configured
- [x] 2. Verify by hand (section 5); spec, checklist, backlog

## 5. Verification

Same run as Plan 0014 (`api-check13.sh`):

| Environment | Input | Observed |
| --- | --- | --- |
| Development (`Cors:AllowedOrigins` = the UI dev origin) | request with `Origin: http://localhost:4200`; with `Origin: https://evil.test` | `Access-Control-Allow-Origin: http://localhost:4200` and `Access-Control-Allow-Credentials: true`; no CORS header |
| Staging (no origin configured) | request with `Origin: https://evil.test`; a preflight `OPTIONS` on the login route | no CORS header; the preflight answers `405` with no CORS header |

**NOT verified:** a browser refusing a cross-origin read against the real host (the absence of the header is what does that).

## 6. Risks and notes

- A developer who calls the API from another origin (another dev port without the proxy, a tool in a browser) now has to list that origin in `Cors:AllowedOrigins`; the shared UI dev servers proxy `/api` and are unaffected.
