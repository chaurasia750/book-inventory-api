# Deployment notes: first start and PDF on the shared IIS server

Server: Windows + IIS + Plesk, site `bi.bankatm.co.in`, application pool `bi.bankatm.co.in(domain)(4.0)(pool)` (user `IWPD_130(BankAtm)`). Run every command in an **elevated** PowerShell on the server. Read together with [Release_Checklist.md](Release_Checklist.md).

Every step below was hit on the first deployment (2026-11-16) and is what fixed or was tried for it. Do them once per new server or new site.

## 1. Settings the app needs to start

| Symptom in the log | Cause | Fix |
|---|---|---|
| `OptionsValidationException ... 'LinkUrl' ... is required` | `Auth:Reset:LinkUrl` has no value in Production | Set in `appsettings.Production.json` (`https://<host>/auth/reset-password`). Already in the file; change the host if the site name changes. |
| `42501: permission denied for database ...` on `CREATE SCHEMA iam` | The database user cannot create schemas; the app migrates on start | As a PostgreSQL superuser: `ALTER DATABASE "Live_BookInventory" OWNER TO "BookInventory";` (or at least `GRANT CREATE ON DATABASE ... TO "BookInventory";`). Then restart the pool. |

Secrets (connection string, JWT key, PAN / bank keys, seed passwords) are pool environment variables set by `scripts/set-prod-env.ps1`; never put them in a file.

## 2. PDF export

The app prints PDFs with a headless Chrome or Edge already installed on the server (platform spec P25). Nothing is downloaded.

1. **Browser path.** `appsettings.Production.json` has `Documents:Pdf:BrowserPath`. It must be the **exe file**, never a folder: `C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe`. When it is empty the app looks for Chrome, then Edge. A pool variable `Documents__Pdf__BrowserPath` overrides the file.
2. **Load User Profile.** The pool must have it on (the other working site on this server, `hautinfra.com`, has it on; the default is off):
   ```powershell
   Import-Module WebAdministration
   Set-ItemProperty "IIS:\AppPools\bi.bankatm.co.in(domain)(4.0)(pool)" -Name processModel.loadUserProfile -Value $true
   Restart-WebAppPool -Name "bi.bankatm.co.in(domain)(4.0)(pool)"
   ```
   Check: `(Get-ItemProperty "IIS:\AppPools\bi.bankatm.co.in(domain)(4.0)(pool)").processModel.loadUserProfile` prints `True`. The log warning "Neither user profile nor HKLM registry available" must be gone. Plesk can reset this setting when it reconfigures the pool: look here first if PDF fails again.
3. **Permission on the browser folder.** The pool's own user must be able to read and run the browser:
   ```powershell
   icacls "C:\Program Files (x86)\Microsoft\Edge\Application" /grant "IWPD_130(BankAtm):(OI)(CI)RX"
   Restart-WebAppPool -Name "bi.bankatm.co.in(domain)(4.0)(pool)"
   ```
   Undo: `icacls "C:\Program Files (x86)\Microsoft\Edge\Application" /remove "IWPD_130(BankAtm)"`.
4. **Memory.** At most `Documents:Pdf:MaxConcurrentRenders` (default 2) browsers run at once; each takes roughly 100-200 MB. Raise it only if the server has memory to spare.

### The error this fixes

`Failed to launch browser! ... msedge.exe ... side-by-side configuration is incorrect (14001)` on `GET /api/v1/invoices/{id}/pdf` (HTTP 500). It is raised by Windows when it starts the browser, before any app code or browser flag, so it is not a code bug. The Windows `SideBySide` event log had no entry for it, and Edge itself is installed and works for another site on the same server, so the cause is how this pool's user starts it (profile, permission), not the Edge install.

**Status of the PDF fix:** steps 2.2 and 2.3 were applied on 2026-11-16 (the profile warning disappeared after 2.2; 14001 was still there after it). The result after 2.3 was **not yet confirmed** when this note was written: update this line with the outcome. If it still fails: read the `SideBySide` events again, compare the pool with the working site (`Get-ChildItem IIS:\AppPools`, pool variables `Pdf__*`), and as the safest alternative install Google Chrome (64-bit) and set `Documents__Pdf__BrowserPath` to its exe.

## 3. Check after every deployment

- `GET /health` answers `200`; the log shows `Database schema is up to date` and no `OptionsValidationException`.
- Open an invoice and download its PDF: `200`, a real PDF, no `Document render failed` entry in the log.
