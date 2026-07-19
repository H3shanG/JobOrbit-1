import apiClient from '../api/apiClient'

export async function getHiringManagerCandidates(params, signal) {
  const { data } = await apiClient.get('/manager/candidates', { params, signal })
  return data
}
export async function getHiringManagerCandidate(id, signal) {
  const { data } = await apiClient.get(`/manager/candidates/${id}`, { signal })
  return data
}
export async function getHiringManagerDashboardCandidates(signal) {
  const { data } = await apiClient.get('/dashboard/hiring-manager/candidates-to-review', { signal })
  return data
}
export async function downloadHiringManagerResume(id) {
  const response = await apiClient.get(`/manager/candidates/${id}/resume`, { responseType: 'blob' })
  const disposition = response.headers['content-disposition'] || ''
  const match = disposition.match(/filename\*?=(?:UTF-8''|"?)([^";]+)/i)
  return { blob: response.data, fileName: match ? decodeURIComponent(match[1].replace(/"/g, '')) : 'resume' }
}
