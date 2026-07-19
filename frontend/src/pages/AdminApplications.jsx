import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import DashboardLayout from '../components/dashboard/DashboardLayout'
import AdminSidebar from '../components/admin/AdminSidebar'
import AdminNavbar from '../components/admin/AdminNavbar'
import { useAuth } from '../context/AuthContext'
import { getOrganizationLookup } from '../services/adminOrganizationService'
import { getDepartmentLookup } from '../services/adminDepartmentService'
import { getAdminUsers } from '../services/adminUserService'
import { getAdminJobs } from '../services/adminJobService'
import { downloadAdminApplicationResume, getAdminApplications, overrideAdminApplicationStatus } from '../services/adminApplicationService'
import styles from './AdminApplications.module.css'

const statuses = ['Submitted','UnderReview','Shortlisted','Interviewing','Offered','Hired','Rejected','Withdrawn']
const targets = { Submitted:['UnderReview'], UnderReview:['Shortlisted','Rejected'], Shortlisted:['Rejected'], Interviewing:['Rejected'] }

function OverrideDialog({ item, onClose, onSaved }) {
  const [status,setStatus]=useState(''), [reason,setReason]=useState(''), [saving,setSaving]=useState(false), [error,setError]=useState('')
  async function submit(event) {
    event.preventDefault()
    if (!window.confirm('Confirm this administrative correction? Related workflow records will be preserved.')) return
    setSaving(true); setError('')
    try { await overrideAdminApplicationStatus(item.applicationId,status,reason); onSaved() }
    catch (exception) { setError(exception.response?.data?.title || 'Status correction failed.') }
    finally { setSaving(false) }
  }
  return <div className={styles.modalBackdrop} role="presentation" onMouseDown={e=>e.target===e.currentTarget&&onClose()}>
    <form className={styles.modal} role="dialog" aria-modal="true" aria-labelledby="override-title" onSubmit={submit}>
      <h2 id="override-title">Override Application Status</h2><p><b>{item.candidateName}</b> for <b>{item.jobTitle}</b></p><p>Current: <span className={styles.badge}>{item.status}</span></p>
      <div className={styles.warning}>Administrative correction only. This is audited and cannot override a final hiring decision.</div>
      <label>Target status<select required value={status} onChange={e=>setStatus(e.target.value)}><option value="">Select allowed status</option>{(targets[item.status]||[]).map(x=><option key={x}>{x}</option>)}</select></label>
      <label>Reason<textarea required minLength="5" maxLength="500" value={reason} onChange={e=>setReason(e.target.value)}/></label>{error&&<p className={styles.error}>{error}</p>}
      <footer><button type="button" onClick={onClose}>Cancel</button><button disabled={saving||!status||reason.trim().length<5}>{saving?'Saving…':'Confirm correction'}</button></footer>
    </form>
  </div>
}

export default function AdminApplications() {
  const [params]=useSearchParams(), [open,setOpen]=useState(false), [orgs,setOrgs]=useState([]), [deps,setDeps]=useState([]), [jobs,setJobs]=useState([]), [recruiters,setRecruiters]=useState([]), [dialog,setDialog]=useState(null)
  const [query,setQuery]=useState({search:'',status:params.get('status')||'',organizationId:params.get('organizationId')||'',departmentId:params.get('departmentId')||'',jobId:params.get('jobId')||'',recruiterId:'',decision:'',from:'',to:'',sort:'newest',page:1,pageSize:10})
  const [result,setResult]=useState({items:[],page:1,totalPages:0}), [loading,setLoading]=useState(true), [error,setError]=useState('')
  const {user,logout}=useAuth(), navigate=useNavigate()
  useEffect(()=>{ getOrganizationLookup(true).then(setOrgs); getAdminUsers({role:'Recruiter',page:1,pageSize:100}).then(x=>setRecruiters(x.items)) },[])
  useEffect(()=>{ if(query.organizationId)getDepartmentLookup(query.organizationId,true).then(setDeps);else setDeps([]);getAdminJobs({organizationId:query.organizationId||undefined,departmentId:query.departmentId||undefined,page:1,pageSize:100}).then(x=>setJobs(x.items)) },[query.organizationId,query.departmentId])
  const load=useCallback(()=>{const controller=new AbortController();setLoading(true);setError('');getAdminApplications({...query,organizationId:query.organizationId||undefined,departmentId:query.departmentId||undefined,jobId:query.jobId||undefined,recruiterId:query.recruiterId||undefined,from:query.from||undefined,to:query.to||undefined},controller.signal).then(setResult).catch(e=>{if(e.name!=='CanceledError')setError(e.response?.data?.title||'Could not load applications.')}).finally(()=>setLoading(false));return()=>controller.abort()},[query])
  useEffect(()=>load(),[load])
  const set=(key,value)=>setQuery(current=>({...current,[key]:value,page:1,...(key==='organizationId'?{departmentId:'',jobId:'',recruiterId:''}:key==='departmentId'?{jobId:''}:{})}))
  function saved(){setDialog(null);load()}
  return <DashboardLayout sidebar={<AdminSidebar isOpen={open} onClose={()=>setOpen(false)} onLogout={()=>{logout();navigate('/login')}}/>} navbar={<AdminNavbar user={user} onMenuClick={()=>setOpen(true)}/>}>
    <header className={styles.heading}><div><h1>Applications Management</h1><p>Supervise applications without bypassing recruiter and hiring-manager workflows.</p></div></header>
    <div className={styles.filters}>
      <input aria-label="Search applications" placeholder="Search candidate, job, organization or recruiter" value={query.search} onChange={e=>set('search',e.target.value)}/>
      <select aria-label="Status" value={query.status} onChange={e=>set('status',e.target.value)}><option value="">All statuses</option>{statuses.map(x=><option key={x}>{x}</option>)}</select>
      <select aria-label="Organization" value={query.organizationId} onChange={e=>set('organizationId',e.target.value)}><option value="">All organizations</option>{orgs.map(x=><option key={x.organizationId} value={x.organizationId}>{x.name}</option>)}</select>
      <select aria-label="Department" disabled={!query.organizationId} value={query.departmentId} onChange={e=>set('departmentId',e.target.value)}><option value="">All departments</option>{deps.map(x=><option key={x.departmentId} value={x.departmentId}>{x.name}</option>)}</select>
      <select aria-label="Job" value={query.jobId} onChange={e=>set('jobId',e.target.value)}><option value="">All jobs</option>{jobs.map(x=><option key={x.jobId} value={x.jobId}>{x.title}</option>)}</select>
      <select aria-label="Recruiter" value={query.recruiterId} onChange={e=>set('recruiterId',e.target.value)}><option value="">All recruiters</option>{recruiters.filter(x=>!query.organizationId||String(x.organizationId)===String(query.organizationId)).map(x=><option key={x.userId} value={x.userId}>{x.fullName}</option>)}</select>
      <select aria-label="Decision" value={query.decision} onChange={e=>set('decision',e.target.value)}><option value="">All decisions</option>{['Pending','Hold','Hire','Reject'].map(x=><option key={x}>{x}</option>)}</select>
      <input aria-label="Applied from" type="date" value={query.from} onChange={e=>set('from',e.target.value)}/><input aria-label="Applied to" type="date" value={query.to} onChange={e=>set('to',e.target.value)}/>
      <select aria-label="Sort" value={query.sort} onChange={e=>set('sort',e.target.value)}><option value="newest">Newest</option><option value="oldest">Oldest</option><option value="updated">Recently updated</option><option value="candidate">Candidate name</option></select>
    </div>
    {error&&<div className={styles.state}>{error}<button onClick={load}>Retry</button></div>}
    {loading?<div className={styles.state}>Loading applications…</div>:result.items.length===0?<div className={styles.state}>No applications found.</div>:<div className={styles.table}><table><thead><tr><th>Candidate</th><th>Job</th><th>Organization</th><th>Recruiter</th><th>Status</th><th>Interview</th><th>Evaluation</th><th>Decision</th><th>Applied</th><th>Actions</th></tr></thead><tbody>{result.items.map(item=><tr key={item.applicationId}><td><b>{item.candidateName}</b><small>{item.candidateEmail}</small></td><td><b>{item.jobTitle}</b><small>{item.departmentName}</small></td><td>{item.organizationName}</td><td>{item.recruiterName}</td><td><span className={styles.badge}>{item.status}</span></td><td>{item.interviewStatus||'—'}</td><td>{item.evaluationStatus}</td><td>{item.decision}</td><td>{new Date(item.appliedAt).toLocaleDateString()}</td><td><button onClick={()=>navigate(`/admin/applications/${item.applicationId}`)}>View</button>{item.resumeId&&<button onClick={()=>downloadAdminApplicationResume(item.applicationId)}>Resume</button>}{(targets[item.status]||[]).length>0&&<button onClick={()=>setDialog(item)}>Override</button>}</td></tr>)}</tbody></table></div>}
    <div className={styles.pagination}><button disabled={result.page<=1} onClick={()=>setQuery(x=>({...x,page:x.page-1}))}>Previous</button><span>Page {result.page} of {Math.max(1,result.totalPages)}</span><button disabled={result.page>=result.totalPages} onClick={()=>setQuery(x=>({...x,page:x.page+1}))}>Next</button></div>
    {dialog&&<OverrideDialog item={dialog} onClose={()=>setDialog(null)} onSaved={saved}/>} 
  </DashboardLayout>
}
