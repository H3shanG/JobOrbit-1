import apiClient from '../api/apiClient'

const emptyStats = {
  jobsApplied: 0,
  interviews: 0,
  shortlisted: 0,
  pending: 0,
}

export async function getCandidateDashboardStats(signal) {
  const { data } = await apiClient.get('/dashboard/candidate/stats', { signal })

  return Object.fromEntries(
    Object.keys(emptyStats).map((key) => [key, Math.max(0, Number(data?.[key]) || 0)]),
  )
}

export async function getCandidateRecentApplications(signal) {
  const { data } = await apiClient.get('/dashboard/candidate/recent-applications', { signal })
  return Array.isArray(data) ? data : []
}

export { emptyStats }
