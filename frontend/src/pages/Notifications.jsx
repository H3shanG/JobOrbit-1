import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import Sidebar from '../components/candidate/Sidebar'
import TopNavbar from '../components/candidate/TopNavbar'
import RecruiterSidebar from '../components/recruiter/RecruiterSidebar'
import RecruiterNavbar from '../components/recruiter/RecruiterNavbar'
import ManagerSidebar from '../components/manager/ManagerSidebar'
import ManagerNavbar from '../components/manager/ManagerNavbar'
import AdminSidebar from '../components/admin/AdminSidebar'
import AdminNavbar from '../components/admin/AdminNavbar'
import { useAuth } from '../context/AuthContext'
import { deleteNotification, getNotifications, markAllNotificationsRead, markNotificationRead } from '../services/notificationService'
import styles from './Notifications.module.css'

const safePath = path => typeof path === 'string' && ['/candidate/', '/recruiter/', '/manager/', '/admin/'].some(prefix => path.startsWith(prefix))
const formatDate = value => new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))

export default function Notifications() {
  const { user, logout } = useAuth(), navigate = useNavigate(), [open, setOpen] = useState(false)
  const [filter, setFilter] = useState('all'), [page, setPage] = useState(1), [data, setData] = useState(null), [loading, setLoading] = useState(true), [error, setError] = useState('')
  const load = useCallback(async signal => { setLoading(true); setError(''); try { setData(await getNotifications({ page, pageSize: 20, ...(filter === 'unread' ? { isRead: false } : filter === 'read' ? { isRead: true } : {}) }, signal)) } catch (e) { if (e.name !== 'CanceledError') setError('Could not load notifications.') } finally { setLoading(false) } }, [filter, page])
  useEffect(() => { const controller = new AbortController(); load(controller.signal); return () => controller.abort() }, [load])
  async function markRead(item) { await markNotificationRead(item.notificationId); await load(); if (safePath(item.actionUrl)) navigate(item.actionUrl) }
  async function remove(id) { await deleteNotification(id); await load() }
  async function readAll() { await markAllNotificationsRead(); await load() }
  const logoutUser = () => { logout(); navigate('/login') }
  const role = user?.role
  const sidebar = role === 'Candidate' ? <Sidebar isOpen={open} onClose={() => setOpen(false)} onLogout={logoutUser}/> : role === 'Recruiter' ? <RecruiterSidebar isOpen={open} onClose={() => setOpen(false)} onLogout={logoutUser}/> : role === 'HiringManager' ? <ManagerSidebar isOpen={open} onClose={() => setOpen(false)} onLogout={logoutUser}/> : <AdminSidebar isOpen={open} onClose={() => setOpen(false)} onLogout={logoutUser}/>
  const navbar = role === 'Candidate' ? <TopNavbar user={user} onMenuClick={() => setOpen(true)}/> : role === 'Recruiter' ? <RecruiterNavbar user={user} onMenuClick={() => setOpen(true)}/> : role === 'HiringManager' ? <ManagerNavbar user={user} onMenuClick={() => setOpen(true)}/> : <AdminNavbar user={user} onMenuClick={() => setOpen(true)}/>
  return <DashboardLayout sidebar={sidebar} navbar={navbar}><header className={styles.heading}><div><h1>Notifications</h1><p>Keep track of recruitment updates and actions.</p></div><button onClick={readAll} disabled={!data?.items.some(x => !x.isRead)}>Mark all as read</button></header><nav className={styles.filters} aria-label="Notification filters">{['all','unread','read'].map(value => <button key={value} className={filter === value ? styles.active : ''} onClick={() => { setFilter(value); setPage(1) }}>{value[0].toUpperCase() + value.slice(1)}</button>)}</nav>{loading ? <div className={styles.state}>Loading notifications…</div> : error ? <div className={styles.state}>{error}<button onClick={() => load()}>Retry</button></div> : data?.items.length === 0 ? <div className={styles.state}>No notifications found</div> : <div className={styles.list}>{data?.items.map(item => <article className={`${styles.card} ${!item.isRead ? styles.unread : ''}`} key={item.notificationId}><span className={`${styles.priority} ${styles[item.priority.toLowerCase()]}`}>{item.priority}</span><div><header><h2>{item.title}</h2>{!item.isRead && <b>Unread</b>}</header><p>{item.message}</p><time>{formatDate(item.createdAt)}</time></div><footer>{!item.isRead && <button onClick={() => markRead(item)}>Mark as read</button>}{item.actionUrl && <button onClick={() => markRead(item)}>Open</button>}<button className={styles.delete} onClick={() => remove(item.notificationId)}>Delete</button></footer></article>)}</div>}{data?.totalPages > 1 && <footer className={styles.pagination}><button disabled={page <= 1} onClick={() => setPage(x => x - 1)}>Previous</button><span>Page {page} of {data.totalPages}</span><button disabled={page >= data.totalPages} onClick={() => setPage(x => x + 1)}>Next</button></footer>}</DashboardLayout>
}
