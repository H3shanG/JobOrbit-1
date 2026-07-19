import apiClient from '../api/apiClient'
export async function getManagerEvaluations(applicationId, signal) { const { data } = await apiClient.get(`/manager/applications/${applicationId}/evaluations`, { signal }); return data }
export async function createManagerEvaluation(applicationId, payload) { const { data } = await apiClient.post(`/manager/applications/${applicationId}/evaluations`, payload); return data }
export async function updateManagerEvaluation(evaluationId, payload) { const { data } = await apiClient.put(`/manager/evaluations/${evaluationId}`, payload); return data }
export async function getManagerEvaluationSummary(signal) { const { data } = await apiClient.get('/dashboard/hiring-manager/evaluation-summary', { signal }); return data }
