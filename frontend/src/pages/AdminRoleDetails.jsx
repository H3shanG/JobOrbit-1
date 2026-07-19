import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import AdminSidebar from '../components/admin/AdminSidebar'
import AdminNavbar from '../components/admin/AdminNavbar'
import { useAuth } from '../context/AuthContext'
import { getAdminRole, resetAdminRolePermissions, updateAdminRolePermissions } from '../services/adminRoleService'
import styles from './AdminRoles.module.css'

export default function AdminRoleDetails() {
  const { roleName } = useParams()
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)
  const [role, setRole] = useState(null)
  const [selected, setSelected] = useState(new Set())
  const [initial, setInitial] = useState(new Set())
  const [search, setSearch] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')
  const changed = useMemo(() => selected.size !== initial.size || [...selected].some(x => !initial.has(x)), [selected, initial])

  useEffect(() => {
    const warn = event => { if (changed) { event.preventDefault(); event.returnValue = '' } }
    window.addEventListener('beforeunload', warn)
    return () => window.removeEventListener('beforeunload', warn)
  }, [changed])

  const apply = value => {
    setRole(value)
    const assigned = new Set(value.permissions.filter(x => x.isAssigned).map(x => x.code))
    setSelected(assigned)
    setInitial(new Set(assigned))
  }
  const load = useCallback(() => {
    setError('')
    getAdminRole(roleName).then(apply).catch(e => setError(e.response?.status === 404 ? 'Role not found.' : 'Could not load role permissions.'))
  }, [roleName])
  useEffect(load, [load])

  const groups = useMemo(() => {
    const result = {}
    for (const permission of role?.permissions || []) {
      if (search && !`${permission.displayName} ${permission.code} ${permission.description}`.toLowerCase().includes(search.toLowerCase())) continue
      ;(result[permission.category] ??= []).push(permission)
    }
    return result
  }, [role, search])
  const toggle = permission => {
    if (permission.isRequired || !permission.isCompatible) return
    setSelected(old => { const next = new Set(old); next.has(permission.code) ? next.delete(permission.code) : next.add(permission.code); return next })
  }
  const setCategory = (permissions, enabled) => setSelected(old => {
    const next = new Set(old)
    permissions.forEach(permission => { if (!permission.isRequired && permission.isCompatible) enabled ? next.add(permission.code) : next.delete(permission.code) })
    return next
  })
  async function save() {
    const added = [...selected].filter(x => !initial.has(x)).length
    const removed = [...initial].filter(x => !selected.has(x)).length
    if (!window.confirm(`Save permissions for ${role.roleName}?\nAdded: ${added}\nRemoved: ${removed}`)) return
    setBusy(true); setError('')
    try { apply(await updateAdminRolePermissions(roleName, [...selected])); setMessage('Permissions updated successfully.') }
    catch (e) { setError(e.response?.data?.title || 'Permission update failed.') }
    finally { setBusy(false) }
  }
  async function reset() {
    if (!window.confirm(`Reset ${role.roleName} to its default permissions? This replaces all editable assignments.`)) return
    setBusy(true); setError('')
    try { apply(await resetAdminRolePermissions(roleName)); setMessage('Default permissions restored.') }
    catch (e) { setError(e.response?.data?.title || 'Permission reset failed.') }
    finally { setBusy(false) }
  }
  const leave = () => { if (!changed || window.confirm('Discard unsaved permission changes?')) navigate('/admin/roles') }

  return <DashboardLayout sidebar={<AdminSidebar isOpen={open} onClose={() => setOpen(false)} onLogout={() => { logout(); navigate('/login') }} />} navbar={<AdminNavbar user={user} onMenuClick={() => setOpen(true)} />}>
    <button className={styles.back} onClick={leave}>← Back to Roles</button>
    {error && <div className={styles.error}>{error}<button onClick={load}>Retry</button></div>}
    {role && <>
      <header className={styles.detailHead}><div><span>System Role</span><h1>{role.displayName}</h1><p>{role.description}</p></div><dl><dt>Users</dt><dd>{role.userCount}</dd><dt>Assigned</dt><dd>{selected.size}</dd></dl></header>
      {role.roleName === 'Admin' && <div className={styles.warning}><b>Critical Admin permissions are protected.</b> They cannot be removed, ensuring role management can always be repaired.</div>}
      <div className={styles.toolbar}><input aria-label="Search permissions" placeholder="Search permissions" value={search} onChange={e => setSearch(e.target.value)} /><span>{changed ? 'Unsaved changes' : 'All changes saved'}</span></div>
      {Object.entries(groups).map(([category, permissions]) => <section className={styles.group} key={category}><header><h2>{category}</h2><div><button onClick={() => setCategory(permissions, true)}>Select category</button><button onClick={() => setCategory(permissions, false)}>Clear category</button></div></header>{permissions.map(permission => <label key={permission.code} className={!permission.isCompatible ? styles.disabled : ''}><input type="checkbox" checked={selected.has(permission.code)} disabled={permission.isRequired || !permission.isCompatible} onChange={() => toggle(permission)} /><span><b>{permission.displayName}</b><small>{permission.description}</small><code>{permission.code}</code></span>{permission.isRequired && <em>Required</em>}{!permission.isCompatible && <em>{permission.disabledReason || 'Not compatible'}</em>}</label>)}</section>)}
      <footer className={styles.actions}><button className={styles.secondary} onClick={() => { setSelected(new Set(initial)); setMessage('') }} disabled={!changed || busy}>Cancel Changes</button><button className={styles.secondary} onClick={reset} disabled={busy}>Reset to Defaults</button><button className={styles.primary} onClick={save} disabled={!changed || busy}>{busy ? 'Saving…' : 'Save Changes'}</button></footer>
      {message && <p className={styles.success}>{message}</p>}
    </>}
  </DashboardLayout>
}
