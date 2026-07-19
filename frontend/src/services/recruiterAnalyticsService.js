import apiClient from '../api/apiClient'

export async function getRecruiterAnalytics(params, signal) {
  const { data } = await apiClient.get('/recruiter/analytics', { params, signal })
  return data
}
