import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import ApplicationsTable from '../components/candidate/ApplicationsTable'
import Sidebar from '../components/candidate/Sidebar'
import StatCard from '../components/candidate/StatCard'
import TopNavbar from '../components/candidate/TopNavbar'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import { useAuth } from '../context/AuthContext'
import { emptyStats, getCandidateDashboardStats, getCandidateRecentApplications } from '../services/candidateDashboardService'
import styles from './CandidateDashboard.module.css'

const stats = [
  { key: 'jobsApplied', label: 'Jobs Applied', caption: 'Total applications', icon: 'briefcase', tone: 'Blue' },
  { key: 'interviews', label: 'Interviews', caption: 'Upcoming interviews', icon: 'people', tone: 'Green' },
  { key: 'shortlisted', label: 'Shortlisted', caption: "You're a top pick!", icon: 'calendar', tone: 'Orange' },
  { key: 'pending', label: 'Pending', caption: 'Awaiting response', icon: 'bookmark', tone: 'Purple' },
]

export default function CandidateDashboard() {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [dashboardStats, setDashboardStats] = useState(emptyStats)
  const [isStatsLoading, setIsStatsLoading] = useState(true)
  const [statsError, setStatsError] = useState(false)
  const [applications, setApplications] = useState([])
  const [areApplicationsLoading, setAreApplicationsLoading] = useState(true)
  const [applicationsError, setApplicationsError] = useState(false)
  const navigate = useNavigate()
  const { logout, user } = useAuth()
  const firstName = user?.firstName || user?.fullName?.split(' ')[0] || 'Candidate'

  const loadStats = useCallback(async (signal) => {
    setIsStatsLoading(true)
    setStatsError(false)

    try {
      const result = await getCandidateDashboardStats(signal)
      setDashboardStats(result)
    } catch (error) {
      if (error.name !== 'CanceledError') {
        setDashboardStats(emptyStats)
        setStatsError(true)
      }
    } finally {
      if (!signal?.aborted) setIsStatsLoading(false)
    }
  }, [])

  const loadApplications = useCallback(async (signal) => {
    setAreApplicationsLoading(true)
    setApplicationsError(false)
    try {
      const result = await getCandidateRecentApplications(signal)
      const tones = { Shortlisted: 'Green', Submitted: 'Orange', UnderReview: 'Orange', Interviewing: 'Blue', Offered: 'Green', Hired: 'Green', Rejected: 'Orange', Withdrawn: 'Orange' }
      setApplications(result.map((application) => ({
        ...application,
        title: application.jobTitle,
        company: application.companyName,
        tone: tones[application.status] || 'Blue',
        date: new Intl.DateTimeFormat(undefined, { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(application.appliedOn)),
      })))
    } catch (error) {
      if (error.name !== 'CanceledError') {
        setApplications([])
        setApplicationsError(true)
      }
    } finally {
      if (!signal?.aborted) setAreApplicationsLoading(false)
    }
  }, [])

  useEffect(() => {
    const abortController = new AbortController()
    loadStats(abortController.signal)
    return () => abortController.abort()
  }, [loadStats])

  useEffect(() => {
    const abortController = new AbortController()
    loadApplications(abortController.signal)
    return () => abortController.abort()
  }, [loadApplications])

  function handleLogout() {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <DashboardLayout
      sidebar={(
        <Sidebar
          isOpen={isSidebarOpen}
          onClose={() => setIsSidebarOpen(false)}
          onLogout={handleLogout}
        />
      )}
      navbar={<TopNavbar onMenuClick={() => setIsSidebarOpen(true)} user={user} />}
    >
      <header className={styles.welcomeHeader}>
        <h1>Good Morning, {firstName}! <span aria-hidden="true">👋</span></h1>
        <p>Here's what's happening with your job search today.</p>
      </header>

      <section className={styles.statsGrid} aria-label="Application statistics">
        {stats.map(({ key, ...stat }) => (
          <StatCard
            key={key}
            {...stat}
            value={dashboardStats[key]}
            isLoading={isStatsLoading}
          />
        ))}
      </section>

      {statsError && (
        <div className={styles.statsError} role="alert">
          <span>Could not load dashboard statistics.</span>
          <button type="button" onClick={() => loadStats()}>Retry</button>
        </div>
      )}

      <div className={styles.dashboardGrid}>
        <ApplicationsTable applications={applications} isLoading={areApplicationsLoading} hasError={applicationsError} onRetry={() => loadApplications()} />
      </div>
    </DashboardLayout>
  )
}
