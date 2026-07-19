import apiClient from '../api/apiClient'

export async function askJobAssistant(jobId, mode, signal) {
  const { data } = await apiClient.post(`/jobs/${jobId}/assistant`, { mode }, { signal })
  return {
    mode: data?.mode ?? mode,
    intro: data?.intro ?? '',
    highlights: Array.isArray(data?.highlights) ? data.highlights : [],
    interviewQuestions: Array.isArray(data?.interviewQuestions) ? data.interviewQuestions : [],
    usedAi: Boolean(data?.usedAi),
    note: data?.note ?? '',
  }
}
