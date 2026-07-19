import apiClient from '../api/apiClient'
export const getAdminOrganizations=(params,signal)=>apiClient.get('/admin/organizations',{params,signal}).then(x=>x.data)
export const getAdminOrganization=(id,signal)=>apiClient.get(`/admin/organizations/${id}`,{signal}).then(x=>x.data)
export const getOrganizationLookup=(includeInactive=false)=>apiClient.get('/admin/organizations/lookup',{params:{includeInactive}}).then(x=>x.data)
export const createAdminOrganization=payload=>apiClient.post('/admin/organizations',payload).then(x=>x.data)
export const updateAdminOrganization=(id,payload)=>apiClient.put(`/admin/organizations/${id}`,payload).then(x=>x.data)
export const setAdminOrganizationStatus=(id,isActive,reason='')=>apiClient.patch(`/admin/organizations/${id}/status`,{isActive,reason}).then(x=>x.data)
