import DashboardIcon from '../candidate/DashboardIcon'
import NotificationBell from '../notifications/NotificationBell'
import styles from '../../pages/HiringManagerDashboard.module.css'

export default function ManagerNavbar({ user, onMenuClick }) {
  const name = user?.fullName || `${user?.firstName || ''} ${user?.lastName || ''}`.trim() || 'Hiring Manager'
  return <header className={styles.navbar}><button className={styles.menu} onClick={onMenuClick} aria-label="Open navigation"><DashboardIcon name="menu" /></button><div className={styles.search}><DashboardIcon name="search" size={15} /><span>Search candidates…</span></div><NotificationBell/><div className={styles.user}><span>{name[0]}</span><div><strong>{name}</strong><small>Hiring Manager</small></div><DashboardIcon name="chevron" size={14} /></div></header>
}
