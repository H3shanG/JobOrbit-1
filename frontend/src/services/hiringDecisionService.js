import apiClient from '../api/apiClient'
export async function getHiringDecisions(params,signal){const{data}=await apiClient.get('/manager/hiring-decisions',{params,signal});return data}
export async function getHiringDecision(id,signal){const{data}=await apiClient.get(`/manager/hiring-decisions/${id}`,{signal});return data}
export async function createHiringDecision(id,payload){const{data}=await apiClient.post(`/manager/applications/${id}/hiring-decision`,payload);return data}
export async function updateHiringDecision(id,payload){const{data}=await apiClient.put(`/manager/applications/${id}/hiring-decision`,payload);return data}
export async function getHiringFunnel(signal){const{data}=await apiClient.get('/dashboard/hiring-manager/hiring-funnel',{signal});return data}
