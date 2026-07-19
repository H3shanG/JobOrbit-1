import apiClient from '../api/apiClient'

export async function getRecruiterSettings(signal) { const { data } = await apiClient.get('/recruiters/me/settings', { signal }); return data }
export async function updateRecruiterSettings(settings) { const { data } = await apiClient.put('/recruiters/me/settings', settings); return data }
export async function changeRecruiterPassword(passwords) { await apiClient.put('/recruiters/me/password', passwords) }
