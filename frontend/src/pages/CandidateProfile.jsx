import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Sidebar from '../components/candidate/Sidebar'
import TopNavbar from '../components/candidate/TopNavbar'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import { useAuth } from '../context/AuthContext'
import { getCandidateProfile, updateCandidateProfile } from '../services/candidateProfileService'
import styles from './CandidateProfile.module.css'

const fields = ['firstName', 'lastName', 'phone', 'address', 'professionalTitle', 'professionalSummary', 'education', 'experience', 'linkedinUrl', 'portfolioUrl']

export default function CandidateProfile() {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [profile, setProfile] = useState(null)
  const [saved, setSaved] = useState(null)
  const [state, setState] = useState('loading')
  const [message, setMessage] = useState('')
  const { logout, refreshUser, user } = useAuth()
  const navigate = useNavigate()

  const load = useCallback(async (signal) => {
    setState('loading')
    try { const data = await getCandidateProfile(signal); setProfile(data); setSaved(data); setState('ready') }
    catch (error) { if (error.name !== 'CanceledError') setState('error') }
  }, [])

  useEffect(() => { const controller = new AbortController(); load(controller.signal); return () => controller.abort() }, [load])
  const dirty = useMemo(() => profile && saved && fields.some((key) => (profile[key] || '') !== (saved[key] || '')), [profile, saved])
  useEffect(() => {
    const warn = (event) => { if (dirty) { event.preventDefault(); event.returnValue = '' } }
    window.addEventListener('beforeunload', warn); return () => window.removeEventListener('beforeunload', warn)
  }, [dirty])

  const change = (event) => setProfile((current) => ({ ...current, [event.target.name]: event.target.value }))
  const logoutUser = () => { logout(); navigate('/login', { replace: true }) }

  async function submit(event) {
    event.preventDefault(); setState('saving'); setMessage('')
    try {
      const updated = await updateCandidateProfile(Object.fromEntries(fields.map((key) => [key, profile[key] || null])))
      setProfile(updated); setSaved(updated); await refreshUser(); setState('ready'); setMessage('Profile saved successfully.')
    } catch (error) {
      setState('ready'); setMessage(error.response?.data?.detail || 'Unable to save your profile. Please check the fields and try again.')
    }
  }

  return <DashboardLayout sidebar={<Sidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)} onLogout={logoutUser} />} navbar={<TopNavbar onMenuClick={() => setIsSidebarOpen(true)} user={user} />}>
    <header className={styles.header}><div><h1>My Profile</h1><p>Keep your candidate information accurate and up to date.</p></div>{profile && <div className={styles.completion}><strong>{profile.profileCompletionPercentage}%</strong><span>Profile complete</span><i><b style={{ width: `${profile.profileCompletionPercentage}%` }} /></i></div>}</header>
    {state === 'loading' && <div className={styles.skeleton} />}
    {state === 'error' && <div className={styles.errorState}>Could not load your profile.<button type="button" onClick={() => load()}>Retry</button></div>}
    {profile && state !== 'loading' && <form className={styles.form} onSubmit={submit}>
      <section><h2>Personal Information</h2><div className={styles.grid}><label>First name<input required maxLength="100" name="firstName" value={profile.firstName || ''} onChange={change} /></label><label>Last name<input required maxLength="100" name="lastName" value={profile.lastName || ''} onChange={change} /></label><label>Email<input value={profile.email || ''} disabled /><small>Email changes are managed securely outside this profile.</small></label><label>Phone<input maxLength="30" name="phone" value={profile.phone || ''} onChange={change} placeholder="0771234567" /></label><label className={styles.full}>Address<input maxLength="200" name="address" value={profile.address || ''} onChange={change} placeholder="Colombo, Sri Lanka" /></label></div></section>
      <section><h2>Professional Information</h2><div className={styles.grid}><label className={styles.full}>Professional title<input maxLength="200" name="professionalTitle" value={profile.professionalTitle || ''} onChange={change} /></label><label className={styles.full}>Professional summary<textarea maxLength="2000" name="professionalSummary" value={profile.professionalSummary || ''} onChange={change} /></label><label className={styles.full}>Education<textarea maxLength="4000" name="education" value={profile.education || ''} onChange={change} /></label><label className={styles.full}>Experience<textarea maxLength="4000" name="experience" value={profile.experience || ''} onChange={change} /></label><label>LinkedIn URL<input type="url" maxLength="1000" name="linkedinUrl" value={profile.linkedinUrl || ''} onChange={change} /></label><label>Portfolio URL<input type="url" maxLength="1000" name="portfolioUrl" value={profile.portfolioUrl || ''} onChange={change} /></label></div></section>
      {message && <div className={message.startsWith('Profile saved') ? styles.success : styles.error} role="status">{message}</div>}
      <div className={styles.actions}><button type="button" className={styles.reset} disabled={!dirty || state === 'saving'} onClick={() => { setProfile(saved); setMessage('') }}>Reset</button><button type="submit" disabled={!dirty || state === 'saving'}>{state === 'saving' ? 'Saving…' : 'Save Changes'}</button></div>
    </form>}
  </DashboardLayout>
}
