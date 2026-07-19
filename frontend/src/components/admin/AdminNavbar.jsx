import DashboardIcon from '../candidate/DashboardIcon'
import NotificationBell from '../notifications/NotificationBell'
import styles from '../../pages/AdminDashboard.module.css'

export default function AdminNavbar({ user, onMenuClick }) {
  return <header className={styles.navbar}><button onClick={onMenuClick} aria-label="Open navigation"><DashboardIcon name="menu"/></button><label><DashboardIcon name="search"/><input placeholder="Search users, departments…" disabled/></label><NotificationBell/><span><b>{user?.fullName || 'Admin User'}</b><small>Administrator</small></span></header>
}
