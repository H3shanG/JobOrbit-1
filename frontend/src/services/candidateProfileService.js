import apiClient from '../api/apiClient'

export async function getCandidateProfile(signal) {
  const { data } = await apiClient.get('/candidates/me', { signal })
  return data
}

export async function updateCandidateProfile(profile) {
  const { data } = await apiClient.put('/candidates/me', profile)
  return data
}
