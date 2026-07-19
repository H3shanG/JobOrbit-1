import apiClient from '../api/apiClient'
export async function getRecruiterDepartments(signal){const{data}=await apiClient.get('/recruiter/departments',{signal});return data}
export async function getRecruiterSkills(signal){const{data}=await apiClient.get('/recruiter/skills',{signal});return data}
export async function createRecruiterJob(request){const{data}=await apiClient.post('/recruiter/jobs',request);return data}
export async function getRecruiterJob(id,signal){const{data}=await apiClient.get(`/recruiter/jobs/${id}`,{signal});return data}
export async function getRecruiterJobs(params,signal){const{data}=await apiClient.get('/recruiter/jobs',{params,signal});return data}
export async function updateRecruiterJob(id,request){await apiClient.put(`/recruiter/jobs/${id}`,request)}
export async function publishRecruiterJob(id){await apiClient.patch(`/recruiter/jobs/${id}/publish`)}
export async function closeRecruiterJob(id){await apiClient.patch(`/recruiter/jobs/${id}/close`)}
export async function deleteRecruiterJob(id){await apiClient.delete(`/recruiter/jobs/${id}`)}
