import { useState } from "react";
import "../../components/styles/Navbar.css";
import { Link, useNavigate } from "react-router-dom";
import { logout } from "../../components/Authentication/LoginAPI";
import { useAuth } from "../../components/Authentication/AuthContext";

 function Navbar() {
    const navigate = useNavigate();
    const { user, setUser } = useAuth();

    const [isOpen, setIsOpen] = useState(false);

  const handleLogout = async () => {
      try {
        await logout();
        setUser(null);
        navigate("/login");
      } catch (error) {
        console.error("Logout failed:", error);
      }
    };

  return (
    <nav className="navbar">
      <div className="navbar-brand">E-Commerce</div>

      {user ? (
      <div className={`navbar-links ${isOpen ? "open" : ""}`}>
        <Link to={`/${user.role}dashboard`}>Home</Link>
        {user?.role === "User" && <Link to="/userdashboard">User Dashboard</Link>}
        {user?.role === "Admin" && <Link to="/admindashboard">Admin Dashboard</Link>}
      </div>
      ) : (
      <div className={`navbar-links ${isOpen ? "open" : ""}`}>
        <Link to="/login">Login</Link>
      </div>
      )}

      <div 
        className={`hamburger ${isOpen ? "active" : ""}`} 
        onClick={() => setIsOpen(!isOpen)}
      >
        <span></span>
        <span></span>
        <span></span>
      </div>
      <div className="navbar-actions">
        <button onClick={handleLogout} className="logout-btn">
          Logout
        </button>
        </div>
    </nav>
  );
}

export default Navbar;
