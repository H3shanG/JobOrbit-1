import { useCallback, useEffect, useState } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import Sidebar from '../components/candidate/Sidebar'
import TopNavbar from '../components/candidate/TopNavbar'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import { useAuth } from '../context/AuthContext'
import { getJob } from '../services/jobService'
import { getCandidateJobMatch } from '../services/jobMatchingService'
import MatchAnalysis from '../components/MatchAnalysis'
import JobAssistantCard from '../components/JobAssistantCard'
import styles from './CandidateJobDetails.module.css'

const formatDate = (value) => value ? new Intl.DateTimeFormat(undefined, { day: '2-digit', month: 'long', year: 'numeric' }).format(new Date(value)) : 'Open'
const formatSalary = (value) => new Intl.NumberFormat(undefined, { maximumFractionDigits: 0 }).format(value)

export default function CandidateJobDetails() {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [job, setJob] = useState(null)
  const [match, setMatch] = useState(null)
  const [state, setState] = useState('loading')
  const { jobId } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
  const { logout, user } = useAuth()

  const loadJob = useCallback(async (signal) => {
    setState('loading')
    try {
      const [jobData,matchData]=await Promise.all([getJob(jobId,signal),getCandidateJobMatch(jobId,signal)])
      setJob(jobData);setMatch(matchData)
      setState('ready')
    } catch (error) {
      if (error.name !== 'CanceledError') setState(error.response?.status === 404 ? 'notFound' : 'error')
    }
  }, [jobId])

  useEffect(() => {
    const controller = new AbortController()
    loadJob(controller.signal)
    return () => controller.abort()
  }, [loadJob])

  const handleLogout = () => { logout(); navigate('/login', { replace: true }) }
  const closed = job?.closingDate && new Date(job.closingDate) <= new Date()
  const actionLabel = job?.hasApplied ? 'Already Applied' : closed ? 'Applications Closed' : 'Apply Now'

  return <DashboardLayout
    sidebar={<Sidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)} onLogout={handleLogout} />}
    navbar={<TopNavbar onMenuClick={() => setIsSidebarOpen(true)} user={user} />}
  >
    <button className={styles.back} type="button" onClick={() => navigate('/candidate/jobs')}>← Back to Jobs</button>
    {state === 'loading' && <div className={styles.skeleton} aria-label="Loading job details" />}
    {state === 'notFound' && <div className={styles.state}><h1>Job not found</h1><p>This job may be unavailable or no longer accepting applications.</p><button type="button" onClick={() => navigate('/candidate/jobs')}>Browse Jobs</button></div>}
    {state === 'error' && <div className={styles.state}><h1>Unable to load this job</h1><p>Please try again.</p><button type="button" onClick={() => loadJob()}>Retry</button></div>}
    {state === 'ready' && job && <>
      {location.state?.applicationSubmitted && <div className={styles.applied}>Application submitted successfully.</div>}
      <header className={styles.hero}>
        <div><span className={styles.company}>{job.companyName}</span><h1>{job.title}</h1><p>{job.departmentName || 'General'} · {job.location} · {job.employmentType}</p></div>
        {job.hasApplied && <span className={styles.applied}>Applied</span>}
      </header>
      <div className={styles.detailsGrid}>
        <div className={styles.mainContent}>
          <section><h2>About the role</h2><p className={styles.copy}>{job.description || 'No description provided.'}</p></section>
          {job.responsibilities && <section><h2>Responsibilities</h2><p className={styles.copy}>{job.responsibilities}</p></section>}
          {job.requirements && <section><h2>Requirements</h2><p className={styles.copy}>{job.requirements}</p></section>}
          <section><h2>Skills</h2>{job.skills?.length ? <div className={styles.skills}>{job.skills.map((skill) => <span key={skill}>{skill}</span>)}</div> : <p className={styles.muted}>No specific skills listed.</p>}</section>
          <MatchAnalysis match={match}/>
          <JobAssistantCard jobId={job.jobId}/>
          {job.companySummary && <section><h2>About {job.companyName}</h2><p className={styles.copy}>{job.companySummary}</p></section>}
        </div>
        <aside className={styles.summaryCard}>
          <h2>Job overview</h2>
          <dl><div><dt>Location</dt><dd>{job.location}</dd></div><div><dt>Employment</dt><dd>{job.employmentType}</dd></div><div><dt>Posted</dt><dd>{formatDate(job.postedOn)}</dd></div><div><dt>Closing date</dt><dd>{formatDate(job.closingDate)}</dd></div>
          {(job.minimumSalary != null || job.maximumSalary != null) && <div><dt>Salary range</dt><dd>{job.minimumSalary != null ? formatSalary(job.minimumSalary) : '—'} – {job.maximumSalary != null ? formatSalary(job.maximumSalary) : '—'}</dd></div>}</dl>
          <button type="button" disabled={job.hasApplied || closed} onClick={() => navigate(`/candidate/jobs/${job.jobId}/apply`)}>{actionLabel}</button>
        </aside>
      </div>
    </>}
  </DashboardLayout>
}
