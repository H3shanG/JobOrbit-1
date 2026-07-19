import styles from './DashboardLayout.module.css'

export default function DashboardLayout({ sidebar, navbar, children }) {
  return (
    <div className={styles.dashboardShell}>
      <div className={styles.sidebarSlot}>{sidebar}</div>
      <div className={styles.mainArea}>
        {navbar}
        <main className={styles.pageContent}>{children}</main>
      </div>
    </div>
  )
}
