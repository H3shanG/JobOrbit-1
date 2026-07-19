import DashboardIcon from './DashboardIcon'
import styles from '../../pages/CandidateDashboard.module.css'

export default function StatCard({ label, value, caption, icon, tone, isLoading }) {
  return (
    <article className={styles.statCard} aria-busy={isLoading}>
      <div>
        <span className={`${styles.statLabel} ${styles[`toneText${tone}`]}`}>{label}</span>
        {isLoading ? <span className={styles.statValueSkeleton} aria-label="Loading" /> : <strong>{value}</strong>}
        <small>{caption}</small>
      </div>
      <span className={`${styles.statIcon} ${styles[`tone${tone}`]}`}>
        <DashboardIcon name={icon} size={23} />
      </span>
    </article>
  )
}
