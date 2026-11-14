# Spec: price - Price management

- **Module:** `price` (PostgreSQL schema `price`)
- **Status:** R1-R17 Approved by abhishek in chat on 2026-11-14 (Q1-Q12 answered one by one, all with the recommended answers; D1-D5 with them)
- **Last updated:** 2026-11-14
- **Source docs:** [BRD §1.2, §3 (who may change prices), §4.5 Price Management (FR-PRC-01 to FR-PRC-08), §5.2 BR-PRC, §5.3 BR-DIS, §7.1, §8 Data Model (ItemPriceVersion)](../bussiness/BRD-Book-Inventory-Phase1.md); [specs/platform.md](platform.md) P18-P23; [specs/company.md](company.md); [specs/item.md](item.md); [specs/core.md](core.md) (the audit trail it reuses for history)
- **Plans:** [price#01](../plans/price/2026-11-14-01-price-versions.md) (Done), [price#02](../plans/price/2026-11-14-02-ui-price-versions.md) (Approved), [price#03](../plans/price/2026-11-14-03-bulk-price-change.md) (Approved), [price#04](../plans/price/2026-11-14-04-ui-bulk-price-change.md) (Approved); the capability it needs from item: [item#03](../plans/item/2026-11-14-03-item-directory.md) (Approved)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`price` holds the prices of the company's items. An item has several **price versions** (for example `2025-26` and `2026-27`); each version has an MRP, a sale price and a purchase price and the date from which it applies. Orders, invoices, counter bills and returns of later modules look a price up here and then **copy** the rate into the document (BR-PRC-03): a later price change never touches a document that was issued.

Features: **Price versions** (R1-R9): the Owner adds and edits the versions of an item, the newest applicable version is the default, and a whole version can be copied to a new one for the new year. **Bulk price change** (R10-R14): the Owner chooses a group of items, raises or lowers the prices by a percentage or an amount, looks at a preview and then applies it. **History and visibility** (R15-R17): every change is traceable and Staff see only what they need to sell.

Out of scope for these features: Excel upload of prices (FR-PRC-05, NFR-12: its own later feature, it needs an Excel package approved, Q10); the purchase warning "the vendor's rate differs, make a new version?" (FR-PRC-08: it belongs to the Purchase module, which will call this module); choosing a version on an order or invoice line (the Order modules); discounts and GST on a document (BR-DIS, BR-GST: later modules, the item's GST rate is in [specs/item.md](item.md)); stock. The Super Admin's price screens in the `admin` app (Q11).

## 2. Concepts and glossary

- **Price version**: one named set of prices of one item, with the date it applies from (R2).
- **MRP**, **sale price**, **purchase price**: the printed maximum price, the rate the company sells at and the rate it buys at (FR-PRC-01).
- **Default version**: the version a new document line starts with (R4).
- **Business date**: "today" is the date in the business time zone `Asia/Kolkata` ([specs/company.md](company.md) D7); amounts are rupees.

## 3. Business rules

**Price versions**
- **R1** A price version belongs to exactly one item and one company, the company of the session. The item must exist in the company (active or not) and is read through the item module's directory capability ([specs/item.md](item.md) R16), never by joining its tables; a version of another company's item is refused as an unknown item (`404`). Another company's versions are never read or changed (platform P18-P21).
- **R2** A version has: **version name** (required, 2-30 characters, for example `2026-27`), **effective from** (a date in the business time zone, required), **MRP**, **sale price** and **purchase price** (each required, zero or more, `numeric(18,2)`, at most two decimals) (FR-PRC-01). Values with more decimals are refused, never rounded silently. The version name is unique inside the item ignoring case and extra spaces (`409`).
- **R3** The Owner adds a version to an item and edits any version of it (FR-PRC-03). Editing changes the values in place; the old values stay in the history (R15). A version is never deleted (Q6). Documents already issued are not touched by an edit (BR-PRC-03, R14).
- **R4** The **default version** of an item is the version with the latest *effective from* date that is not after the business date; when two versions share that date the one created later wins; an item with no such version has no default (BR-PRC-02, FR-PRC-02) (Q2). A later module may let the user pick another version of the item for a line or type a rate (FR-PRC-02); that choice is made in the document, not here.
- **R5** Reading prices: the Owner reads every field of every version; **Staff read the version name, the effective date, the MRP and the sale price** but never the purchase price (the purchase price comes back empty) (Q4). A Super Admin reads and changes prices only while they have entered a company ([specs/company.md](company.md) R13); with no company entered there is nothing to read and a write is refused (platform P19, P20). Anyone else is refused (`403`, anonymous `401`).
- **R6** Only the Owner (and an entered Super Admin) changes prices (BRD §3: "Price badalna: Haan / Nahi" for Staff). Staff's writes answer `403`.
- **R7** Sale price and purchase price may be above or below the MRP; the only value rule is R2 (Q5).
- **R8** **Copy a version** (FR-PRC-06): the Owner chooses a source version name, a new version name and an effective date, and optionally the same item selection as a bulk change (R10); for every selected item that has the source version and does not already have the new name, a new version with the same MRP, sale price and purchase price is created. Items without the source version or that already have the new name are skipped and counted, never overwritten. The answer says how many were copied and how many skipped (and why). One request is all or nothing.
- **R9** The list of prices shows one row per item with its **default** version (name, date, MRP, sale price and, for the Owner, purchase price), paged (page size capped at 200), searchable by item name, item code and barcode and filterable by category, vendor and class through the item directory, and sortable by item name and item code (BRD §7.1: every list has search and filter).

**Bulk price change**
- **R10** The Owner selects items by any mix of **vendor**, **category**, **class**, or the whole list, optionally only active items, and names the version to change (FR-PRC-04) (Q3).
- **R11** For each of MRP, sale price and purchase price separately the Owner may give an **increase or decrease** as a percentage or as an amount in rupees, or leave it unchanged (FR-PRC-04). A percentage is of the current value of that same field of that same version. The new value is rounded to two decimals, halves away from zero (Q1, the owner's answer: `2.525` becomes `2.53`). A result below zero is not applied: that item is listed in the preview as "would go below zero" and skipped.
- **R12** A **preview** computes, for every selected item that has the named version, the old and the new value of each changed field and the items that will be skipped (no such version, or below zero); it **saves nothing** (FR-PRC-04). The preview is paged and shows the totals (items changed, items skipped).
- **R13** **Apply** recomputes the change on the server from the same selection and values (it never trusts numbers sent back from the preview) and edits the named version of every changed item in one transaction: all or nothing. Each edit is recorded in the history (R15). A request to apply with a selection that matches no item is refused (`400`).
- **R14** A bulk change or a copy never touches a document already issued (BR-PRC-03), and only the selected items' versions change.

**History and visibility**
- **R15** The **history** of a version (when, who, what changed: old and new values) is the audit trail of `core` for that version ([specs/core.md](core.md) A1-A9); this module adds no history table of its own. The Owner reads it; Staff do not.
- **R16** Every create and change records who and when through the standard audit columns and the audit trail (R15).
- **R17** Money: all amounts are exact decimals with two places; percentages are applied with exact decimal arithmetic, never floating point; the rounding of R11 happens once, at the end, per field.

## 4. Data

Schema `price`:
- `price_version` (aggregate root, company-owned, platform P18): `company_id` (not null), `item_id` (plain id, no foreign key across schemas), `version_name`, `version_name_normalized`, `effective_from` (`date`), `mrp_amount`, `sale_amount`, `purchase_amount` (each `numeric(18,2)`, not negative), plus the standard audit and concurrency columns. Unique index on `(company_id, item_id, version_name_normalized)` among live rows; index on `(company_id, item_id, effective_from)`.

`company_id` and `item_id` are plain ids; the `company` and `item` modules guarantee they exist. A version is a root of its own (not a child of an item) so that a bulk change edits many small aggregates, each saved through its own root.

## 5. API contract (`/api/v1`)

All routes are company-scoped by the session (platform P19, P20); the company is never a request field.

| Method + route | Who | Notes |
|---|---|---|
| `GET items/{itemId}/prices` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | the item's versions, newest first, with the default marked (R4); Staff get no purchase price (R5); `404` for an unknown item |
| `POST items/{itemId}/prices` | `OWNER`, `SUPER_ADMIN` (entered) | `{versionName, effectiveFrom, mrpAmount, saleAmount, purchaseAmount}` -> `201`; `400` for invalid values; `409` same name |
| `PUT items/{itemId}/prices/{versionId}` | `OWNER`, `SUPER_ADMIN` (entered) | edits R2 values (not the item) |
| `GET prices` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | the list of R9 with the default version of each item |
| `POST prices/copy` | `OWNER`, `SUPER_ADMIN` (entered) | `{fromVersionName, toVersionName, effectiveFrom, selection}` -> counts copied and skipped (R8) |
| `POST prices/bulk/preview` | `OWNER`, `SUPER_ADMIN` (entered) | `{versionName, selection, mrp?, sale?, purchase?}` where each change is `{kind: PERCENT or AMOUNT, direction: INCREASE or DECREASE, value}`; paged preview, nothing saved (R12) |
| `POST prices/bulk/apply` | `OWNER`, `SUPER_ADMIN` (entered) | same body; applies R13; answers the counts |

`selection` is `{vendorId?, category?, className?, onlyActive?}`; empty means the whole list. Errors are `ProblemDetails` with a message code from the module's catalogue. Validation failures are `400`.

## 6. User interface

Written in the UI plans; summary: in the company workspace (`member` app) the Owner and Staff get a "Prices" menu in its own group: the price list with search and filters; an item's prices open beside the list with an X to close them (platform U10); the Owner adds and edits versions there and reads each version's history; a "Copy version" dialog and a "Bulk change" screen with its preview. Staff see the list and the versions read-only without purchase prices. Each state shows loading, empty, error with retry and success.

## 7. Events

Raised by the `PriceVersion` aggregate (past tense, ids and values only): `PriceVersionAdded`, `PriceVersionChanged`. Reactions: none in these features.

## 8. Dependencies

`price` depends on `iam` for the current company, on `core` for the audit trail and the code of platform P18-P22, and on `item` through its directory capability ([specs/item.md](item.md) R16, plan item#03). Documents of later modules call this module to look a price up and then copy it.

## 9. Acceptance criteria

- **AC1** The Owner of company A adds version `2026-27` to item X with effective date 2026-04-01, MRP 125.00, sale 112.50, purchase 100.00: `201`; the row's `company_id` is A; the same name again (also `2026-27 ` or `2026-27` in other case) is `409`; a negative value, a value with three decimals, a name of one character and an unknown item are `400` or `404` (R1, R2).
- **AC2** Item X with versions `2025-26` (from 2025-04-01) and `2026-27` (from 2026-04-01): on 2026-11-14 the default is `2026-27`; a version `2027-28` from 2027-04-01 is not the default yet; two versions with the same date, the later created wins; an item with only a future version has no default (R4).
- **AC3** Editing a version changes the values and keeps the version name unique; the old values appear in its history; no version can be deleted (R3, R15).
- **AC4** The Owner of B listing or opening A's item prices gets nothing or `404`, identical to an unknown item; a `companyId` in the query or body changes nothing (R1).
- **AC5** Staff reading an item's prices get names, dates, MRP and sale price and an empty purchase price; Staff `POST`, `PUT`, copy, bulk preview and bulk apply answer `403`; an anonymous caller `401` (R5, R6).
- **AC6** A Super Admin with no company entered gets no prices and a write answers `403 core.company_required`; after entering A they read and can add a version (R5).
- **AC7** The price list is paged and capped at 200; search by item name, item code and barcode, the category, vendor and class filters and the sorts work; an unknown sort column is ignored; each row shows the default version (R9).
- **AC8** Copy `2025-26` to `2026-27` from 2026-04-01 for the items of one vendor: items with `2025-26` get an identical `2026-27`; items without it and items that already have `2026-27` are skipped and counted; nothing is overwritten (R8).
- **AC9** Bulk preview for the items of one category, MRP +10 percent and purchase price -5 rupees: the preview lists old and new values per item, the skipped items with the reason, and the totals; the database is unchanged afterwards (R10, R11, R12).
- **AC10** Bulk apply with the same body changes exactly the previewed items to the previewed values, in one transaction, and each changed version has a history entry; an item whose result would be below zero is skipped; a selection that matches no item is `400` (R11, R13).
- **AC11** Money, worked out independently first (rounding of Q1: halves away from zero): MRP 2.50 with +1 percent is 2.525 and becomes 2.53; 100.00 with -33.33 percent is 66.67; 125.00 with +10 percent is 137.50; 10.00 with an amount decrease of 10.01 would be -0.01 and is skipped; 99.99 with +0 percent stays 99.99; the maximum `numeric(18,2)` value with +1 percent is refused (overflow) rather than stored wrong (R2, R11, R17).
- **AC12** The history of a version, read by the Owner, shows creation and each edit with the actor, the time and the old and new values; Staff cannot read it (R15, R16).

## 10. Decisions

| # | Decision |
|---|---|
| D1 | New module `price` with its own schema. A price version is a company-owned aggregate root of its own, pointing at its item by id. (proposed by the assistant) |
| D2 | History is the audit trail of `core`, not a table of this module, so there is one history mechanism (R15). (proposed by the assistant) |
| D3 | The items of a price are read through the item module's directory capability (item R16), never by joining. (proposed by the assistant) |
| D4 | A bulk change or a copy runs in one request transaction over many small aggregates, all or nothing; the aggregate rule "one transaction per aggregate" is not broken because each version is still changed through its own root. (proposed by the assistant) |
| D5 | Versions are never deleted: a mistake is fixed by editing, and the history keeps what was there. (proposed by the assistant, Q6) |

## 11. Open questions

None. Q1-Q12 were answered by the owner one by one on 2026-11-14, all with the recommended answers (Q1: half away from zero at two decimals; Q2: latest effective date not after today; Q3: bulk change edits the named version); the rules above follow them. The list keeps the questions with the answer taken.

- **Q1** **Rounding of a percentage change (a business decision).** Recommended: round half away from zero at two decimals (`2.525` becomes `2.53`), applied once per field at the end (R11, R17). Alternative: round half to the even digit (`2.525` becomes `2.52`), or round to the whole rupee.
- **Q2** "Newest version" (BR-PRC-02). Recommended: the version with the latest effective date that is not after today in India time; a future-dated version waits for its date (R4). Alternative: the version created last, whatever its date.
- **Q3** What a bulk change edits. Recommended: it edits the named existing version of every selected item (R10, R13), the history keeps the old values; to start a new year the Owner first copies a version (R8) and then changes the copy. Alternative: it creates a new version for every selected item.
- **Q4** What Staff may see. Recommended: MRP and sale price, never the purchase price (R5). Alternative: no prices at all for Staff.
- **Q5** Sale price against MRP. Recommended: no relation enforced, only "zero or more" (R7). Alternative: refuse a sale price above the MRP.
- **Q6** Deleting a version. Recommended: never (D5). Alternative: allow deleting a version that no document used (needs the documents module).
- **Q7** Items with no version. Recommended: an item with no default simply shows no price in the list; documents of later modules refuse a line with no price unless the user types a rate (FR-PRC-02).
- **Q8** Inactive items. Recommended: the Owner may still add and edit versions of an inactive item (R1); the price list shows them under a filter.
- **Q9** Version name format. Recommended: free text of 2-30 characters, no format check, because the BRD gives `2026-27` only as an example (R2).
- **Q10** Excel upload (FR-PRC-05). Recommended: a later feature, after an Excel library is approved, together with the school, vendor and item imports.
- **Q11** The Super Admin's price screens in the `admin` app. Recommended: later, like the other masters.
- **Q12** Copy and bulk selection by class. Recommended: the item's class text must equal the given class ignoring case (the item's class is free text, item Q6); a class picker comes with the planning module.
