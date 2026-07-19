import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Sidebar from '../components/candidate/Sidebar'
import TopNavbar from '../components/candidate/TopNavbar'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import { useAuth } from '../context/AuthContext'
import { getJobs } from '../services/jobService'
import styles from './CandidateJobs.module.css'

const initialResult = { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 }
const formatDate = (value) => value ? new Intl.DateTimeFormat(undefined, { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value)) : 'Open'

export default function CandidateJobs() {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [filters, setFilters] = useState({ search: '', location: '', employmentType: '', sort: 'newest', page: 1, pageSize: 10 })
  const [result, setResult] = useState(initialResult)
  const [isLoading, setIsLoading] = useState(true)
  const [hasError, setHasError] = useState(false)
  const { logout, user } = useAuth()
  const navigate = useNavigate()

  const loadJobs = useCallback(async (signal) => {
    setIsLoading(true)
    setHasError(false)
    try {
      setResult(await getJobs(filters, signal))
    } catch (error) {
      if (error.name !== 'CanceledError') {
        setResult(initialResult)
        setHasError(true)
      }
    } finally {
      if (!signal?.aborted) setIsLoading(false)
    }
  }, [filters])

  useEffect(() => {
    const controller = new AbortController()
    const timeout = setTimeout(() => loadJobs(controller.signal), 250)
    return () => { clearTimeout(timeout); controller.abort() }
  }, [loadJobs])

  const updateFilter = (event) => setFilters((current) => ({ ...current, [event.target.name]: event.target.value, page: 1 }))
  const changePage = (page) => setFilters((current) => ({ ...current, page }))
  const handleLogout = () => { logout(); navigate('/login', { replace: true }) }

  return (
    <DashboardLayout
      sidebar={<Sidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)} onLogout={handleLogout} />}
      navbar={<TopNavbar onMenuClick={() => setIsSidebarOpen(true)} user={user} />}
    >
      <header className={styles.pageHeader}>
        <div><h1>Find Jobs</h1><p>Discover opportunities that match your next move.</p></div>
        <span>{result.totalItems} jobs available</span>
      </header>

      <section className={styles.filters} aria-label="Job filters">
        <input name="search" value={filters.search} onChange={updateFilter} placeholder="Search jobs or companies" aria-label="Search jobs" />
        <input name="location" value={filters.location} onChange={updateFilter} placeholder="Location" aria-label="Filter by location" />
        <select name="employmentType" value={filters.employmentType} onChange={updateFilter} aria-label="Employment type">
          <option value="">All employment types</option><option>Full-time</option><option>Part-time</option><option>Contract</option><option>Internship</option>
        </select>
        <select name="sort" value={filters.sort} onChange={updateFilter} aria-label="Sort jobs">
          <option value="newest">Newest first</option><option value="oldest">Oldest first</option><option value="closing">Closing soon</option>
        </select>
      </section>

      <section className={styles.jobsList} aria-live="polite">
        {isLoading && Array.from({ length: 4 }, (_, index) => <div className={styles.skeleton} key={index} />)}
        {!isLoading && hasError && <div className={styles.state}>Could not load jobs.<button type="button" onClick={() => loadJobs()}>Retry</button></div>}
        {!isLoading && !hasError && result.items.length === 0 && <div className={styles.state}>No jobs match your filters.</div>}
        {!isLoading && !hasError && result.items.map((job) => (
          <article className={styles.jobCard} key={job.jobId}>
            <div className={styles.cardTop}>
              <div><h2>{job.title}</h2><p>{job.companyName}</p></div>
              {job.hasApplied && <span className={styles.applied}>Applied</span>}
            </div>
            <div className={styles.meta}><span>{job.location}</span><span>{job.employmentType}</span><span>Posted {formatDate(job.postedOn)}</span><span>Closes {formatDate(job.closingDate)}</span></div>
            <p className={styles.summary}>{job.summary}</p>
            <div className={styles.cardBottom}>
              <div className={styles.skills}>{job.skills.map((skill) => <span key={skill}>{skill}</span>)}</div>
              <button type="button" onClick={() => navigate(`/candidate/jobs/${job.jobId}`)}>View Details</button>
            </div>
          </article>
        ))}
      </section>

      {!isLoading && !hasError && result.totalPages > 1 && <nav className={styles.pagination} aria-label="Job pages">
        <button type="button" disabled={result.page <= 1} onClick={() => changePage(result.page - 1)}>Previous</button>
        <span>Page {result.page} of {result.totalPages}</span>
        <button type="button" disabled={result.page >= result.totalPages} onClick={() => changePage(result.page + 1)}>Next</button>
      </nav>}
    </DashboardLayout>
  )
}
