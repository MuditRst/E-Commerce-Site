import React, { JSX, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getUserDetails } from "../Authentication/LoginAPI";

function ProtectedRoute({ children }: { children: JSX.Element }) {
  const navigate = useNavigate();
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    let isMounted = true;

    getUserDetails()
      .then(() => {
        if (isMounted) setIsChecking(false);
      })
      .catch(() => {
        if (isMounted) navigate("/login");
      });

    return () => {
      isMounted = false;
    };
  }, [navigate]);

  if (isChecking) return <p>Loading...</p>;

  return children;
}

export default ProtectedRoute;
