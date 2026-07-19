import apiClient from '../api/apiClient'

export async function getHiringManagerDashboardStats(signal) {
  const { data } = await apiClient.get('/dashboard/hiring-manager/stats', { signal })
  return data
}
