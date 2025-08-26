import React, { JSX, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getUserDetails } from "../Authentication/LoginAPI";

type ProtectedRouteProps = {
  children: JSX.Element;
  requiredRole?: string; // optional, for role-based routes
};

function ProtectedRoute({ children, requiredRole }: ProtectedRouteProps) {
  const navigate = useNavigate();
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    let isMounted = true;

    getUserDetails()
      .then((user) => {
        if (!isMounted) return;

        if (requiredRole && user.data.role !== requiredRole) {
          navigate("/unauthorized");
          return;
        }

        setIsChecking(false);
      })
      .catch(() => {
        if (isMounted) navigate("/login");
      });

    return () => {
      isMounted = false;
    };
  }, [navigate, requiredRole]);

  if (isChecking) return <p>Loading...</p>;

  return children;
}

export default ProtectedRoute;
