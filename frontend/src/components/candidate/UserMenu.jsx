import DashboardIcon from './DashboardIcon'
import styles from '../../pages/CandidateDashboard.module.css'

export default function UserMenu({ user }) {
  const fullName = user?.fullName || 'Candidate'
  const initials = fullName
    .split(' ')
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase()

  return (
    <button className={styles.userMenu} type="button" aria-label="Open user menu">
      <span className={styles.avatar}>{initials}</span>
      <span className={styles.userIdentity}>
        <strong>{fullName}</strong>
        <small>{user?.role || 'Candidate'}</small>
      </span>
      <DashboardIcon name="chevron" size={16} />
    </button>
  )
}
