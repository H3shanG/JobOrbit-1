import apiClient from '../api/apiClient'
export const emptyRecruiterStats={totalJobs:0,totalApplications:0,totalCandidates:0,interviewsThisMonth:0}
export async function getRecruiterDashboardStats(signal){const {data}=await apiClient.get('/dashboard/recruiter/stats',{signal});return {...emptyRecruiterStats,...data}}
export async function getRecruiterRecentApplicants(signal){const {data}=await apiClient.get('/dashboard/recruiter/recent-applicants',{signal});return Array.isArray(data)?data:[]}
export async function getRecruiterUpcomingInterviews(signal){const {data}=await apiClient.get('/dashboard/recruiter/upcoming-interviews',{signal});return Array.isArray(data)?data:[]}
export async function getRecruiterApplicationsOverview(signal){const {data}=await apiClient.get('/dashboard/recruiter/applications-overview',{signal});return Array.isArray(data?.months)?data.months:[]}
