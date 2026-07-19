import apiClient from '../api/apiClient'
export async function getRecruiterInterviews(params,signal){const{data}=await apiClient.get('/recruiter/interviews',{params,signal});return data}
export async function getRecruiterInterview(id,signal){const{data}=await apiClient.get(`/recruiter/interviews/${id}`,{signal});return data}
export async function getShortlistedApplications(signal){const{data}=await apiClient.get('/recruiter/interviews/shortlisted-applications',{signal});return data}
export async function createRecruiterInterview(request){const{data}=await apiClient.post('/recruiter/interviews',request);return data}
export async function updateRecruiterInterview(id,request){await apiClient.put(`/recruiter/interviews/${id}`,request)}
export async function cancelRecruiterInterview(id){await apiClient.patch(`/recruiter/interviews/${id}/cancel`)}
export async function completeRecruiterInterview(id){await apiClient.patch(`/recruiter/interviews/${id}/complete`)}
