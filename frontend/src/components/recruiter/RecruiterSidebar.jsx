import { useLocation, useNavigate } from 'react-router-dom'
import DashboardIcon from '../candidate/DashboardIcon'
import styles from '../../pages/RecruiterDashboard.module.css'

const items = [
  ['Dashboard', 'dashboard', '/recruiter/dashboard'],
  ['Post a Job', 'calendar', '/recruiter/jobs/new'],
  ['Jobs', 'briefcase', '/recruiter/jobs'],
  ['Applicants', 'people', '/recruiter/applicants'],
  ['Interviews', 'applications', '/recruiter/interviews'],
  ['Analytics', 'sparkles', '/recruiter/analytics'],
  ['Settings', 'settings', '/recruiter/settings'],
]

export default function RecruiterSidebar({ isOpen, onClose, onLogout }) {
  const location = useLocation(), navigate = useNavigate()
  const active = (label, path) => path && (location.pathname === path ||
    (label === 'Applicants' && (location.pathname.startsWith('/recruiter/applicants/') || /\/recruiter\/jobs\/\d+\/applications/.test(location.pathname))) ||
    (label === 'Interviews' && location.pathname.startsWith('/recruiter/interviews/')))
  return <><button className={`${styles.overlay} ${isOpen ? styles.overlayOpen : ''}`} onClick={onClose} aria-label="Close navigation" /><aside className={`${styles.sidebar} ${isOpen ? styles.sidebarOpen : ''}`}><div className={styles.brand}><span>✦</span> JobOrbit</div><nav>{items.map(([label, icon, path]) => <button key={label} className={active(label, path) ? styles.active : ''} onClick={path ? () => { navigate(path); onClose() } : undefined}><DashboardIcon name={icon} /><span>{label}</span></button>)}</nav><button className={styles.logout} onClick={onLogout}><DashboardIcon name="logout" /><span>Logout</span></button></aside></>
}
