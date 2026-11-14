# Plan vendor#03: Vendor directory capability (other modules read vendors)

- **File:** `docs/plans/vendor/2026-11-14-03-vendor-directory.md`
- **Kind:** Backend
- **Spec:** [../../specs/vendor.md](../../specs/vendor.md) - rules covered: R15; AC13
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** `vendor#01` (Done)
- **UI plan:** none, it adds no screen or endpoint: it is a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-14 (chat: "all approved hai kro implement")
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** `feature/vendor-01` (cut from it: vendor#03 builds on vendor#01)

## 1. In one minute
After this plan another module (the item master) can ask the vendor module three things without touching its tables: does this vendor exist in my company and is it active, what are the names of these vendors, and which vendors match this text. Nothing visible changes; item#01 is the first user. You can see it working through the check script that calls the capability for the Owner of a company.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Vendor` (a capability `IVendorDirectory` with three read methods and its small result records), `BookInventory.Infrastructure/Vendor` (its implementation on the existing store, no new table), `docs/Code_Map.md`.
Leaves out: any endpoint, any change to a vendor's data, any UI.
Schema change: no.

## 3. Approach
`IVendorDirectory` lives in the vendor module's Application layer and is implemented in its Infrastructure over the same repository and company filter as `VendorStore`, so it answers only for the session's company (P19). Methods: `GetAsync(id)` returns the vendor's id, name and active flag or null; `NamesAsync(ids)` returns id and name pairs; `IdsMatchingAsync(text, limit)` returns the ids of vendors whose name or code contains the text, capped at the limit (200). It returns small records, never the aggregate.

## 4. Tasks (at most 8)
- [x] 1. `IVendorDirectory` and its records in `BookInventory.Application/Vendor` (R15).
- [x] 2. The implementation in `BookInventory.Infrastructure/Vendor` over the company-filtered repository (R15).
- [x] 3. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 4. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R15`, `AC13` | 1, 2 | A throw-away call (scratch code, not committed) inside the running API for the Owner of A: `GetAsync` of A's vendor -> id, name, active; of B's vendor id -> null; `NamesAsync` of two ids -> both names; `IdsMatchingAsync("alpha", 200)` -> A's matching vendors only, at most 200; for the Owner of B the same calls see only B's vendors **Observed (2026-11-14, real `book_inventory`, a scratch controller on a second API instance on port 7200, 8 checks, all pass; the controller was deleted afterwards):** A's vendor -> id, name `V01 Alpha Publications`, active true; A's inactive vendor -> active false; B's vendor id asked by A -> null; `NamesAsync` of A's two ids and one of B's -> only A's two names; `IdsMatchingAsync("alpha")` as A -> A's vendor only, as B -> B's vendor only; `"v01"` as B -> 11 ids, none of A's, at most 200; blank text -> empty list. |

## 6. Not verified
- The 200 cap was not exercised with 200+ vendors (only the `Take(200)` in the code and 11 matches observed).
- No migration: this plan changes no schema.

## 7. Risks and notes
The check needs scratch code that calls the capability; item#01 exercises it end to end afterwards (vendor on an item, search by vendor name), which is the real proof.
