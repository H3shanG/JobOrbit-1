import apiClient from '../api/apiClient'
export async function getHiringManagerInterviews(params,signal){const{data}=await apiClient.get('/manager/interviews',{params,signal});return data}
export async function getHiringManagerInterview(id,signal){const{data}=await apiClient.get(`/manager/interviews/${id}`,{signal});return data}
