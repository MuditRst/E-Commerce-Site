import  { JSX } from "react";
import { useLocation, Navigate } from "react-router-dom";
import { useAuth } from "../Authentication/AuthContext";

type ProtectedRouteProps = {
  children: JSX.Element;
  requiredRole?: string;
};

function ProtectedRoute({ children, requiredRole }: ProtectedRouteProps) {
  const { user, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return <p className="text-center mt-8">Loading...</p>;
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (requiredRole && user.role !== requiredRole) {
    return <Navigate to="/unauthorized" replace />;
  }

  return children;
}

export default ProtectedRoute;
