# Plan 0012: iam - configurable PBKDF2 work factor

- **Number:** 0012 (global serial)
- **Module:** `iam` (primary) ; also touches: none
- **Spec:** [docs/specs/iam.md](../specs/iam.md) - R16, R54
- **Status:** Done
- **Created:** 2026-09-27   **Completed:** 2026-09-27

## 1. Goal

The PBKDF2 iteration count of new password hashes is a validated setting (`Auth:Password:Iterations`, default 210,000), so scripted checks that log in hundreds of times can run with a low cost while every real deployment keeps the OWASP value. Requested by the owner (2026-09-27) to shorten verification runs.

## 2. Out of scope

Changing the algorithm or the hash format, a per-user cost, and any change to the default.

## 3. Approach

- `AuthOptions.Password.Iterations`: default 210,000, range 1,000-2,000,000 (startup validation, R54). `Pbkdf2PasswordHasher` reads it instead of a constant; the stored format already carries the cost, so old hashes keep verifying and a hash with a lower cost is re-hashed at the next login (existing `SuccessRehashNeeded` path, R16).
- A value below the default is logged as a warning at every start ("use only for tests"), so it cannot hide in a deployment.
- The spec's R16 states the setting and the rule.

## 4. Tasks

- [x] 1. Option, hasher wiring, startup warning
- [x] 2. Spec R16/R54 text
- [x] 3. Verify by hand (section 5)

## 5. Verification

Real API against a throw-away database (`iter-check.sh`, not committed), build 0 warnings, 0 errors:

- `Auth__Password__Iterations=10` -> startup stops: `OptionsValidationException ... 'Iterations' ... must be between 1000 and 2000000`.
- `Auth__Password__Iterations=1000` -> the seeded hash starts `$pbkdf2-sha512$1000$`, the warning `Auth:Password:Iterations is 1000, below the default 210000: new password hashes are weaker` is logged, five logins take 0.67 s in total.
- Restart with the default: login with the same password answers `200` and the stored hash is now `$pbkdf2-sha512$210000$` (the 1000-iteration hash verified and was upgraded).

**NOT verified:** behaviour at the upper bound (2,000,000 iterations would make each login very slow; the bound only guards against a typo).

## 6. Risks and notes

- A low value in production weakens new hashes. Mitigation: the startup warning, the release checklist (Plan 0010) should list `Auth:Password:Iterations` as "unset or 210000".
