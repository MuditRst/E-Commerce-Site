import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import UserDashBoard from "./RoleBasedInterface/User/UserDashBoard";
import Login from "./components/Authentication/Login";
import ProtectedRoute from "./components/Routing/ProtectedRoute";
import Register from "./components/Authentication/Register";
import AdminDashBoard from "./RoleBasedInterface/Admin/AdminDashBoard";
import ErrorPage from "./components/ErrorPage/Error";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
        
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        <Route
          path="/userdashboard"
          element={
            <ProtectedRoute>
              <UserDashBoard />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admindashboard"
          element={
            <ProtectedRoute requiredRole="Admin">
              <AdminDashBoard />
            </ProtectedRoute>
          }
        />
        <Route path="/unauthorized" element={<ErrorPage message="You do not have permission to access this page." />} />
        <Route path="*" element={<ErrorPage message="404 - Page Not Found" />} />
      </Routes>
    </Router>
  );
}

export default App;
