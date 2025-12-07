import { Navigate, useLocation } from 'react-router-dom';
import { isAdminUser } from '../utils/auth';

interface PrivateRouteProps {
  children: React.ReactNode;
}

const PrivateRoute = ({ children }: PrivateRouteProps) => {
  const location = useLocation();
  const token = localStorage.getItem('authToken');
  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  const goingAdmin = location.pathname.startsWith('/admin');
  if (goingAdmin && !isAdminUser(user)) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
};

export default PrivateRoute;
