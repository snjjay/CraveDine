# EatKath — Claude Code Instructions

## Architecture
- React + TypeScript frontend
- ASP.NET Core 8 Web API
- EF Core + SQL Server
- MSTest API tests

## Development rules
- Inspect existing patterns before creating new ones.
- Prefer modifying existing code over duplicating functionality.
- Keep frontend/backend responsibilities separated.
- Add/update tests for backend business logic changes.
- Do not change database schema unless required.
- Do not remove existing functionality without approval.

## Workflow
- For non-trivial changes: explore → plan → review → implement → test.
- Explain important assumptions before making significant changes.
