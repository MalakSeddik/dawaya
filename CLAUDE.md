# Dawaya

Dawaya is a medication companion. Users scan or type a medicine, read a simplified explanation of its official leaflet (Egyptian Arabic first, English available), manage family profiles, follow a dose schedule with reminders, check interactions, get refill alerts, find pharmacies, and share medical records with a doctor by QR code.

It ships as a **responsive, installable web app (PWA) first**. Android and iOS apps come later and must reuse the same API. Every decision below protects that.

## Source of truth (read before any task)
- `docs/business-rules.md`: all 88 business rules (BR-01 to BR-88). If code and a rule disagree, the rule wins. Ask me before changing a rule.
- `docs/er-diagrams.md`: the database design. Ask me before changing the schema in a way that departs from it.
- `docs/roadmap.md`: the slices, in order. Work on one slice at a time and tick items off as they are done.

## Stack
- **Server:** ASP.NET Core Web API, EF Core, SQL Server, ASP.NET Identity. Use the .NET LTS installed on this machine.
- **Web:** React + TypeScript + Vite, Tailwind CSS, shadcn/ui (Radix), TanStack Query, React Router, react-hook-form + zod, i18next, vite-plugin-pwa.
- **Monorepo (pnpm workspaces):**
  ```
  server/Dawaya.Api/        ASP.NET Core API (feature folders)
  server/Dawaya.Tests/      xUnit tests
  apps/web/                 React PWA
  packages/api-client/      TypeScript client generated from the API's OpenAPI spec
  apps/mobile/              (later) React Native/Expo or Capacitor
  docs/
  ```

## Architecture rules (these keep the mobile apps cheap later)
1. **API first.** Every feature is an API endpoint under `/api/v1/...` before it is a screen. The web app is just one client.
2. **Business logic lives on the server** (interaction checks, refill math, missed-dose window, dose-limit check, warnings, rate limits). The frontend only displays results.
3. **Auth is JWT access token + refresh token**, not cookie-only, so mobile clients work identically.
4. **OpenAPI is the contract.** Generate `packages/api-client` from it. Never hand-write API types in the web app.
5. **Wrap platform features behind small interfaces** (push notifications, file/camera capture, storage, text-to-speech), so a mobile app can swap the implementation.
6. **Storage and AI behind interfaces:** `IBlobStorage` (local disk in dev, cloud in prod, access via short-lived signed URLs) and `IMedicineVisionService` / `ILeafletSimplifier` (AI provider is replaceable). API keys are server-side only, never in the frontend.
7. **Background work** (missed doses, refill alerts, course end, image cleanup, share-link expiry) runs as hosted services or Hangfire jobs, never inside request handlers.
8. All dates are UTC in the API (ISO 8601); convert to Africa/Cairo only in the UI.
9. Return consistent errors (RFC 7807 ProblemDetails) and validate all input (FluentValidation or DataAnnotations).

## UI rules
- **Mobile-first**, then tablet/desktop. Bottom tab bar on phones, sidebar on desktop. Touch targets at least 44px.
- **RTL is a first-class layout**, not an afterthought. Use CSS logical properties (`ms-`, `me-`, `ps-`, `pe-`, `start`, `end`), never left/right. Default language is Arabic (Egyptian), with English available. No hard-coded strings; everything goes through i18n resource files.
- Large-text mode and high-contrast mode are available from the very first screen and persist (BR-33). Build them on CSS variables and rem units.
- Main flows (scan, view, remind) work in 3 taps or fewer (BR-35).
- Every medicine page shows the fixed disclaimer (BR-23), the source and last-updated date (BR-14), and an "unverified" label where applicable (BR-15). Serious warnings are never hidden behind "read more" (BR-18).
- Accessible by default: labels, focus order, contrast, screen-reader text, keyboard support.

## Medical safety rules (non-negotiable)
- The AI **never** states a medical fact that is not in the medicine database or official leaflet. It simplifies and translates; it does not add facts (BR-09, BR-13).
- Never guess. If a name, strength or dose is unclear or low-confidence, say so and send the user to a pharmacist or doctor (BR-12, BR-49).
- Never calculate, suggest, adjust, raise, lower or stop a dose. Show the leaflet's usual dose, or the doctor's dose the user confirmed (BR-19 to BR-21, BR-54). Never suggest a double dose (BR-57).
- Missing interaction data displays "no data available", never "safe" (BR-30).
- Anything extracted by AI needs explicit user confirmation before it is saved (BR-46).
- Overdose, poisoning, or serious-reaction questions stop normal answering and show emergency guidance: 123 and poison control (BR-26).
- Out-of-scope questions ("what should I take for...") are politely declined with a pharmacist/doctor referral (BR-02).
- Scanned images are deleted after identification unless the user chooses to keep them (BR-37).
- Health data is encrypted in transit and at rest; never log health data, tokens or images; share-link tokens are stored hashed (BR-36, BR-74).

## Conventions
- C#: nullable enabled, feature folders (`Features/Catalog`, `Features/Profiles`, ...), one file per entity/config, async all the way, no business logic in controllers.
- TypeScript: strict mode, no `any`, feature folders in `apps/web/src/features/*`, server state via TanStack Query only.
- Migrations: one migration per slice, named clearly. Never edit a migration that has been applied.
- Secrets: user-secrets / environment variables only. Never commit them.
- Commits: small, one logical change each, conventional messages (`feat:`, `fix:`, `test:`, `docs:`).

## Workflow for every task
1. **Plan first.** For anything bigger than a small fix, propose a short plan and wait for my approval.
2. Build the API endpoint(s) with tests, then regenerate the api-client, then the UI.
3. Write unit tests for every rule-based logic (interactions, refill math, missed-dose window, dose limit, warnings, rate limits, share-link expiry).
4. Run build, tests and lint before saying a task is done. Fix failures yourself.
5. Tick the completed items in `docs/roadmap.md` and list which BR numbers the work covers.
6. Tell me anything you assumed, and anything a business rule leaves unclear, instead of silently deciding.

## Commands (update as they are created)
- Server: `dotnet build`, `dotnet test`, `dotnet run --project server/Dawaya.Api`
- Web: `pnpm install`, `pnpm --filter web dev`, `pnpm --filter web build`, `pnpm --filter web test`
- Client regen: `pnpm --filter api-client generate`
- Migrations: `dotnet ef migrations add <Name> --project server/Dawaya.Api`, then `dotnet ef database update`

## Decisions already made
- Saving a medicine without a regimen is `Status = Saved`; it becomes `Active` when a regimen is added.
- Custom (not in database) medicines get reminders and refill alerts, but no leaflet content and interaction checks say "no data available".
- Pack quantity and dose must use the same unit (`MedicineProducts.UnitOfCount`).
- The max-daily-dose typo check (BR-85) is skipped for Infant and Child profiles; show "confirm with your pharmacist" instead.
- A dose becomes `Missed` after 2 hours, or halfway to the next dose if that is shorter.
- Guests may ask AI questions, rate-limited per device/IP; registered users get a higher limit.
- Roles use ASP.NET Identity roles (User, Admin, Pharmacist).
