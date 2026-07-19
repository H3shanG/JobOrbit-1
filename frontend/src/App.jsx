import { useEffect, useState } from 'react'
import './App.css'

const API_URL = import.meta.env.VITE_API_URL ?? 'https://localhost:7075'

function OrbitMark() {
  return (
    <svg viewBox="0 0 44 44" role="img" aria-label="JobOrbit">
      <circle cx="20" cy="23" r="10.5" fill="none" stroke="currentColor" strokeWidth="4.5" />
      <path d="M5 25c2.7 8.8 13.9 13.4 24.9 10.1C41 31.8 47.6 21.9 45 13.2" fill="none" stroke="currentColor" strokeLinecap="round" strokeWidth="2.2" opacity=".45" />
      <circle cx="35" cy="9" r="5.5" fill="currentColor" />
      <circle cx="35" cy="9" r="2" fill="white" opacity=".9" />
    </svg>
  )
}

function ArrowIcon() {
  return (
    <svg viewBox="0 0 20 20" aria-hidden="true">
      <path d="M4 10h12m-5-5 5 5-5 5" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.8" />
    </svg>
  )
}

function App() {
  const [apiStatus, setApiStatus] = useState('checking')

  useEffect(() => {
    const abortController = new AbortController()

    fetch(`${API_URL}/api/health`, { signal: abortController.signal })
      .then((response) => {
        if (!response.ok) throw new Error('API is unavailable')
        return response.json()
      })
      .then((health) => setApiStatus(health.status === 'Healthy' ? 'online' : 'offline'))
      .catch((error) => {
        if (error.name !== 'AbortError') setApiStatus('offline')
      })

    return () => abortController.abort()
  }, [])

  const statusText = {
    checking: 'Checking API',
    online: 'API online',
    offline: 'API offline',
  }[apiStatus]

  return (
    <div className="site-shell">
      <header className="topbar">
        <a className="brand" href="#top" aria-label="JobOrbit home">
          <span className="brand-mark"><OrbitMark /></span>
          <span>JobOrbit</span>
        </a>

        <nav aria-label="Primary navigation">
          <a href="#foundation">Platform</a>
          <a href="#architecture">Architecture</a>
          <a href={`${API_URL}/swagger`} target="_blank" rel="noreferrer">API docs</a>
        </nav>

        <span className={`api-pill ${apiStatus}`}>
          <span className="status-dot" aria-hidden="true" />
          {statusText}
        </span>
      </header>

      <main id="top">
        <section className="hero-section">
          <div className="hero-copy">
            <div className="eyebrow">
              <span>AI-powered recruitment</span>
              <span className="eyebrow-divider" />
              <span>Built for what’s next</span>
            </div>

            <h1>Great teams start with a <span>better orbit.</span></h1>
            <p className="hero-lede">
              A thoughtfully engineered foundation for intelligent hiring—bringing
              people, decisions, and momentum into one clear system.
            </p>

            <div className="hero-actions">
              <a className="primary-action" href="#foundation">
                Explore the foundation <ArrowIcon />
              </a>
              <a className="secondary-action" href={`${API_URL}/swagger`} target="_blank" rel="noreferrer">
                Open API docs
              </a>
            </div>

            <div className="trust-row" aria-label="Platform principles">
              <span><i>✓</i> Secure by design</span>
              <span><i>✓</i> Clean architecture</span>
              <span><i>✓</i> Ready to evolve</span>
            </div>
          </div>

          <div className="orbit-visual" aria-hidden="true">
            <div className="ambient-glow" />
            <div className="orbit-ring orbit-ring-one" />
            <div className="orbit-ring orbit-ring-two" />
            <div className="orbit-core">
              <OrbitMark />
              <strong>JobOrbit</strong>
              <span>Intelligence layer</span>
            </div>
            <div className="floating-card card-candidate">
              <span className="card-icon">◎</span>
              <div><strong>People</strong><small>Human potential</small></div>
            </div>
            <div className="floating-card card-insight">
              <span className="card-icon">✦</span>
              <div><strong>Intelligence</strong><small>Clearer decisions</small></div>
            </div>
            <div className="floating-card card-team">
              <span className="card-icon">↗</span>
              <div><strong>Momentum</strong><small>Teams in motion</small></div>
            </div>
          </div>
        </section>

        <section className="foundation-section" id="foundation">
          <div className="section-heading">
            <span className="section-kicker">Platform foundation</span>
            <h2>Built clean from day one.</h2>
            <p>The core technical layers are connected and ready for focused product development.</p>
          </div>

          <div className="foundation-grid" id="architecture">
            <article>
              <span className="feature-number">01</span>
              <div className="feature-icon api-icon">{'{}'}</div>
              <h3>Composed API</h3>
              <p>Controller-based ASP.NET Core endpoints with versioned OpenAPI documentation.</p>
              <span className="feature-tag">ASP.NET Core</span>
            </article>
            <article>
              <span className="feature-number">02</span>
              <div className="feature-icon data-icon">▦</div>
              <h3>Data ready</h3>
              <p>A dedicated infrastructure layer prepared for SQL Server through Entity Framework Core.</p>
              <span className="feature-tag">EF Core</span>
            </article>
            <article>
              <span className="feature-number">03</span>
              <div className="feature-icon secure-icon">◇</div>
              <h3>Data integrity</h3>
              <p>Relational constraints protect identities, applications, skills, and hiring records.</p>
              <span className="feature-tag">SQL Server</span>
            </article>
          </div>
        </section>
      </main>

      <footer>
        <a className="brand footer-brand" href="#top">
          <span className="brand-mark"><OrbitMark /></span>
          <span>JobOrbit</span>
        </a>
        <p>Intelligent hiring, thoughtfully engineered.</p>
        <span>Foundation build</span>
      </footer>
    </div>
  )
}

export default App
