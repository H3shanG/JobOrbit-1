import NotificationBell from '../notifications/NotificationBell'
import styles from '../../pages/RecruiterDashboard.module.css'

export default function RecruiterNavbar({ user, onMenuClick }) {
  return <header className={styles.navbar}><button className={styles.menu} onClick={onMenuClick} aria-label="Open navigation">☰</button><div className={styles.search}>⌕ <span>Search candidates, jobs...</span></div><NotificationBell/><div className={styles.user}><span>{(user?.firstName?.[0] || 'R').toUpperCase()}</span><div><strong>{user?.fullName || 'Recruiter'}</strong><small>Recruiter</small></div><b>⌄</b></div></header>
}
