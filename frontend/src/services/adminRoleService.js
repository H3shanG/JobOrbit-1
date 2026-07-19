import apiClient from '../api/apiClient'
export const getAdminRoles=()=>apiClient.get('/admin/roles').then(x=>x.data)
export const getAdminRole=name=>apiClient.get(`/admin/roles/${encodeURIComponent(name)}`).then(x=>x.data)
export const getAdminPermissions=params=>apiClient.get('/admin/permissions',{params}).then(x=>x.data)
export const updateAdminRolePermissions=(name,permissionCodes)=>apiClient.put(`/admin/roles/${encodeURIComponent(name)}/permissions`,{permissionCodes}).then(x=>x.data)
export const resetAdminRolePermissions=name=>apiClient.post(`/admin/roles/${encodeURIComponent(name)}/permissions/reset`).then(x=>x.data)
