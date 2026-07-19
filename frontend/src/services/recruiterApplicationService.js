import apiClient from '../api/apiClient'
export async function getRecruiterApplications(params,signal){const{data}=await apiClient.get('/recruiter/applications',{params,signal});return data}
export async function getRecruiterApplication(id,signal){const{data}=await apiClient.get(`/recruiter/applications/${id}`,{signal});return data}
export async function updateRecruiterApplicationStatus(id,status){await apiClient.patch(`/recruiter/applications/${id}/status`,{status})}
export async function downloadRecruiterApplicationResume(id){const response=await apiClient.get(`/recruiter/applications/${id}/resume`,{responseType:'blob'});const disposition=response.headers['content-disposition']||'';const match=disposition.match(/filename\*?=(?:UTF-8''|"?)([^";]+)/i);return{blob:response.data,fileName:match?decodeURIComponent(match[1].replace(/"/g,'')):'resume'}}
