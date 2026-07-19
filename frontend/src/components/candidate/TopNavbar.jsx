import DashboardIcon from './DashboardIcon'
import UserMenu from './UserMenu'
import NotificationBell from '../notifications/NotificationBell'
import styles from '../../pages/CandidateDashboard.module.css'

export default function TopNavbar({ onMenuClick, user }) {
  return (
    <header className={styles.topNavbar}>
      <button className={styles.menuButton} type="button" onClick={onMenuClick} aria-label="Open navigation">
        <DashboardIcon name="menu" size={21} />
      </button>

      <label className={styles.searchBox}>
        <DashboardIcon name="search" size={17} />
        <input type="search" placeholder="Search for jobs, skills, companies..." aria-label="Search" />
      </label>

      <div className={styles.navbarActions}>
        <NotificationBell />
        <UserMenu user={user} />
      </div>
    </header>
  )
}
