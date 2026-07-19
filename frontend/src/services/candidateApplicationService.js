import apiClient from '../api/apiClient'

export async function getCandidateApplications(params, signal) {
  const { data } = await apiClient.get('/candidates/me/applications', { params, signal })
  return {
    items: Array.isArray(data?.items) ? data.items : [], page: Number(data?.page) || 1,
    pageSize: Number(data?.pageSize) || 10, totalItems: Number(data?.totalItems) || 0,
    totalPages: Number(data?.totalPages) || 0,
    summary: { total: 0, pending: 0, shortlisted: 0, interviews: 0, rejected: 0, ...data?.summary },
  }
}

export async function getCandidateApplication(applicationId, signal) {
  const { data } = await apiClient.get(`/candidates/me/applications/${applicationId}`, { signal })
  return data
}
