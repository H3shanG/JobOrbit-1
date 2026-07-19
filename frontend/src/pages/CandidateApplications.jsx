import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Sidebar from '../components/candidate/Sidebar'
import TopNavbar from '../components/candidate/TopNavbar'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import { useAuth } from '../context/AuthContext'
import { getCandidateApplications } from '../services/candidateApplicationService'
import styles from './CandidateApplications.module.css'

const initial = { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0, summary: { total: 0, pending: 0, shortlisted: 0, interviews: 0, rejected: 0 } }
const statuses = ['', 'Submitted', 'UnderReview', 'Shortlisted', 'Interviewing', 'Offered', 'Hired', 'Rejected', 'Withdrawn']
const formatDate = (value) => value ? new Intl.DateTimeFormat(undefined, { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value)) : '—'
const tone = (status) => ({ Submitted: 'pending', UnderReview: 'pending', Shortlisted: 'success', Interviewing: 'interview', Offered: 'success', Hired: 'success', Rejected: 'danger', Withdrawn: 'muted' }[status] || 'pending')

export default function CandidateApplications() {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [filters, setFilters] = useState({ search: '', status: '', sort: 'newest', page: 1, pageSize: 10 })
  const [result, setResult] = useState(initial)
  const [state, setState] = useState('loading')
  const { logout, user } = useAuth()
  const navigate = useNavigate()

  const load = useCallback(async (signal) => {
    setState('loading')
    try { setResult(await getCandidateApplications(filters, signal)); setState('ready') }
    catch (error) { if (error.name !== 'CanceledError') { setResult(initial); setState('error') } }
  }, [filters])

  useEffect(() => {
    const controller = new AbortController()
    const timeout = setTimeout(() => load(controller.signal), 250)
    return () => { clearTimeout(timeout); controller.abort() }
  }, [load])

  const update = (event) => setFilters((current) => ({ ...current, [event.target.name]: event.target.value, page: 1 }))
  const logoutUser = () => { logout(); navigate('/login', { replace: true }) }
  const counters = [['Total', result.summary.total], ['Pending', result.summary.pending], ['Shortlisted', result.summary.shortlisted], ['Interviews', result.summary.interviews], ['Rejected', result.summary.rejected]]

  return <DashboardLayout sidebar={<Sidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)} onLogout={logoutUser} />} navbar={<TopNavbar onMenuClick={() => setIsSidebarOpen(true)} user={user} />}>
    <header className={styles.header}><div><h1>My Applications</h1><p>Track your applications and hiring progress.</p></div></header>
    <section className={styles.counters}>{counters.map(([label, value]) => <article key={label}><span>{label}</span><strong>{value}</strong></article>)}</section>
    <section className={styles.filters}><input name="search" value={filters.search} onChange={update} placeholder="Search jobs, companies, or locations" aria-label="Search applications" /><select name="status" value={filters.status} onChange={update} aria-label="Filter by status">{statuses.map((status) => <option value={status} key={status || 'all'}>{status || 'All statuses'}</option>)}</select><select name="sort" value={filters.sort} onChange={update} aria-label="Sort applications"><option value="newest">Newest first</option><option value="oldest">Oldest first</option></select></section>
    <section className={styles.panel} aria-live="polite">
      {state === 'loading' && <div className={styles.loading}>{Array.from({ length: 5 }, (_, index) => <span key={index} />)}</div>}
      {state === 'error' && <div className={styles.empty}>Could not load applications.<button type="button" onClick={() => load()}>Retry</button></div>}
      {state === 'ready' && result.items.length === 0 && <div className={styles.empty}>No applications found.</div>}
      {state === 'ready' && result.items.length > 0 && <div className={styles.scroller}><table><thead><tr><th>Job</th><th>Status</th><th>Applied</th><th>Interview</th><th /></tr></thead><tbody>{result.items.map((application) => <tr key={application.applicationId}><td data-label="Job"><strong>{application.jobTitle}</strong><small>{application.companyName} · {application.location} · {application.employmentType}</small></td><td data-label="Status"><span className={`${styles.badge} ${styles[tone(application.status)]}`}>{application.status}</span></td><td data-label="Applied">{formatDate(application.appliedOn)}</td><td data-label="Interview">{formatDate(application.interviewDate)}</td><td><button type="button" onClick={() => navigate(`/candidate/applications/${application.applicationId}`)}>View Details</button></td></tr>)}</tbody></table></div>}
    </section>
    {state === 'ready' && result.totalPages > 1 && <nav className={styles.pagination}><button type="button" disabled={result.page <= 1} onClick={() => setFilters((x) => ({ ...x, page: x.page - 1 }))}>Previous</button><span>Page {result.page} of {result.totalPages}</span><button type="button" disabled={result.page >= result.totalPages} onClick={() => setFilters((x) => ({ ...x, page: x.page + 1 }))}>Next</button></nav>}
  </DashboardLayout>
}
