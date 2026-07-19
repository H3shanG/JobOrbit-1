import apiClient from'../api/apiClient';
export const getAdminApplications=(params,signal)=>apiClient.get('/admin/applications',{params,signal}).then(x=>x.data);
export const getAdminApplication=(id,signal)=>apiClient.get(`/admin/applications/${id}`,{signal}).then(x=>x.data);
export const getAdminApplicationHistory=id=>apiClient.get(`/admin/applications/${id}/history`).then(x=>x.data);
export const overrideAdminApplicationStatus=(id,status,reason)=>apiClient.patch(`/admin/applications/${id}/status`,{status,reason}).then(x=>x.data);
export async function downloadAdminApplicationResume(id){const response=await apiClient.get(`/admin/applications/${id}/resume`,{responseType:'blob'}),url=URL.createObjectURL(response.data),a=document.createElement('a');a.href=url;const disposition=response.headers['content-disposition']||'',match=disposition.match(/filename\*?=(?:UTF-8''|\")?([^\";]+)/i);a.download=match?decodeURIComponent(match[1].replace(/\"/g,'')):'resume';a.click();URL.revokeObjectURL(url)}
