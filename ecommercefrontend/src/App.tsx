import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import RealTimeOrders from "./components/RealTimeOrders";
import UserDashBoard from "./components/UserDashBoard";
import Login from "./components/Authentication/Login";
import ProtectedRoute from "./components/Routing/ProtectedRoute";
import Register from "./components/Authentication/Register";

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
          path="/realtimeorders"
          element={
            <ProtectedRoute>
              <RealTimeOrders />
            </ProtectedRoute>
          }
        />
      </Routes>
    </Router>
  );
}

export default App;
