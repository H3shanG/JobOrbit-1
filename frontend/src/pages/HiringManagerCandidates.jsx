import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import ManagerSidebar from '../components/manager/ManagerSidebar'
import ManagerNavbar from '../components/manager/ManagerNavbar'
import { useAuth } from '../context/AuthContext'
import { getHiringManagerCandidates } from '../services/hiringManagerCandidateService'
import styles from './HiringManagerCandidates.module.css'

export default function HiringManagerCandidates() {
  const [open, setOpen] = useState(false), [query, setQuery] = useState({ search: '', status: '', jobId: '', sort: 'newest', page: 1, pageSize: 10 })
  const [result, setResult] = useState({ items: [], page: 1, totalPages: 0 }), [loading, setLoading] = useState(true), [error, setError] = useState('')
  const { user, logout } = useAuth(), navigate = useNavigate()
  const load = useCallback(() => { const c = new AbortController(); setLoading(true); setError(''); getHiringManagerCandidates({ ...query, jobId: query.jobId || undefined }, c.signal).then(setResult).catch(e => { if (e.name !== 'CanceledError') setError('Could not load candidates.') }).finally(() => setLoading(false)); return () => c.abort() }, [query])
  useEffect(() => load(), [load])
  const jobs = useMemo(() => [...new Map(result.items.map(x => [x.jobId, x.jobTitle])).entries()], [result.items])
  const set = (key, value) => setQuery(x => ({ ...x, [key]: value, page: 1 })), signOut = () => { logout(); navigate('/login') }
  return <DashboardLayout sidebar={<ManagerSidebar isOpen={open} onClose={() => setOpen(false)} onLogout={signOut} />} navbar={<ManagerNavbar user={user} onMenuClick={() => setOpen(true)} />}>
    <header className={styles.heading}><h1>Candidates to Review</h1><p>Review shortlisted candidates and interview outcomes.</p></header>
    <div className={styles.filters}><input aria-label="Search candidates" placeholder="Search candidate, title, or job" value={query.search} onChange={e => set('search', e.target.value)} /><select aria-label="Filter by job" value={query.jobId} onChange={e => set('jobId', e.target.value)}><option value="">All jobs</option>{jobs.map(([id, title]) => <option key={id} value={id}>{title}</option>)}</select><select aria-label="Filter by status" value={query.status} onChange={e => set('status', e.target.value)}><option value="">All review statuses</option>{['Shortlisted', 'Interviewing'].map(x => <option key={x}>{x}</option>)}</select><select aria-label="Sort candidates" value={query.sort} onChange={e => set('sort', e.target.value)}><option value="newest">Newest</option><option value="oldest">Oldest</option><option value="name">Candidate name</option></select></div>
    {error && <div className={styles.state}>{error} <button onClick={load}>Retry</button></div>}
    {loading ? <div className={styles.state}>Loading candidates…</div> : result.items.length === 0 ? <div className={styles.state}><h2>No candidates to review</h2><p>Shortlisted and interviewed candidates in your assigned scope will appear here.</p></div> : <div className={styles.tableWrap}><table><thead><tr><th>Candidate</th><th>Job</th><th>Match</th><th>Application</th><th>Interview</th><th>Evaluation</th><th>Applied</th><th /></tr></thead><tbody>{result.items.map(x => <tr key={x.applicationId}><td><strong>{x.candidateName}</strong><span>{x.professionalTitle || 'Not provided'}</span></td><td>{x.jobTitle}</td><td>{x.matchScore == null ? '—' : `${x.matchScore}%`}</td><td><span className={styles.badge}>{x.status}</span></td><td>{x.interviewStatus || 'Not scheduled'}</td><td>{x.evaluationStatus}</td><td>{new Date(x.appliedOn).toLocaleDateString()}</td><td><button onClick={() => navigate(`/manager/candidates/${x.applicationId}`)}>View Details</button></td></tr>)}</tbody></table></div>}
    <div className={styles.pagination}><button disabled={result.page <= 1} onClick={() => setQuery(x => ({ ...x, page: x.page - 1 }))}>Previous</button><span>Page {result.page} of {Math.max(1, result.totalPages)}</span><button disabled={result.page >= result.totalPages} onClick={() => setQuery(x => ({ ...x, page: x.page + 1 }))}>Next</button></div>
  </DashboardLayout>
}
