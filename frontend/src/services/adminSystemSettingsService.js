import apiClient from '../api/apiClient'
export const getSystemSettings=(signal)=>apiClient.get('/admin/system-settings',{signal}).then(x=>x.data)
export const updateSystemSettings=(section,data)=>apiClient.put(`/admin/system-settings/${section}`,data).then(x=>x.data)
export const resetSystemSettings=(section)=>apiClient.post(`/admin/system-settings/reset/${section}`).then(x=>x.data)
export const getPublicPlatformSettings=(signal)=>apiClient.get('/platform-settings/public',{signal}).then(x=>x.data)
