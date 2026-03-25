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
- `GET /api/foods/search?q=milk` - returns up to 25 matching foods with portion sizes and calories

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

## Data transformation

Generate the searchable food JSON from the USDA MyPyramid source:

```bash
python3 scripts/transform_mypyramid_data.py
```

The transform script downloads the USDA ZIP into `data/raw/` when needed, reads `Food_Display_Table.xml`, and writes normalized search records to `src/CalorieCounter.Api/Data/food-data.json`.

Each generated record includes:

- `foodCode`
- `displayName`
- `searchName`
- `portionAmount`
- `portionDisplayName`
- `portionDescription`
- `calories`
