import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import Sidebar from '../components/candidate/Sidebar'
import TopNavbar from '../components/candidate/TopNavbar'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import { useAuth } from '../context/AuthContext'
import { getCandidateApplication } from '../services/candidateApplicationService'
import styles from './CandidateApplicationDetails.module.css'

const formatDate = (value, withTime = false) => value ? new Intl.DateTimeFormat(undefined, withTime ? { dateStyle: 'medium', timeStyle: 'short' } : { dateStyle: 'medium' }).format(new Date(value)) : '—'
const tone = (status) => ({ Shortlisted: 'success', Offered: 'success', Hired: 'success', Interviewing: 'interview', InterviewScheduled: 'interview', Rejected: 'danger', Withdrawn: 'muted' }[status] || 'pending')

export default function CandidateApplicationPlaceholder() {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [application, setApplication] = useState(null)
  const [state, setState] = useState('loading')
  const { applicationId } = useParams()
  const navigate = useNavigate()
  const { logout, user } = useAuth()

  const load = useCallback(async (signal) => {
    setState('loading')
    try { setApplication(await getCandidateApplication(applicationId, signal)); setState('ready') }
    catch (error) { if (error.name !== 'CanceledError') setState(error.response?.status === 404 ? 'notFound' : 'error') }
  }, [applicationId])

  useEffect(() => {
    const controller = new AbortController()
    load(controller.signal)
    return () => controller.abort()
  }, [load])

  const logoutUser = () => { logout(); navigate('/login', { replace: true }) }

  return <DashboardLayout sidebar={<Sidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)} onLogout={logoutUser} />} navbar={<TopNavbar onMenuClick={() => setIsSidebarOpen(true)} user={user} />}>
    <button className={styles.back} type="button" onClick={() => navigate('/candidate/applications')}>← Back to My Applications</button>
    {state === 'loading' && <div className={styles.skeleton} aria-label="Loading application details" />}
    {state === 'notFound' && <div className={styles.state}><h1>Application not found</h1><p>This application does not exist or is unavailable.</p><button type="button" onClick={() => navigate('/candidate/applications')}>My Applications</button></div>}
    {state === 'error' && <div className={styles.state}><h1>Unable to load application</h1><button type="button" onClick={() => load()}>Retry</button></div>}
    {state === 'ready' && application && <>
      <header className={styles.hero}><div><span>{application.companyName}</span><h1>{application.jobTitle}</h1><p>{application.departmentName} · {application.location} · {application.employmentType}</p></div><span className={`${styles.badge} ${styles[tone(application.status)]}`}>{application.status}</span></header>
      <div className={styles.grid}>
        <main className={styles.content}>
          <section><h2>Application progress</h2><ol className={styles.timeline}>{application.timeline?.map((item, index) => <li key={`${item.status}-${item.date}`}><span>{index + 1}</span><div><strong>{item.status}</strong><p>{item.description}</p><small>{formatDate(item.date, true)}</small></div></li>)}</ol></section>
          <section><h2>Cover letter</h2><p className={styles.cover}>{application.coverLetter || 'No cover letter was provided.'}</p></section>
        </main>
        <aside className={styles.side}>
          <section><h2>Application summary</h2><dl><div><dt>Applied</dt><dd>{formatDate(application.appliedOn)}</dd></div><div><dt>Last updated</dt><dd>{formatDate(application.updatedOn)}</dd></div><div><dt>Status</dt><dd>{application.status}</dd></div></dl><button type="button" onClick={() => navigate(`/candidate/jobs/${application.jobId}`)}>View Job</button></section>
          {application.interview && <section className={styles.interviewCard}><span>Interview</span><h2>{formatDate(application.interview.scheduledAt, true)}</h2><p>{application.interview.location || 'Location to be confirmed'}</p><strong>{application.interview.status}</strong>{application.interview.meetingLink && <a href={application.interview.meetingLink} target="_blank" rel="noreferrer">Open meeting link</a>}</section>}
        </aside>
      </div>
    </>}
  </DashboardLayout>
}
