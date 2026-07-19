import apiClient from '../api/apiClient'

export async function getCandidateSettings(signal) { const { data } = await apiClient.get('/candidates/me/settings', { signal }); return data }
export async function updateCandidateSettings(settings) { const { data } = await apiClient.put('/candidates/me/settings', settings); return data }
export async function changeCandidatePassword(passwords) { await apiClient.put('/candidates/me/password', passwords) }
