# Repository Guidelines

## Project Structure & Module Organization
- `backend/`: ASP.NET Core Minimal API (C#) and SQLite access.
- `frontend/`: Vue 3 (Vite) SPA with PrimeVue, Pinia stores, and pages.
- `Database.db`, `script.sql`, `seed.sql`: SQLite database and schema/seed scripts.
- `Docs/`: project notes and assignment docs.
- `package.json` (repo root): helper scripts to run frontend + backend together.

## Build, Test, and Development Commands
- Frontend dev server: `cd frontend && npm run dev`
- Frontend build: `cd frontend && npm run build`
- Frontend type-check: `cd frontend && npm run type-check`
- Backend run: `cd backend && dotnet run`
- Run both: `npm run dev` (from repo root, uses `concurrently`)

## Coding Style & Naming Conventions
- Vue components: `PascalCase.vue` in `frontend/src/components/`.
- Pages: `PascalCasePage.vue` in `frontend/src/pages/`.
- Pinia stores: `frontend/src/stores/*.ts` (camelCase exports like `useAuthStore`).
- C# files: `PascalCase.cs`, endpoints grouped by feature under `backend/Endpoints/`.

## Testing Guidelines
- No automated tests configured yet.
- Use `npm run type-check` for frontend correctness and `dotnet build` for backend compile checks.

## Commit & Pull Request Guidelines
- No explicit commit convention in history; use clear, imperative messages (e.g., `Add cart checkout flow`).
- PRs should include: summary, screenshots for UI changes, and testing notes.

## Configuration Tips
- Frontend API base URL: `frontend/.env` (`VITE_API_BASE_URL`).
- Admin API calls require header `X-Admin-UserId`.
- SQLite path is configured in `backend/appsettings.json`.
