import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getDashboardPath } from '../services/authService'

export default function RoleProtectedRoute({ allowedRoles, children }) {
  const { user } = useAuth()

  if (!user || !allowedRoles.includes(user.role)) {
    return <Navigate to={getDashboardPath(user?.role)} replace />
  }

  return children
}
