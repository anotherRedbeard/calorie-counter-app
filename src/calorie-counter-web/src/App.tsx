import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

type FoodSearchResult = {
  foodCode: string
  displayName: string
  portionDescription: string
  calories: number
}

type FoodSearchResponse = {
  query: string
  totalMatches: number
  results: FoodSearchResult[]
  warning: string | null
}

function App() {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<FoodSearchResult[]>([])
  const [totalMatches, setTotalMatches] = useState<number | null>(null)
  const [warning, setWarning] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const calorieFormatter = useMemo(
    () =>
      new Intl.NumberFormat(undefined, {
        minimumFractionDigits: 0,
        maximumFractionDigits: 2,
      }),
    [],
  )

  const handleSearch = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const trimmedQuery = query.trim()
    if (!trimmedQuery) {
      setResults([])
      setTotalMatches(null)
      setWarning('Please enter a food description before searching.')
      return
    }

    setIsLoading(true)
    setWarning(null)

    try {
      const response = await fetch(
        `/api/foods/search?q=${encodeURIComponent(trimmedQuery)}`,
      )

      if (!response.ok) {
        const problem = (await response.json()) as { detail?: string; title?: string }
        throw new Error(problem.detail ?? problem.title ?? 'Search failed.')
      }

      const data = (await response.json()) as FoodSearchResponse
      setResults(data.results)
      setTotalMatches(data.totalMatches)
      setWarning(data.warning)
    } catch (error) {
      setResults([])
      setTotalMatches(0)
      setWarning(
        error instanceof Error
          ? error.message
          : 'Something went wrong while searching.',
      )
    } finally {
      setIsLoading(false)
    }
  }

  const handleClear = () => {
    setQuery('')
    setResults([])
    setTotalMatches(null)
    setWarning(null)
  }

  return (
    <main className="app-shell">
      <section className="hero-panel app-header">
        <p className="eyebrow">Calorie Counter</p>
        <h1>Search USDA MyPyramid food data and compare calories by portion.</h1>
        <p className="summary">
          Enter a food description, run a search, and review up to 25 matching
          foods with their portion sizes and calories.
        </p>
      </section>

      <section className="content-grid">
        <article className="card search-card">
          <h2>Find a food</h2>
          <form className="search-form" onSubmit={handleSearch}>
            <label className="field-label" htmlFor="food-query">
              Food description
            </label>
            <input
              id="food-query"
              className="search-input"
              type="text"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Try milk, apple, chicken, or rice"
            />
            <div className="button-row">
              <button className="primary-button" type="submit" disabled={isLoading}>
                {isLoading ? 'Searching...' : 'Search'}
              </button>
              <button className="secondary-button" type="button" onClick={handleClear}>
                Clear
              </button>
            </div>
          </form>

          {warning && (
            <p className="warning-banner" role="alert">
              {warning}
            </p>
          )}
        </article>

        <article className="card results-card">
          <div className="results-header">
            <h2>
              Results
              {totalMatches != null && (
                <span className="match-count" aria-label={`${totalMatches} total ${totalMatches === 1 ? 'match' : 'matches'} found`}>{totalMatches} {totalMatches === 1 ? 'match' : 'matches'}</span>
              )}
            </h2>
            <p>Limited to the first 25 matches.</p>
          </div>

          <div className="results-panel" role="region" aria-live="polite">
            {results.length === 0 ? (
              <p className="empty-state">
                Search for a food to see matching portion sizes and calories.
              </p>
            ) : (
              <ul className="results-list">
                {results.map((result) => (
                  <li key={`${result.foodCode}-${result.portionDescription}`} className="result-item">
                    <div>
                      <h3>{result.displayName}</h3>
                      <p>{result.portionDescription}</p>
                    </div>
                    <strong>{calorieFormatter.format(result.calories)} cal</strong>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </article>
      </section>
    </main>
  )
}

export default App
