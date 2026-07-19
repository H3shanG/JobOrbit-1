import DashboardIcon from './DashboardIcon'
import { useLocation, useNavigate } from 'react-router-dom'
import styles from '../../pages/CandidateDashboard.module.css'

const navigationItems = [
  { label: 'Dashboard', icon: 'dashboard', path: '/candidate/dashboard' },
  { label: 'Jobs', icon: 'briefcase', path: '/candidate/jobs' },
  { label: 'My Applications', icon: 'applications', path: '/candidate/applications' },
  { label: 'My Resume', icon: 'resume', path: '/candidate/resume' },
  { label: 'Profile', icon: 'user', path: '/candidate/profile' },
  { label: 'Settings', icon: 'settings', path: '/candidate/settings' },
]

export default function Sidebar({ isOpen, onClose, onLogout }) {
  const location = useLocation()
  const navigate = useNavigate()
  return (
    <>
      <button className={`${styles.sidebarOverlay} ${isOpen ? styles.sidebarOverlayVisible : ''}`} type="button" onClick={onClose} aria-label="Close navigation" />
      <aside className={`${styles.sidebar} ${isOpen ? styles.sidebarOpen : ''}`}>
        <div className={styles.logo}>
          <span className={styles.logoMark}>✦</span>
          <span>JobOrbit</span>
        </div>

        <nav className={styles.sidebarNav} aria-label="Candidate navigation">
          {navigationItems.map((item) => (
            <button
              className={`${styles.navItem} ${item.path && location.pathname.startsWith(item.path) ? styles.navItemActive : ''}`}
              key={item.label}
              type="button"
              onClick={item.path ? () => { navigate(item.path); onClose() } : undefined}
              aria-current={item.path && location.pathname.startsWith(item.path) ? 'page' : undefined}
            >
              <DashboardIcon name={item.icon} />
              <span>{item.label}</span>
            </button>
          ))}
        </nav>

        <button className={styles.logoutButton} type="button" onClick={onLogout}>
          <DashboardIcon name="logout" />
          <span>Logout</span>
        </button>
      </aside>
    </>
  )
}
