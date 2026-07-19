import apiClient from '../api/apiClient'
export const getCandidateJobMatch=(jobId,signal)=>apiClient.get(`/candidate/jobs/${jobId}/match`,{signal}).then(x=>x.data)
export const getRankedApplicants=(jobId,params,signal)=>apiClient.get(`/recruiter/jobs/${jobId}/ranked-applicants`,{params,signal}).then(x=>x.data)
export const getRecruiterApplicationMatch=(applicationId,signal)=>apiClient.get(`/recruiter/applications/${applicationId}/match`,{signal}).then(x=>x.data)
export const getManagerApplicationMatch=(applicationId,signal)=>apiClient.get(`/manager/applications/${applicationId}/match`,{signal}).then(x=>x.data)
