import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import UserDashBoard from "./RoleBasedInterface/User/UserDashBoard";
import Login from "./components/Authentication/Login";
import ProtectedRoute from "./components/Routing/ProtectedRoute";
import Register from "./components/Authentication/Register";
import AdminDashBoard from "./RoleBasedInterface/Admin/AdminDashBoard";
import ErrorPage from "./components/ErrorPage/Error";
import Navbar from "./RoleBasedInterface/Common/Navbar";
import ForgetPassword from "./components/Authentication/ForgetPassword";

function App() {
  return (
  
    <Router>
      <Navbar />
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />

        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/forgetpassword" element={<ForgetPassword />} />

        <Route
          path="/userdashboard"
          element={<ProtectedRoute requiredRole="User">
            <UserDashBoard />
          </ProtectedRoute>} />
        <Route
          path="/admindashboard"
          element={<ProtectedRoute requiredRole="Admin">
            <AdminDashBoard />
          </ProtectedRoute>} />
        <Route path="/unauthorized" element={<ErrorPage message="You do not have permission to access this page." />} />
        <Route path ="/error" element={<ErrorPage message="An error occurred. Please try again later." />} />
        <Route path="*" element={<ErrorPage message="404 - Page Not Found" />} />
      </Routes>
    </Router>
  );
}

export default App;
