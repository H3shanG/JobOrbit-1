import apiClient, { AUTH_TOKEN_KEY, AUTH_USER_KEY } from '../api/apiClient'

const roleNames = {
  1: 'Candidate',
  2: 'Recruiter',
  3: 'Administrator',
  4: 'HiringManager',
}

export function normalizeUser(user) {
  if (!user) return null

  return {
    ...user,
    role: roleNames[user.role] ?? user.role,
  }
}

export function getDashboardPath(role) {
  return {
    Candidate: '/candidate/dashboard',
    Recruiter: '/recruiter/dashboard',
    HiringManager: '/manager/dashboard',
    Administrator: '/admin/dashboard',
  }[role] ?? '/login'
}

export async function login(email, password) {
  const { data } = await apiClient.post('/auth/login', { email, password })
  localStorage.setItem(AUTH_TOKEN_KEY, data.token)

  const user = await getCurrentUser()
  localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user))

  return { token: data.token, user }
}

export async function getCurrentUser() {
  const { data } = await apiClient.get('/auth/me')
  return normalizeUser(data)
}

export function getStoredToken() {
  return localStorage.getItem(AUTH_TOKEN_KEY)
}

export function clearAuthentication() {
  localStorage.removeItem(AUTH_TOKEN_KEY)
  localStorage.removeItem(AUTH_USER_KEY)
}
