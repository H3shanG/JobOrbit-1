import { useCallback, useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import DashboardIcon from '../candidate/DashboardIcon'
import { useAuth } from '../../context/AuthContext'
import { getNotifications, getUnreadCount, markAllNotificationsRead, markNotificationRead } from '../../services/notificationService'
import styles from './NotificationBell.module.css'

const safePath = path => typeof path === 'string' && ['/candidate/', '/recruiter/', '/manager/', '/admin/'].some(prefix => path.startsWith(prefix))
const relativeTime = value => { const seconds = Math.max(1, Math.floor((Date.now() - new Date(value).getTime()) / 1000)); if (seconds < 60) return 'Just now'; if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`; if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`; return `${Math.floor(seconds / 86400)}d ago` }

export default function NotificationBell() {
  const { isAuthenticated } = useAuth(), navigate = useNavigate(), root = useRef(null)
  const [open, setOpen] = useState(false), [count, setCount] = useState(0), [items, setItems] = useState([]), [loading, setLoading] = useState(false)
  const refreshCount = useCallback(signal => getUnreadCount(signal).then(setCount).catch(error => { if (error.name !== 'CanceledError') setCount(0) }), [])
  useEffect(() => { if (!isAuthenticated) return undefined; const controller = new AbortController(); refreshCount(controller.signal); const timer = setInterval(() => refreshCount(), 45000); return () => { controller.abort(); clearInterval(timer) } }, [isAuthenticated, refreshCount])
  useEffect(() => { const close = event => { if (!root.current?.contains(event.target)) setOpen(false) }; document.addEventListener('pointerdown', close); return () => document.removeEventListener('pointerdown', close) }, [])
  async function toggle() { const next = !open; setOpen(next); if (next) { setLoading(true); try { const data = await getNotifications({ page: 1, pageSize: 6 }); setItems(data.items) } finally { setLoading(false) } } }
  async function select(item) { if (!item.isRead) { await markNotificationRead(item.notificationId); setCount(x => Math.max(0, x - 1)) } setOpen(false); if (safePath(item.actionUrl)) navigate(item.actionUrl) }
  async function readAll() { await markAllNotificationsRead(); setItems(x => x.map(item => ({ ...item, isRead: true }))); setCount(0) }
  return <div className={styles.root} ref={root}><button className={styles.trigger} type="button" onClick={toggle} aria-label={count ? `Notifications, ${count} unread` : 'Notifications'} aria-expanded={open}><DashboardIcon name="bell" size={20}/>{count > 0 && <span className={styles.count}>{count > 99 ? '99+' : count}</span>}</button>{open && <section className={styles.dropdown} aria-label="Recent notifications"><header><strong>Notifications</strong>{count > 0 && <button onClick={readAll}>Mark all read</button>}</header><div className={styles.list}>{loading ? <p className={styles.state}>Loading…</p> : items.length === 0 ? <p className={styles.state}>No notifications yet</p> : items.map(item => <button key={item.notificationId} className={`${styles.item} ${!item.isRead ? styles.unread : ''}`} onClick={() => select(item)}><span className={`${styles.dot} ${styles[item.priority.toLowerCase()]}`}/><span><strong>{item.title}</strong><small>{item.message}</small><time>{relativeTime(item.createdAt)}</time></span></button>)}</div><footer><Link to="/notifications" onClick={() => setOpen(false)}>View all notifications</Link></footer></section>}</div>
}
