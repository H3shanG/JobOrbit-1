import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import ManagerSidebar from '../components/manager/ManagerSidebar'
import ManagerNavbar from '../components/manager/ManagerNavbar'
import DashboardIcon from '../components/candidate/DashboardIcon'
import { useAuth } from '../context/AuthContext'
import { getHiringManagerDashboardStats } from '../services/hiringManagerDashboardService'
import { getHiringManagerDashboardCandidates } from '../services/hiringManagerCandidateService'
import { getManagerEvaluationSummary } from '../services/hiringManagerEvaluationService'
import { getHiringFunnel } from '../services/hiringDecisionService'
import { getHiringManagerInterviews } from '../services/hiringManagerInterviewService'
import styles from './HiringManagerDashboard.module.css'

const cards = [['pendingReviews', 'Pending Reviews', 'Candidates'], ['todaysInterviews', "Today's Interviews", 'Scheduled'], ['teamFeedback', 'Team Feedback', 'Pending'], ['hiredThisMonth', 'Hired (This Month)', 'New employees']]
export default function HiringManagerDashboard() {
  const [open, setOpen] = useState(false), [stats, setStats] = useState({}), [loading, setLoading] = useState(true), [error, setError] = useState(false)
  const [candidates, setCandidates] = useState([]), [candidatesLoading, setCandidatesLoading] = useState(true), [candidatesError, setCandidatesError] = useState(false)
  const [evaluation, setEvaluation] = useState(null), [evaluationError, setEvaluationError] = useState(false)
  const [funnel, setFunnel] = useState(null), [funnelError, setFunnelError] = useState(false)
  const [interviews, setInterviews] = useState([]), [interviewsLoading, setInterviewsLoading] = useState(true), [interviewsError, setInterviewsError] = useState(false)
  const { user, logout } = useAuth(), navigate = useNavigate()
  const loadStats = useCallback(() => { const c = new AbortController(); setLoading(true); setError(false); getHiringManagerDashboardStats(c.signal).then(setStats).catch(e => { if (e.name !== 'CanceledError') setError(true) }).finally(() => setLoading(false)); return () => c.abort() }, [])
  const loadCandidates = useCallback(() => { const c = new AbortController(); setCandidatesLoading(true); setCandidatesError(false); getHiringManagerDashboardCandidates(c.signal).then(setCandidates).catch(e => { if (e.name !== 'CanceledError') setCandidatesError(true) }).finally(() => setCandidatesLoading(false)); return () => c.abort() }, [])
  const loadEvaluation = useCallback(() => { const c = new AbortController(); setEvaluationError(false); getManagerEvaluationSummary(c.signal).then(setEvaluation).catch(e => { if (e.name !== 'CanceledError') setEvaluationError(true) }); return () => c.abort() }, [])
  const loadFunnel = useCallback(() => { const c = new AbortController(); setFunnelError(false); getHiringFunnel(c.signal).then(setFunnel).catch(e => { if (e.name !== 'CanceledError') setFunnelError(true) }); return () => c.abort() }, [])
  const loadInterviews = useCallback(() => { const c = new AbortController(); setInterviewsLoading(true); setInterviewsError(false); getHiringManagerInterviews({ page: 1, pageSize: 3, sort: 'upcoming' }, c.signal).then(x => setInterviews(x.items)).catch(e => { if (e.name !== 'CanceledError') setInterviewsError(true) }).finally(() => setInterviewsLoading(false)); return () => c.abort() }, [])
  useEffect(() => loadStats(), [loadStats]); useEffect(() => loadCandidates(), [loadCandidates]); useEffect(() => loadEvaluation(), [loadEvaluation]); useEffect(() => loadFunnel(), [loadFunnel]); useEffect(() => loadInterviews(), [loadInterviews])
  const signOut = () => { logout(); navigate('/login') }
  return <DashboardLayout sidebar={<ManagerSidebar isOpen={open} onClose={() => setOpen(false)} onLogout={signOut} />} navbar={<ManagerNavbar user={user} onMenuClick={() => setOpen(true)} />}>
    <header className={styles.heading}><h1>Hiring Manager Dashboard</h1><p>Review candidates, share feedback and make great hiring decisions.</p></header>
    <section className={styles.stats}>{cards.map(([key, label, caption]) => <article key={key}><span>{label}</span>{loading ? <i /> : <strong>{stats[key] ?? 0}</strong>}<small>{caption}</small></article>)}</section>
    {error && <div className={styles.error}>Could not load dashboard statistics.<button onClick={loadStats}>Retry</button></div>}
    <div className={styles.dashboardGrid}>
      <section className={styles.panel}><header><h2>Candidates to Review</h2><button onClick={() => navigate('/manager/candidates')}>View All</button></header><div className={styles.peopleList}>{candidatesLoading ? <p>Loading candidates…</p> : candidatesError ? <p>Could not load candidates. <button onClick={loadCandidates}>Retry</button></p> : candidates.length === 0 ? <p>No candidates to review</p> : candidates.map(candidate => <article key={candidate.applicationId} onClick={() => navigate(`/manager/candidates/${candidate.applicationId}`)}><b>{candidate.candidateName[0]}</b><div><strong>{candidate.candidateName}</strong><small>{candidate.professionalTitle || candidate.jobTitle}</small><em>Applied {new Date(candidate.appliedOn).toLocaleDateString()}</em></div><span>{candidate.matchScore == null ? 'New' : `${candidate.matchScore}% Match`}</span></article>)}</div></section>
      <section className={styles.panel}><header><h2>Hiring Funnel</h2><select disabled><option>Current</option></select></header>{funnelError?<p>Could not load funnel. <button onClick={loadFunnel}>Retry</button></p>:!funnel?<p>Loading funnel…</p>:<div className={styles.funnel}><i /><i /><i /><i /><div><span>Shortlisted <b>{funnel.shortlisted}</b></span><span>Interviewed <b>{funnel.interviewed}</b></span><span>Evaluated <b>{funnel.evaluated}</b></span><span>Held <b>{funnel.held}</b></span><span>Hired <b>{funnel.hired}</b></span><span>Rejected <b>{funnel.rejected}</b></span></div></div>}</section>
      <section className={styles.panel}><header><h2>Interview Schedule</h2><button onClick={() => navigate('/manager/interviews')}>View All</button></header><div className={styles.peopleList}>{interviewsLoading ? <p>Loading interviews…</p> : interviewsError ? <p>Could not load interviews. <button onClick={loadInterviews}>Retry</button></p> : interviews.length === 0 ? <p>No upcoming interviews</p> : interviews.map(interview => <article key={interview.interviewId} onClick={() => navigate(`/manager/interviews/${interview.interviewId}`)}><b>{interview.candidateName[0]}</b><div><strong>{interview.candidateName}</strong><small>{interview.jobTitle}</small><em><DashboardIcon name="calendar" size={11} /> {new Date(interview.scheduledAt).toLocaleString()}</em></div></article>)}</div></section>
      <section className={styles.panel}><h2>Evaluation Summary</h2>{evaluationError ? <p>Could not load evaluations. <button onClick={loadEvaluation}>Retry</button></p> : !evaluation ? <p>Loading evaluations…</p> : <div className={styles.evaluation}><div className={styles.donut}><span>Average<br /><b>{evaluation.averageOverallScore}</b></span></div><ul><li>Completed <b>{evaluation.completedEvaluations}</b></li><li>Pending <b>{evaluation.pendingEvaluations}</b></li>{evaluation.recommendationCounts.map(x => <li key={x.recommendation}>{x.recommendation} <b>{x.count}</b></li>)}</ul></div>}</section>
    </div>
  </DashboardLayout>
}
