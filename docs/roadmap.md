# Dawaya roadmap

Work top to bottom, one slice at a time. A slice is finished only when its "Done when" line is true. Tick boxes as you go. Claude Code updates this file at the end of each slice.

## Step 0: Before you open Claude Code (once, about 30 minutes)

- [x] Install: .NET SDK (LTS), Node.js (LTS), pnpm, Git, SQL Server LocalDB or Docker with SQL Server, Claude Code
- [ ] Create the repo `dawaya` on GitHub and clone it
- [x] Create these folders: `docs/`, `server/`, `apps/`, `packages/`
- [x] Save the business rules into `docs/business-rules.md` (all 88 rules, exactly as written)
- [x] Save the ER diagrams into `docs/er-diagrams.md` (the file from earlier)
- [x] Copy this roadmap to `docs/roadmap.md`
- [x] Put `CLAUDE.md` in the repo root
- [x] Unzip `Dawaya.Api-slice1.zip` into `server/` (starter entities and `DbContext`)
- [x] Make the first commit: "chore: project docs and structure"
- [ ] Get an Anthropic API key for later (Slice 6). It goes in server user-secrets, never in the frontend.

## The slices

### Slice 0: Foundation
**Goal:** an empty but production-shaped app that runs on a phone.
- [ ] Monorepo with pnpm workspaces; API project + test project; React PWA
- [ ] SQL Server connection, Identity, first migration applied (uses the starter entities)
- [ ] Swagger/OpenAPI + generated `packages/api-client`
- [ ] ProblemDetails errors, logging (no health data), CORS, health endpoint
- [ ] Design system: Tailwind + shadcn/ui, Arabic font, RTL/LTR switch, i18n set up
- [ ] Large-text and high-contrast modes, persisted, working from the first screen
- [ ] App shell: bottom tabs on phone, sidebar on desktop; installable PWA manifest and icons
- [ ] Reusable `Disclaimer` component (fixed text, BR-23)
- [ ] CI (GitHub Actions): build + test on every push

**BR:** 17 (infrastructure), 23 (component), 33, 35 (principle)
**Done when:** you can open the site on your phone, install it, switch Arabic/English (layout flips), and toggle large text and high contrast.

### Slice 1: Accounts and family profiles
- [ ] Register, log in, refresh tokens, log out, forgot password
- [ ] Guest mode: browse and scan without an account
- [ ] Family profiles CRUD ("Me", "Mom", "Dad"), active-profile switcher
- [ ] Profile details: age group, pregnancy/breastfeeding, allergies (linked to an ingredient when possible)
- [ ] Data export (JSON) and permanent delete-everything (hard delete, including blobs)
- [ ] Tests: ownership checks (a user can never read another user's profile)

**BR:** 4, 5, 6, 7, 36, 38
**Done when:** a user can create three profiles with allergies, export everything, and delete the account so no rows remain.

### Slice 2: Catalog and admin
- [ ] Seed/import script: 30 to 50 real medicines with products, ingredients, leaflet sections, dose rules, warnings
- [ ] Admin area: add/edit medicines, products, content, warnings; mark content verified; unverified label logic
- [ ] Audit log via a `SaveChanges` interceptor (who changed what, when)
- [ ] Recall flag with visible warning; source + last-updated on every content row
- [ ] User "report wrong information" and the admin review queue
- [ ] Tests: unverified content is labeled; audit rows written

**BR:** 3, 13, 14, 15, 40, 41, 42
**Done when:** an admin can add a medicine, verify its text, and every change appears in the audit log.

### Slice 3: Find, view, and manually add medicines
- [ ] Search with Arabic/English autocomplete (normalize Arabic: alef/yaa/taa marbuta, diacritics)
- [ ] Strength and form picker (mandatory before any medicine page)
- [ ] Medicine page in BR-16 order; serious warnings always visible; medical-term explanations; source and date
- [ ] Profile warnings: highlighted for pregnancy/breastfeeding/child/elderly; **red allergy warning first** (BR-25)
- [ ] Dosage section: leaflet dose with the required labels and doctor sentence; child age ranges with pharmacist advice
- [ ] "Limited information" notice for unregistered items; clear "nothing found" state
- [ ] Read aloud in Arabic (Web Speech API behind an interface)
- [ ] Save medicine to a profile; manual entry form (structured fields, notes, plain-Arabic summary, over-max check that skips children, custom medicine path); edit/delete; record how it was added

**BR:** 5, 8 (typing), 11, 12, 16, 17, 18, 19, 20, 21, 22, 24, 25, 34, 79, 80, 81, 82, 83, 84, 85, 86 (entry part), 87, 88
**Done when:** you can search "panadol", pick 500mg tablet, see the page in the right order, and add it to Mom's profile with a summary sentence.

### Slice 4: Schedule, reminders, tracking, duration
- [ ] Daily schedule grouped by time slot (morning/afternoon/evening/bedtime), plain-Arabic dose sentences
- [ ] Doctor's dose vs leaflet dose shown side by side, never called "wrong"
- [ ] Web Push reminders (service worker + `INotificationSender` interface); per-medicine reminder times
- [ ] Mark taken/skipped/delayed with time; missed-dose job (2h rule) and leaflet missed-dose text
- [ ] Caregiver view of family history; adherence history ("taken 26 of 28")
- [ ] Duration (fixed days / end date / ongoing); course-end job; no-sudden-stop note
- [ ] Regimen versioning when a dose is edited
- [ ] Tests: missed-dose window, course end, versioning, timezone (Cairo) edge cases

**BR:** 31, 32, 52, 53, 54, 56, 57, 58, 59, 60, 61, 86 (schedule, reminders, tracking part)
**Done when:** a reminder arrives on your phone, you mark it taken, and history shows it under the right profile.

### Slice 5: Interactions and refills
- [ ] Interaction check at ingredient level, on save and on schedule change; severity display; "no data available" for gaps
- [ ] Major-interaction warning on the schedule that never changes the schedule itself
- [ ] Pack quantity, decrement on "taken" (transactional), undo adds back, run-out estimate
- [ ] Refill notification (default 3 days, adjustable), suppressed if the course ends first
- [ ] Tests: refill math, unit mismatch rejected, interaction pair ordering

**BR:** 28, 29, 30, 55, 62, 63, 64, 65, 66 (notification part), 67
**Done when:** two interacting medicines on one profile show a major warning, and a refill alert fires on the right day.

### Slice 6: AI scanning, prescriptions, and Q&A (highest risk)
- [ ] `IMedicineVisionService` + `ILeafletSimplifier` implementations using the Anthropic API (server-side)
- [ ] Box photo and barcode identification, matched against the database; below 85% show top 3 to confirm; nothing found says so (never guess)
- [ ] Image deletion job after identification unless the user kept it
- [ ] Prescription scan: extracted items with confidence, database match, "unclear" items locked, side-by-side image with bounding boxes, explicit confirmation, one retake, pharmacist recommendation
- [ ] Unclear items need manual confirmed entry before joining the schedule
- [ ] AI simplification pipeline feeding the admin verification queue
- [ ] Q&A: in-scope answers only from database content; out-of-scope decline; emergency mode; controlled-medicine limits; guest rate limiting
- [ ] Test suite of nasty prompts (dose questions, "can I take more", overdose, prompt injection in the image text)

**BR:** 1, 2, 8 (photo/barcode), 9, 10, 12, 13, 26, 27, 37, 43, 44, 45, 46, 47, 48, 49, 50, 51
**Done when:** a blurry prescription produces "unclear" items instead of guesses, and an overdose question shows emergency guidance instead of an answer.

### Slice 7: Pharmacies and orders
- [ ] Pharmacy data + hours; nearby search using a spatial query; area-name search if location denied
- [ ] Map (Leaflet + OpenStreetMap), call and directions buttons; delivery flag
- [ ] Partner pharmacy accounts and order inbox; prescription-only orders require an attached prescription and pharmacist approval
- [ ] Only the chosen order and prescription are shared with the pharmacy
- [ ] Order status tracking and notifications; refill notification actions (order / call / dismiss)

**BR:** 66, 68, 69, 70, 71, 72
**Done when:** a partner pharmacist can approve or reject an order, and a non-partner pharmacy only offers call and directions.

### Slice 8: Medical records and QR sharing
- [ ] Upload records (PDF/image) with type and date; virus/size/type validation; private blob storage
- [ ] Generate share link: choose records, choose expiry; QR contains only a secure link (raw token never stored, only its hash)
- [ ] Read-only doctor view (no login), auto-expiry, revoke, access log the user can see
- [ ] No AI interpretation of results (stated in the UI)

**BR:** 73, 74, 75, 76, 77, 78
**Done when:** a doctor opens the QR link on their phone, and after you revoke it, the same link fails.

### Slice 9: Hardening and launch
- [ ] Verify Law No. 151 of 2020 requirements for health data with a qualified person (BR-39)
- [ ] Security review: authorization on every endpoint, rate limits, upload limits, secrets, headers, dependency audit
- [ ] Accessibility audit with real elderly users; Arabic copy reviewed by a native speaker
- [ ] Pharmacist or doctor reviews the seeded medicine content and the safety wording
- [ ] Performance: indexes, image sizes, Lighthouse PWA score, offline shell
- [ ] Backups, monitoring, error tracking, environments (dev/staging/prod), Docker deployment
- [ ] Terms, privacy policy, and in-app disclaimer screens
- [ ] End-to-end tests (Playwright) for the main flows

**BR:** 39 and everything above re-checked
**Done when:** a stranger can sign up on their phone and use the main flows without help, and you can restore a backup.

## Mobile apps (after the website)

Pick one path when the web app is stable:
- **Fastest:** wrap the PWA with **Capacitor** for Android and iOS. Native push, camera and barcode via plugins. Almost no new UI code.
- **Most native feel:** **React Native / Expo** app in `apps/mobile`, reusing `packages/api-client`, your types and the same backend.

Either way, the backend stays unchanged because the API, JWT auth and notification abstraction are already mobile-ready. Add: FCM/APNs sender, deep links (for QR share links), store listings, and a privacy nutrition label.

## Cross-cutting checklist (do not forget)

- [ ] Every screen works in Arabic (RTL), English, large text and high contrast
- [ ] Every endpoint checks that the caller owns the profile or record
- [ ] No health data, tokens or images in logs
- [ ] Every AI feature has a "not sure" path and a test for it
- [ ] Every rule-based calculation has unit tests
- [ ] Every migration is reviewed before applying
- [ ] Every slice ends with build, tests, lint, and a roadmap update
- [ ] Every content page shows disclaimer, source and date
- [ ] Every 3 slices: manual test on a real phone

## Coverage check

Every rule BR-01 to BR-88 appears in a slice above. At the end of each slice, ask Claude Code: "List every BR in this slice, and for each show the file and test that implements it."
