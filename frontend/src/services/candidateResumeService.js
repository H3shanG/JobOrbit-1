import apiClient from '../api/apiClient'

export async function getResumes(signal) { const { data } = await apiClient.get('/candidates/me/resumes', { signal }); return Array.isArray(data) ? data : [] }
export async function uploadResume(file, displayName, onProgress) {
  const form = new FormData(); form.append('file', file); if (displayName.trim()) form.append('displayName', displayName.trim())
  const { data } = await apiClient.post('/candidates/me/resumes', form, { onUploadProgress: (e) => onProgress?.(e.total ? Math.round(e.loaded * 100 / e.total) : 0) }); return data
}
export async function setDefaultResume(id) { await apiClient.patch(`/candidates/me/resumes/${id}/default`) }
export async function deleteResume(id) { await apiClient.delete(`/candidates/me/resumes/${id}`) }
export async function downloadResume(resume) {
  const { data } = await apiClient.get(`/candidates/me/resumes/${resume.resumeId}`, { responseType: 'blob' }); const url = URL.createObjectURL(data); const link = document.createElement('a'); link.href = url; link.download = resume.originalFileName; link.click(); URL.revokeObjectURL(url)
}
