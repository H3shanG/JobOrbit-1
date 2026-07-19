import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getDashboardPath } from '../services/authService'
import { getPublicPlatformSettings } from '../services/adminSystemSettingsService'

export default function LoginPage() {
  const navigate = useNavigate()
  const { isAuthenticated, isLoading: isRestoring, login, user } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [maintenanceMessage, setMaintenanceMessage] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    getPublicPlatformSettings(controller.signal).then((settings) => {
      setMaintenanceMessage(settings.maintenanceModeEnabled ? settings.maintenanceMessage : '')
    }).catch(() => {})
    return () => controller.abort()
  }, [])

  useEffect(() => {
    if (!isRestoring && isAuthenticated && user) {
      navigate(getDashboardPath(user.role), { replace: true })
    }
  }, [isAuthenticated, isRestoring, navigate, user])

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      const currentUser = await login(email.trim(), password)
      navigate(getDashboardPath(currentUser.role), { replace: true })
    } catch (requestError) {
      if (requestError.response?.status === 401) {
        setError('Invalid email or password.')
      } else if (requestError.response?.status === 400) {
        setError('Please enter a valid email address and password.')
      } else if (!requestError.response) {
        setError('Unable to reach JobOrbit. Check your connection and try again.')
      } else {
        setError('Something went wrong while signing in. Please try again.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isRestoring) {
    return <div className="auth-loading">Restoring your session…</div>
  }

  return (
    <main className="login-page">
      <section className="login-brand-panel" aria-label="JobOrbit introduction">
        <a className="auth-brand" href="/" aria-label="JobOrbit home">
          <span className="auth-brand-mark">O</span>
          <span>JobOrbit</span>
        </a>
        <div className="login-brand-copy">
          <span className="login-kicker">AI-powered recruitment</span>
          <h1>Welcome back to your orbit.</h1>
          <p>Sign in to continue building remarkable teams with clarity and confidence.</p>
        </div>
        <div className="login-orbit" aria-hidden="true">
          <span />
          <span />
          <i />
        </div>
      </section>

      <section className="login-form-panel">
        <form className="login-card" onSubmit={handleSubmit}>
          <div className="login-heading">
            <span className="login-mobile-brand">JobOrbit</span>
            <h2>Sign in</h2>
            <p>Enter your account details to continue.</p>
          </div>

          {maintenanceMessage && <div className="login-error" role="status"><strong>Maintenance mode:</strong> {maintenanceMessage} Administrator access remains available.</div>}

          <label htmlFor="email">Email address</label>
          <input
            id="email"
            name="email"
            type="email"
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="you@example.com"
            required
            disabled={isSubmitting}
          />

          <label htmlFor="password">Password</label>
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="Enter your password"
            required
            disabled={isSubmitting}
          />

          {error && <div className="login-error" role="alert">{error}</div>}

          <button className="login-submit" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Signing in…' : 'Sign in'}
          </button>

          <p className="login-note">Your destination is selected from your verified account role.</p>
        </form>
      </section>
    </main>
  )
}
