import { createBrowserRouter, Navigate } from 'react-router-dom';
import { MainLayout } from '../components/layout';
import PrivateRoute from './PrivateRoute';

// Auth Pages
import LoginPage from '../features/auth/pages/LoginPage';

// Dashboard
import DashboardPage from '../features/dashboard/pages/DashboardPage';
import KycPage from '../features/compliance/pages/KycPage';
import KvkkPage from '../features/compliance/pages/KvkkPage';
import AccountsPage from '../features/accounts/pages/AccountsPage';
import NewAccountPage from '../features/accounts/pages/NewAccountPage';
import TransactionsPage from '../features/transactions/pages/TransactionsPage';
import TransferPage from '../features/transactions/pages/TransferPage';
import CardsPage from '../features/cards/pages/CardsPage';
import CardApplicationPage from '../features/cards/pages/CardApplicationPage';
import UserApplicationsPage from '../features/cards/pages/UserApplicationsPage';
import CardDetailPage from '../features/cards/pages/CardDetailPage';
import AdminFraudPage from '../features/admin/pages/AdminFraudPage';
import AdminAuditPage from '../features/admin/pages/AdminAuditPage';
import AdminSystemPage from '../features/admin/pages/AdminSystemPage';
import AccountDetailPage from '../features/accounts/pages/AccountDetailPage';
import RegisterPage from '../features/auth/pages/RegisterPage';
import AdminUsersPage from '../features/admin/pages/AdminUsersPage';
import ChangePasswordPage from '../features/auth/pages/ChangePasswordPage';

// Placeholder component for pages not yet created
const ComingSoon = ({ title }: { title: string }) => (
  <div style={{ padding: '40px', textAlign: 'center' }}>
    <h2>{title}</h2>
    <p>Bu sayfa henüz geliştirme aşamasında...</p>
  </div>
);

// Get user from localStorage
const getUser = () => {
  const userJson = localStorage.getItem('user');
  return userJson ? JSON.parse(userJson) : null;
};

const router = createBrowserRouter([
  // Public Routes
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/register',
    element: <RegisterPage />,
  },

  // Protected Routes
  {
    path: '/',
    element: (
      <PrivateRoute>
        <MainLayout user={getUser()} />
      </PrivateRoute>
    ),
    children: [
      {
        index: true,
        element: <Navigate to="/dashboard" replace />,
      },
      {
        path: 'dashboard',
        element: <DashboardPage />,
      },
      {
        path: 'accounts',
        element: <AccountsPage />,
      },
      {
        path: 'accounts/new',
        element: <NewAccountPage />,
      },
      {
        path: 'accounts/:id',
        element: <AccountDetailPage />,
      },
      {
        path: 'transactions',
        element: <TransactionsPage />,
      },
      {
        path: 'transfer',
        element: <TransferPage />,
      },
      {
        path: 'cards',
        element: <CardsPage />,
      },
      {
        path: 'cards/apply',
        element: <CardApplicationPage />,
      },
      {
        path: 'cards/applications',
        element: <UserApplicationsPage />,
      },
      {
        path: 'cards/:id',
        element: <CardDetailPage />,
      },
      {
        path: 'profile/security',
        element: <ChangePasswordPage />,
      },
      {
        path: 'compliance/kyc',
        element: <KycPage />,
      },
      {
        path: 'compliance/kvkk',
        element: <KvkkPage />,
      },
      // Admin Routes
      {
        path: 'admin/users',
        element: <AdminUsersPage />,
      },
      {
        path: 'admin/fraud',
        element: <AdminFraudPage />,
      },
      {
        path: 'admin/audit',
        element: <AdminAuditPage />,
      },
      {
        path: 'admin/system',
        element: <AdminSystemPage />,
      },
    ],
  },

  // 404 Page
  {
    path: '*',
    element: (
      <div style={{ padding: '40px', textAlign: 'center' }}>
        <h1>404</h1>
        <p>Sayfa Bulunamadı</p>
        <a href="/dashboard">Ana Sayfaya Dön</a>
      </div>
    ),
  },
]);

export default router;
