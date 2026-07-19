import styles from '../../pages/CandidateDashboard.module.css'

export default function ApplicationsTable({ applications, isLoading, hasError, onRetry }) {
  return (
    <section className={`${styles.contentPanel} ${styles.applicationsPanel}`}>
      <div className={styles.panelHeading}>
        <h2>Recent Applications</h2>
        <button type="button">View All</button>
      </div>
      <div className={styles.tableScroller}>
        <table className={styles.applicationsTable}>
          <thead>
            <tr>
              <th>Job Title</th>
              <th>Company</th>
              <th>Status</th>
              <th>Applied On</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && Array.from({ length: 3 }, (_, index) => (
              <tr key={`loading-${index}`} aria-hidden="true">
                <td colSpan="4"><span className={styles.tableLoading} /></td>
              </tr>
            ))}
            {!isLoading && hasError && (
              <tr><td colSpan="4" className={styles.tableMessage}>Could not load applications. <button type="button" onClick={onRetry}>Retry</button></td></tr>
            )}
            {!isLoading && !hasError && applications.length === 0 && (
              <tr><td colSpan="4" className={styles.tableMessage}>No applications yet</td></tr>
            )}
            {!isLoading && !hasError && applications.map((application) => (
              <tr key={application.applicationId}>
                <td>{application.title}</td>
                <td>{application.company}</td>
                <td><span className={`${styles.status} ${styles[`status${application.tone}`]}`}>{application.status}</span></td>
                <td>{application.date}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
