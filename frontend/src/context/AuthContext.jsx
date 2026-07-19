import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import {
  clearAuthentication,
  getCurrentUser,
  getStoredToken,
  login as loginRequest,
} from '../services/authService'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let isActive = true

    async function restoreAuthentication() {
      if (!getStoredToken()) {
        setIsLoading(false)
        return
      }

      try {
        const currentUser = await getCurrentUser()
        if (isActive) setUser(currentUser)
      } catch {
        clearAuthentication()
        if (isActive) setUser(null)
      } finally {
        if (isActive) setIsLoading(false)
      }
    }

    restoreAuthentication()

    return () => {
      isActive = false
    }
  }, [])

  async function login(email, password) {
    const authentication = await loginRequest(email, password)
    setUser(authentication.user)
    return authentication.user
  }

  function logout() {
    clearAuthentication()
    setUser(null)
  }

  async function refreshUser() {
    const currentUser = await getCurrentUser()
    setUser(currentUser)
    return currentUser
  }

  const value = useMemo(
    () => ({
      user,
      isLoading,
      isAuthenticated: Boolean(user),
      login,
      logout,
      refreshUser,
    }),
    [user, isLoading],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// oxlint-disable-next-line react/only-export-components -- the hook and provider share one context.
export function useAuth() {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider.')
  }

  return context
}
