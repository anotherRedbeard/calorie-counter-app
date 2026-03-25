# Calorie Counter Plan

## Problem

Build a calorie counter application as a React single-page app with a small .NET backend. The app will let users search USDA MyPyramid food data, return calorie information with portion sizes, and support the required tier-3 user stories first before any bonus enhancements.

## Progress

- Done: repository scaffolded with a React + Vite frontend and a .NET 9 Web API backend.
- Done: frontend development proxy wired to `/api` for local development.
- Done: USDA data pipeline implemented from the published MyPyramid ZIP resource.
- Done: required search UI and backend endpoint implemented and smoke-tested.
- Done: local frontend and backend services started and verified on ports `5173` and `5268`.
- Next: phase-two bonus features remain pending.

## Proposed approach

Create the project as two parts:
- A React SPA for the search form, validation, results panel, and clear/reset behavior.
- A .NET backend API that loads transformed USDA food data and exposes a search endpoint.

Prepare the USDA source data ahead of runtime:
- Download the MyPyramid Excel spreadsheet.
- Convert the spreadsheet into a normalized JSON dataset during a data-prep step.
- Store only the fields needed for search and display, such as description, portion size, and calories.

Build the first release around required functionality only:
- Input, Search, and Clear controls.
- Warning states for empty input and no matches.
- Scrollable results panel capped at 25 entries.
- Matching rows showing food item, portion size, and calories.

Design phase two for bonus features:
- Match count next to results.
- Wildcard search support.
- Incremental loading beyond 25 entries.
- Faster search indexing or alternate data structures / database-backed lookup.

## Todos

1. Scaffold the application structure
   - Create a React SPA frontend and a .NET backend with a simple local development workflow.
   - Define the API boundary, environment configuration, and local run commands.

2. Build the USDA data transformation pipeline
   - Acquire the source spreadsheet.
   - Convert it into a normalized JSON format suitable for runtime loading and searching.
   - Document the transformation assumptions and output schema.

3. Implement backend data loading and search
   - Load the transformed JSON on startup.
   - Implement a search endpoint that validates input, performs case-insensitive matching, and limits results to 25 entries.
   - Return a consistent response shape for matches and warnings.

4. Build the frontend search experience
   - Add the food description input, Search button, Clear button, warnings, and results panel.
   - Connect the UI to the backend API and handle loading, empty input, no-result, and success states.

5. Validate required behavior end to end
   - Verify required user stories with manual testing and any existing automated tests.
   - Confirm clear/reset behavior, result limiting, and scrolling UX.

6. Plan phase-two enhancements
   - Identify where to add result counts, wildcard handling, pagination/load-more behavior, and faster search indexing without rework.

## Notes and considerations

- Because the repository is currently empty, the implementation will start from a fresh project structure.
- The backend should own search logic so the data pipeline and search rules remain centralized.
- For the first release, substring search is the safest default unless future wildcard support changes matching semantics.
- The JSON transformation step is part of the deliverable because it makes runtime search simpler and keeps the app aligned with the project requirements.
- The initial transform uses `Food_Display_Table.xml`, which already contains the fields required for search, portion display, and calorie output.
