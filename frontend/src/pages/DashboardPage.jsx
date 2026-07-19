import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function DashboardPage({ title }) {
  const navigate = useNavigate()
  const { logout, user } = useAuth()

  function handleLogout() {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <main className="dashboard-page">
      <section className="dashboard-card">
        <h1>{title}</h1>
        <p className="dashboard-name">{user.fullName}</p>
        <p className="dashboard-role">{user.role}</p>
        <button type="button" onClick={handleLogout}>Log out</button>
      </section>
    </main>
  )
}
