import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import Sidebar from '../components/candidate/Sidebar'
import TopNavbar from '../components/candidate/TopNavbar'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import { useAuth } from '../context/AuthContext'
import { applyForJob, getJob } from '../services/jobService'
import { getResumes } from '../services/candidateResumeService'
import styles from './CandidateApply.module.css'

const maximumLength = 8000

export default function CandidateApplyPlaceholder() {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [job, setJob] = useState(null)
  const [coverLetter, setCoverLetter] = useState('')
  const [confirmed, setConfirmed] = useState(false)
  const [pageState, setPageState] = useState('loading')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [resumes, setResumes] = useState([])
  const [resumeId, setResumeId] = useState('')
  const { jobId } = useParams()
  const navigate = useNavigate()
  const { logout, user } = useAuth()

  const loadJob = useCallback(async (signal) => {
    setPageState('loading')
    try {
      setJob(await getJob(jobId, signal))
      setPageState('ready')
    } catch (requestError) {
      if (requestError.name !== 'CanceledError') setPageState(requestError.response?.status === 404 ? 'notFound' : 'error')
    }
  }, [jobId])

  useEffect(() => {
    const controller = new AbortController()
    loadJob(controller.signal)
    return () => controller.abort()
  }, [loadJob])

  useEffect(() => {
    const controller = new AbortController()
    getResumes(controller.signal).then((items) => {
      setResumes(items)
      const preferred = items.find((item) => item.isDefault)
      if (preferred) setResumeId(String(preferred.resumeId))
    }).catch(() => {})
    return () => controller.abort()
  }, [])

  const handleLogout = () => { logout(); navigate('/login', { replace: true }) }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    if (coverLetter.trim().length < 20) return setError('Please enter at least 20 characters.')
    if (!confirmed) return setError('Please confirm that your information is accurate.')
    setIsSubmitting(true)
    try {
      await applyForJob(jobId, coverLetter.trim(), resumeId ? Number(resumeId) : null)
      navigate(`/candidate/jobs/${jobId}`, { replace: true, state: { applicationSubmitted: true } })
    } catch (requestError) {
      if (requestError.response?.status === 409) {
        setError('You have already applied for this job.')
        setJob((current) => current ? { ...current, hasApplied: true } : current)
      } else if (requestError.response?.status === 404) setError('This job is no longer accepting applications.')
      else if (requestError.response?.status === 400) setError(requestError.response?.data?.detail || 'Please check your application details.')
      else setError('Unable to submit your application. Please try again.')
    } finally { setIsSubmitting(false) }
  }

  return <DashboardLayout sidebar={<Sidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)} onLogout={handleLogout} />} navbar={<TopNavbar onMenuClick={() => setIsSidebarOpen(true)} user={user} />}>
    <button className={styles.back} type="button" onClick={() => navigate(`/candidate/jobs/${jobId}`)}>← Back to Job Details</button>
    {pageState === 'loading' && <div className={styles.skeleton} aria-label="Loading application form" />}
    {pageState === 'notFound' && <div className={styles.state}><h1>Job unavailable</h1><p>This job is no longer accepting applications.</p><button type="button" onClick={() => navigate('/candidate/jobs')}>Browse Jobs</button></div>}
    {pageState === 'error' && <div className={styles.state}><h1>Unable to load application</h1><button type="button" onClick={() => loadJob()}>Retry</button></div>}
    {pageState === 'ready' && job && <div className={styles.applyGrid}>
      <form className={styles.formCard} onSubmit={handleSubmit}>
        <header><span>Application</span><h1>{job.title}</h1><p>{job.companyName} · {job.location}</p></header>
        {job.hasApplied ? <div className={styles.appliedMessage}>You have already applied for this position.</div> : <>
          <label className={styles.field}><span>Cover letter</span><textarea value={coverLetter} maxLength={maximumLength} onChange={(event) => setCoverLetter(event.target.value)} placeholder="Tell the hiring team why you're interested in this role..." disabled={isSubmitting} /><small>{coverLetter.length.toLocaleString()} / {maximumLength.toLocaleString()}</small></label>
          <div className={styles.resumeBox}><strong>Resume</strong>{resumes.length ? <select value={resumeId} onChange={(event) => setResumeId(event.target.value)}><option value="">No resume</option>{resumes.map((resume) => <option key={resume.resumeId} value={resume.resumeId}>{resume.displayName}{resume.isDefault ? ' (Default)' : ''}</option>)}</select> : <p>No saved resume. You may submit without one.</p>}</div>
          <label className={styles.confirm}><input type="checkbox" checked={confirmed} onChange={(event) => setConfirmed(event.target.checked)} disabled={isSubmitting} /><span>I confirm that the information in this application is accurate.</span></label>
          {error && <div className={styles.error} role="alert">{error}</div>}
          <div className={styles.actions}><button type="button" className={styles.cancel} onClick={() => navigate(`/candidate/jobs/${jobId}`)} disabled={isSubmitting}>Cancel</button><button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Submitting…' : 'Submit Application'}</button></div>
        </>}
      </form>
      <aside className={styles.summary}><h2>Before you apply</h2><dl><div><dt>Company</dt><dd>{job.companyName}</dd></div><div><dt>Department</dt><dd>{job.departmentName || 'General'}</dd></div><div><dt>Type</dt><dd>{job.employmentType}</dd></div><div><dt>Location</dt><dd>{job.location}</dd></div></dl><p>You can submit only one application for this job.</p></aside>
    </div>}
  </DashboardLayout>
}
