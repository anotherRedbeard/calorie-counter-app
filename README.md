# Calorie Counter

A React SPA with a small .NET backend for searching USDA MyPyramid food data and returning calories by portion.

## Project structure

- `src/calorie-counter-web` - Vite + React frontend
- `src/CalorieCounter.Api` - .NET 9 Web API backend

## Local development

Start the API:

```bash
dotnet run --project src/CalorieCounter.Api
```

Start the frontend in a second terminal:

```bash
cd src/calorie-counter-web
npm run dev
```

The Vite dev server proxies `/api` requests to `http://localhost:5268`.

## Current API surface

- `GET /api/health` - simple connectivity check for local development
- `GET /api/foods/search?q=milk` - returns up to 25 matching foods with portion sizes and calories; the total match count is shown beside the results heading

### Wildcard search

The search endpoint supports `*` as a wildcard character for flexible matching:

| Pattern    | Meaning                        | Example matches               |
|------------|--------------------------------|-------------------------------|
| `milk`     | substring match (default)      | "1% milk (low fat)", "milk"   |
| `apple*`   | starts with "apple"            | "apple juice", "applesauce"   |
| `*milk`    | ends with "milk"               | "chocolate milk", "skim milk" |
| `chick*n`  | starts with "chick", ends "n"  | "chicken"                     |
| `*milk*`   | contains "milk" (any position) | "1% milk (low fat)"           |

All searches are case-insensitive. When no `*` is present the search falls back to a simple substring match.

## Search indexing strategy

The backend builds an in-memory n-gram index when it starts (using 1- to 3-character n-grams from each record's `searchName`).

At query time, the API:

1. normalizes the incoming query
2. looks up candidate records by intersecting indexed n-gram posting lists
3. applies the existing substring match and result limiting rules to preserve behavior and ordering

This avoids relying solely on scanning the entire food array for every search request while keeping local development simple.

## Data transformation

Generate the searchable food JSON from the USDA MyPyramid source:

```bash
python3 scripts/transform_mypyramid_data.py
```

The transform script downloads the USDA ZIP into `data/raw/` when needed, reads `Food_Display_Table.xml`, and writes normalized search records to `src/CalorieCounter.Api/Data/food-data.json`.

No additional index artifact is required: the API rebuilds the in-memory index from this generated JSON at startup.

Each generated record includes:

- `foodCode`
- `displayName`
- `searchName`
- `portionAmount`
- `portionDisplayName`
- `portionDescription`
- `calories`
