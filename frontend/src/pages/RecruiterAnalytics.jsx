import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import RecruiterSidebar from '../components/recruiter/RecruiterSidebar'
import RecruiterNavbar from '../components/recruiter/RecruiterNavbar'
import { useAuth } from '../context/AuthContext'
import { getRecruiterAnalytics } from '../services/recruiterAnalyticsService'
import styles from './RecruiterAnalytics.module.css'

const summaryCards = [
  ['totalJobs', 'Total Jobs'], ['publishedJobs', 'Published Jobs'],
  ['totalApplications', 'Applications'], ['shortlistedCandidates', 'Shortlisted'],
  ['interviewsScheduled', 'Interviews'], ['offersMade', 'Offers'],
  ['hiredCandidates', 'Hired'], ['rejectedApplications', 'Rejected'],
]
const trendSeries = [['applications', 'Applications', '#1765ee'], ['shortlisted', 'Shortlisted', '#24a866'], ['interviews', 'Interviews', '#e79818'], ['hired', 'Hired', '#805bd6']]
const blank = { summary: {}, conversionRates: {}, applicationsTrend: [], applicationsByStatus: [], topJobs: [] }

function TrendChart({ data }) {
  const max = Math.max(1, ...data.flatMap(item => trendSeries.map(([key]) => item[key] || 0)))
  const width = 600, height = 220, bottom = 195
  const x = index => data.length < 2 ? width / 2 : index * width / (data.length - 1)
  const y = value => bottom - (value / max) * 170
  return <div className={styles.chartWrap}><div className={styles.legend}>{trendSeries.map(([, label, color]) => <span key={label}><i style={{ background: color }} />{label}</span>)}</div><div className={styles.svgWrap}>{data.every(x => !x.applications && !x.interviews) && <b>No activity in this period</b>}<svg viewBox={`0 0 ${width} ${height}`} role="img" aria-label="Recruitment activity trend">{trendSeries.map(([key, label, color]) => <g key={key}><polyline fill="none" stroke={color} strokeWidth="3" points={data.map((item, index) => `${x(index)},${y(item[key] || 0)}`).join(' ')} />{data.map((item, index) => <circle key={item.period} cx={x(index)} cy={y(item[key] || 0)} r="4" fill={color}><title>{item.label} {label}: {item[key] || 0}</title></circle>)}</g>)}</svg></div><div className={styles.axis}>{data.map(x => <span key={x.period}>{x.label}</span>)}</div></div>
}

function StatusChart({ data }) {
  const max = Math.max(1, ...data.map(x => x.count))
  return <div className={styles.statusChart}>{data.map(item => <div key={item.status}><span>{item.status}</span><div><i style={{ width: `${item.count / max * 100}%` }} title={`${item.status}: ${item.count}`} /></div><strong>{item.count}</strong></div>)}</div>
}

export default function RecruiterAnalytics() {
  const today = new Date().toISOString().slice(0, 10)
  const sixMonthsAgo = new Date(); sixMonthsAgo.setMonth(sixMonthsAgo.getMonth() - 5); sixMonthsAgo.setDate(1)
  const [range, setRange] = useState({ from: sixMonthsAgo.toISOString().slice(0, 10), to: today })
  const [analytics, setAnalytics] = useState(blank), [loading, setLoading] = useState(true), [error, setError] = useState(''), [open, setOpen] = useState(false)
  const { user, logout } = useAuth(), navigate = useNavigate()
  const load = useCallback(() => { const controller = new AbortController(); setLoading(true); setError(''); getRecruiterAnalytics(range, controller.signal).then(setAnalytics).catch(e => { if (e.name !== 'CanceledError') setError(e.response?.data?.detail || 'Could not load recruitment analytics.') }).finally(() => setLoading(false)); return () => controller.abort() }, [range])
  useEffect(() => load(), [load])
  const rateCards = [['applicationToShortlistRate', 'Application to Shortlist'], ['shortlistToInterviewRate', 'Shortlist to Interview'], ['interviewToHireRate', 'Interview to Hire']]
  return <DashboardLayout sidebar={<RecruiterSidebar isOpen={open} onClose={() => setOpen(false)} onLogout={() => { logout(); navigate('/login') }} />} navbar={<RecruiterNavbar user={user} onMenuClick={() => setOpen(true)} />}><header className={styles.heading}><div><h1>Recruitment Analytics</h1><p>Track hiring performance across your jobs.</p></div><div className={styles.range}><label>From<input type="date" value={range.from} max={range.to} onChange={e => setRange(x => ({ ...x, from: e.target.value }))} /></label><label>To<input type="date" value={range.to} min={range.from} max={today} onChange={e => setRange(x => ({ ...x, to: e.target.value }))} /></label></div></header>{error && <div className={styles.error}>{error}<button onClick={load}>Retry</button></div>}{loading ? <div className={styles.skeleton}>Loading analytics…</div> : <><section className={styles.stats}>{summaryCards.map(([key, label]) => <article key={key}><span>{label}</span><strong>{analytics.summary[key] ?? 0}</strong></article>)}</section><div className={styles.charts}><section><h2>Applications Trend</h2><TrendChart data={analytics.applicationsTrend} /></section><section><h2>Applications by Status</h2><StatusChart data={analytics.applicationsByStatus} /></section></div><section className={styles.conversions}><header><h2>Recruitment Conversion</h2></header><div>{rateCards.map(([key, label]) => <article key={key}><span>{label}</span><strong>{analytics.conversionRates[key] ?? 0}%</strong><div><i style={{ width: `${Math.min(100, analytics.conversionRates[key] || 0)}%` }} /></div></article>)}</div></section><section className={styles.topJobs}><h2>Top-performing Jobs</h2>{analytics.topJobs.length === 0 ? <p>No job activity in this period.</p> : <div><table><thead><tr><th>Job</th><th>Applications</th><th>Shortlisted</th><th>Interviews</th><th>Hired</th></tr></thead><tbody>{analytics.topJobs.map(job => <tr key={job.jobId}><td>{job.jobTitle}</td><td>{job.applicationCount}</td><td>{job.shortlistedCount}</td><td>{job.interviewCount}</td><td>{job.hiredCount}</td></tr>)}</tbody></table></div>}</section></>}</DashboardLayout>
}
