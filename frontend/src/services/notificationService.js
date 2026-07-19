import apiClient from '../api/apiClient'

export async function getNotifications(params = {}, signal) {
  const { data } = await apiClient.get('/notifications', { params, signal })
  return data
}
export async function getUnreadCount(signal) {
  const { data } = await apiClient.get('/notifications/unread-count', { signal })
  return data.unreadCount
}
export async function markNotificationRead(id) { await apiClient.patch(`/notifications/${id}/read`) }
export async function markAllNotificationsRead() {
  const { data } = await apiClient.patch('/notifications/read-all')
  return data.updatedCount
}
export async function deleteNotification(id) { await apiClient.delete(`/notifications/${id}`) }
