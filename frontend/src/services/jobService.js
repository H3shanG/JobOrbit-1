import apiClient from '../api/apiClient'

export async function getJobs(params, signal) {
  const { data } = await apiClient.get('/jobs', { params, signal })
  return {
    items: Array.isArray(data?.items) ? data.items : [],
    page: Number(data?.page) || 1,
    pageSize: Number(data?.pageSize) || 10,
    totalItems: Number(data?.totalItems) || 0,
    totalPages: Number(data?.totalPages) || 0,
  }
}

export async function getJob(jobId, signal) {
  const { data } = await apiClient.get(`/jobs/${jobId}`, { signal })
  return data
}

export async function applyForJob(jobId, coverLetter, resumeId) {
  const { data } = await apiClient.post(`/jobs/${jobId}/applications`, { coverLetter, resumeId })
  return data
}
