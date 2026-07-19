import { useLocation, useNavigate } from 'react-router-dom'
import DashboardIcon from '../candidate/DashboardIcon'
import styles from '../../pages/HiringManagerDashboard.module.css'

const items = [
  ['Dashboard', 'dashboard', '/manager/dashboard'],
  ['Candidates to Review', 'people', '/manager/candidates'],
  ['Interviews', 'calendar', '/manager/interviews'],
  ['Reports', 'briefcase', '/manager/reports'],
  ['Settings', 'settings', '/manager/settings'],
]

export default function ManagerSidebar({ isOpen, onClose, onLogout }) {
  const location = useLocation(), navigate = useNavigate()
  return <><button className={`${styles.overlay} ${isOpen ? styles.overlayOpen : ''}`} onClick={onClose} aria-label="Close navigation" /><aside className={`${styles.sidebar} ${isOpen ? styles.sidebarOpen : ''}`}><div className={styles.brand}><span>✦</span> JobOrbit</div><nav>{items.map(([label, icon, path]) => { const active = path === '/manager/dashboard' ? location.pathname === path : path && location.pathname.startsWith(path); return <button key={label} className={active ? styles.active : ''} onClick={path ? () => { navigate(path); onClose() } : undefined}><DashboardIcon name={icon} /><span>{label}</span></button> })}</nav><button className={styles.logout} onClick={onLogout}><DashboardIcon name="logout" /><span>Logout</span></button></aside></>
}
