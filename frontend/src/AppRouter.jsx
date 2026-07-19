import { Navigate, Route, Routes } from 'react-router-dom'
import HomePage from './App'
import ProtectedRoute from './components/ProtectedRoute'
import RoleProtectedRoute from './components/RoleProtectedRoute'
import AdminDashboard from './pages/AdminDashboard'
import AdminPlaceholder from './pages/AdminPlaceholder'
import AdminUsers from './pages/AdminUsers'
import AdminUserForm from './pages/AdminUserForm'
import AdminUserDetails from './pages/AdminUserDetails'
import AdminRoles from './pages/AdminRoles'
import AdminRoleDetails from './pages/AdminRoleDetails'
import AdminOrganizations from './pages/AdminOrganizations'
import AdminOrganizationForm from './pages/AdminOrganizationForm'
import AdminOrganizationDetails from './pages/AdminOrganizationDetails'
import AdminDepartments from './pages/AdminDepartments'
import AdminDepartmentForm from './pages/AdminDepartmentForm'
import AdminDepartmentDetails from './pages/AdminDepartmentDetails'
import AdminJobs from './pages/AdminJobs'
import AdminJobDetails from './pages/AdminJobDetails'
import AdminJobForm from './pages/AdminJobForm'
import AdminApplications from './pages/AdminApplications'
import AdminApplicationDetails from './pages/AdminApplicationDetails'
import AdminAuditLogs from './pages/AdminAuditLogs'
import AdminAuditLogDetails from './pages/AdminAuditLogDetails'
import AdminSystemSettings from './pages/AdminSystemSettings'
import CandidateDashboard from './pages/CandidateDashboard'
import CandidateJobs from './pages/CandidateJobs'
import CandidateJobDetails from './pages/CandidateJobDetails'
import CandidateApplyPlaceholder from './pages/CandidateApplyPlaceholder'
import CandidateApplications from './pages/CandidateApplications'
import CandidateApplicationPlaceholder from './pages/CandidateApplicationPlaceholder'
import CandidateProfile from './pages/CandidateProfile'
import CandidateResume from './pages/CandidateResume'
import CandidateSettings from './pages/CandidateSettings'
import HiringManagerDashboard from './pages/HiringManagerDashboard'
import HiringManagerCandidates from './pages/HiringManagerCandidates'
import HiringManagerCandidateDetails from './pages/HiringManagerCandidateDetails'
import HiringManagerEvaluation from './pages/HiringManagerEvaluation'
import HiringDecisionDetails from './pages/HiringDecisionDetails'
import HiringManagerReports from './pages/HiringManagerReports'
import HiringManagerSettings from './pages/HiringManagerSettings'
import HiringManagerInterviews from './pages/HiringManagerInterviews'
import HiringManagerInterviewDetails from './pages/HiringManagerInterviewDetails'
import LoginPage from './pages/LoginPage'
import RecruiterDashboard from './pages/RecruiterDashboard'
import RecruiterPostJob from './pages/RecruiterPostJob'
import RecruiterJobPlaceholder from './pages/RecruiterJobPlaceholder'
import RecruiterJobs from './pages/RecruiterJobs'
import RecruiterApplicants from './pages/RecruiterApplicants'
import RecruiterApplicantDetails from './pages/RecruiterApplicantDetails'
import RecruiterRankedApplicants from './pages/RecruiterRankedApplicants'
import RecruiterInterviews from './pages/RecruiterInterviews'
import RecruiterInterviewForm from './pages/RecruiterInterviewForm'
import RecruiterInterviewDetails from './pages/RecruiterInterviewDetails'
import RecruiterAnalytics from './pages/RecruiterAnalytics'
import RecruiterSettings from './pages/RecruiterSettings'
import Notifications from './pages/Notifications'

function ProtectedDashboard({ role, children }) {
  return (
    <ProtectedRoute>
      <RoleProtectedRoute allowedRoles={[role]}>{children}</RoleProtectedRoute>
    </ProtectedRoute>
  )
}

export default function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/notifications" element={<ProtectedRoute><Notifications /></ProtectedRoute>} />
      <Route
        path="/candidate/dashboard"
        element={<ProtectedDashboard role="Candidate"><CandidateDashboard /></ProtectedDashboard>}
      />
      <Route path="/candidate/recommendations" element={<ProtectedDashboard role="Candidate"><Navigate to="/candidate/dashboard" replace /></ProtectedDashboard>} />
      <Route path="/candidate/jobs/recommended" element={<ProtectedDashboard role="Candidate"><Navigate to="/candidate/dashboard" replace /></ProtectedDashboard>} />
      <Route path="/candidate/messages" element={<ProtectedDashboard role="Candidate"><Navigate to="/candidate/dashboard" replace /></ProtectedDashboard>} />
      <Route path="/candidate/jobs" element={<ProtectedDashboard role="Candidate"><CandidateJobs /></ProtectedDashboard>} />
      <Route path="/candidate/jobs/:jobId" element={<ProtectedDashboard role="Candidate"><CandidateJobDetails /></ProtectedDashboard>} />
      <Route path="/candidate/jobs/:jobId/apply" element={<ProtectedDashboard role="Candidate"><CandidateApplyPlaceholder /></ProtectedDashboard>} />
      <Route path="/candidate/applications" element={<ProtectedDashboard role="Candidate"><CandidateApplications /></ProtectedDashboard>} />
      <Route path="/candidate/applications/:applicationId" element={<ProtectedDashboard role="Candidate"><CandidateApplicationPlaceholder /></ProtectedDashboard>} />
      <Route path="/candidate/profile" element={<ProtectedDashboard role="Candidate"><CandidateProfile /></ProtectedDashboard>} />
      <Route path="/candidate/resume" element={<ProtectedDashboard role="Candidate"><CandidateResume /></ProtectedDashboard>} />
      <Route path="/candidate/settings" element={<ProtectedDashboard role="Candidate"><CandidateSettings /></ProtectedDashboard>} />
      <Route
        path="/recruiter/dashboard"
        element={<ProtectedDashboard role="Recruiter"><RecruiterDashboard /></ProtectedDashboard>}
      />
      <Route path="/recruiter/messages" element={<ProtectedDashboard role="Recruiter"><Navigate to="/recruiter/dashboard" replace /></ProtectedDashboard>} />
      <Route path="/recruiter/jobs/new" element={<ProtectedDashboard role="Recruiter"><RecruiterPostJob /></ProtectedDashboard>} />
      <Route path="/recruiter/jobs" element={<ProtectedDashboard role="Recruiter"><RecruiterJobs /></ProtectedDashboard>} />
      <Route path="/recruiter/jobs/:jobId/edit" element={<ProtectedDashboard role="Recruiter"><RecruiterPostJob editMode /></ProtectedDashboard>} />
      <Route path="/recruiter/jobs/:jobId/applications" element={<ProtectedDashboard role="Recruiter"><RecruiterApplicants /></ProtectedDashboard>} />
      <Route path="/recruiter/jobs/:jobId/ranked-applicants" element={<ProtectedDashboard role="Recruiter"><RecruiterRankedApplicants /></ProtectedDashboard>} />
      <Route path="/recruiter/applicants" element={<ProtectedDashboard role="Recruiter"><RecruiterApplicants /></ProtectedDashboard>} />
      <Route path="/recruiter/applicants/:applicationId" element={<ProtectedDashboard role="Recruiter"><RecruiterApplicantDetails /></ProtectedDashboard>} />
      <Route path="/recruiter/interviews" element={<ProtectedDashboard role="Recruiter"><RecruiterInterviews /></ProtectedDashboard>} />
      <Route path="/recruiter/interviews/new" element={<ProtectedDashboard role="Recruiter"><RecruiterInterviewForm /></ProtectedDashboard>} />
      <Route path="/recruiter/interviews/:interviewId" element={<ProtectedDashboard role="Recruiter"><RecruiterInterviewDetails /></ProtectedDashboard>} />
      <Route path="/recruiter/interviews/:interviewId/edit" element={<ProtectedDashboard role="Recruiter"><RecruiterInterviewForm editMode /></ProtectedDashboard>} />
      <Route path="/recruiter/analytics" element={<ProtectedDashboard role="Recruiter"><RecruiterAnalytics /></ProtectedDashboard>} />
      <Route path="/recruiter/settings" element={<ProtectedDashboard role="Recruiter"><RecruiterSettings /></ProtectedDashboard>} />
      <Route path="/recruiter/jobs/:jobId" element={<ProtectedDashboard role="Recruiter"><RecruiterJobPlaceholder /></ProtectedDashboard>} />
      <Route
        path="/manager/dashboard"
        element={<ProtectedDashboard role="HiringManager"><HiringManagerDashboard /></ProtectedDashboard>}
      />
      <Route path="/manager/candidates" element={<ProtectedDashboard role="HiringManager"><HiringManagerCandidates /></ProtectedDashboard>} />
      <Route path="/manager/candidates/:applicationId" element={<ProtectedDashboard role="HiringManager"><HiringManagerCandidateDetails /></ProtectedDashboard>} />
      <Route path="/manager/candidates/:applicationId/evaluate" element={<ProtectedDashboard role="HiringManager"><HiringManagerEvaluation /></ProtectedDashboard>} />
      <Route path="/manager/interviews" element={<ProtectedDashboard role="HiringManager"><HiringManagerInterviews /></ProtectedDashboard>} />
      <Route path="/manager/interviews/:interviewId" element={<ProtectedDashboard role="HiringManager"><HiringManagerInterviewDetails /></ProtectedDashboard>} />
      <Route path="/manager/evaluations" element={<ProtectedDashboard role="HiringManager"><Navigate to="/manager/candidates" replace /></ProtectedDashboard>} />
      <Route path="/manager/decisions" element={<ProtectedDashboard role="HiringManager"><Navigate to="/manager/candidates" replace /></ProtectedDashboard>} />
      <Route path="/manager/decisions/:applicationId" element={<ProtectedDashboard role="HiringManager"><HiringDecisionDetails /></ProtectedDashboard>} />
      <Route path="/manager/reports" element={<ProtectedDashboard role="HiringManager"><HiringManagerReports /></ProtectedDashboard>} />
      <Route path="/manager/settings" element={<ProtectedDashboard role="HiringManager"><HiringManagerSettings /></ProtectedDashboard>} />
      <Route
        path="/admin/dashboard"
        element={<ProtectedDashboard role="Administrator"><AdminDashboard /></ProtectedDashboard>}
      />
      <Route path="/admin/users" element={<ProtectedDashboard role="Administrator"><AdminUsers /></ProtectedDashboard>} />
      <Route path="/admin/users/new" element={<ProtectedDashboard role="Administrator"><AdminUserForm /></ProtectedDashboard>} />
      <Route path="/admin/users/:userId" element={<ProtectedDashboard role="Administrator"><AdminUserDetails /></ProtectedDashboard>} />
      <Route path="/admin/users/:userId/edit" element={<ProtectedDashboard role="Administrator"><AdminUserForm edit /></ProtectedDashboard>} />
      <Route path="/admin/roles" element={<ProtectedDashboard role="Administrator"><AdminRoles /></ProtectedDashboard>} />
      <Route path="/admin/roles/:roleName" element={<ProtectedDashboard role="Administrator"><AdminRoleDetails /></ProtectedDashboard>} />
      <Route path="/admin/organizations" element={<ProtectedDashboard role="Administrator"><AdminOrganizations /></ProtectedDashboard>} />
      <Route path="/admin/organizations/new" element={<ProtectedDashboard role="Administrator"><AdminOrganizationForm /></ProtectedDashboard>} />
      <Route path="/admin/organizations/:organizationId" element={<ProtectedDashboard role="Administrator"><AdminOrganizationDetails /></ProtectedDashboard>} />
      <Route path="/admin/organizations/:organizationId/edit" element={<ProtectedDashboard role="Administrator"><AdminOrganizationForm edit /></ProtectedDashboard>} />
      <Route path="/admin/departments" element={<ProtectedDashboard role="Administrator"><AdminDepartments /></ProtectedDashboard>} />
      <Route path="/admin/departments/new" element={<ProtectedDashboard role="Administrator"><AdminDepartmentForm /></ProtectedDashboard>} />
      <Route path="/admin/departments/:departmentId" element={<ProtectedDashboard role="Administrator"><AdminDepartmentDetails /></ProtectedDashboard>} />
      <Route path="/admin/departments/:departmentId/edit" element={<ProtectedDashboard role="Administrator"><AdminDepartmentForm edit /></ProtectedDashboard>} />
      <Route path="/admin/jobs" element={<ProtectedDashboard role="Administrator"><AdminJobs /></ProtectedDashboard>} />
      <Route path="/admin/jobs/:jobId" element={<ProtectedDashboard role="Administrator"><AdminJobDetails /></ProtectedDashboard>} />
      <Route path="/admin/jobs/:jobId/edit" element={<ProtectedDashboard role="Administrator"><AdminJobForm /></ProtectedDashboard>} />
      <Route path="/admin/applications" element={<ProtectedDashboard role="Administrator"><AdminApplications /></ProtectedDashboard>} />
      <Route path="/admin/applications/:applicationId" element={<ProtectedDashboard role="Administrator"><AdminApplicationDetails /></ProtectedDashboard>} />
      <Route path="/admin/audit-logs" element={<ProtectedDashboard role="Administrator"><AdminAuditLogs /></ProtectedDashboard>} />
      <Route path="/admin/audit-logs/:auditLogId" element={<ProtectedDashboard role="Administrator"><AdminAuditLogDetails /></ProtectedDashboard>} />
      <Route path="/admin/settings" element={<ProtectedDashboard role="Administrator"><AdminSystemSettings /></ProtectedDashboard>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
