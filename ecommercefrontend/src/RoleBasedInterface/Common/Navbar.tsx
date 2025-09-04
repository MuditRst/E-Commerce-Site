import React, { useEffect, useState } from "react";
import "../../components/styles/Navbar.css";
import { Link, useNavigate } from "react-router-dom";
import { getUserDetails, logout } from "../../components/Authentication/LoginAPI";
import User from "../../Interface/User";

 function Navbar() {
    const navigate = useNavigate();

    const [isOpen, setIsOpen] = useState(false);
    const [user,setUser] = useState<User>({} as User);
    const [loading, setLoading] = useState(true);

  useEffect(() => {
    getUserDetails()
      .then(res => {
        setUser(res.data);
        setLoading(false);
      })
      .catch(err => {
        console.error(err);
        setLoading(false);
      });
  }, []);
  const handleLogout = async () => {
      try {
        await logout();
        navigate("/login");
      } catch (error) {
        console.error("Logout failed:", error);
      }
    };

  return (
    <nav className="navbar">
      <div className="navbar-brand">E-Commerce</div>

      <div className={`navbar-links ${isOpen ? "open" : ""}`}>
        <Link to="/">Home</Link>
        {!loading && user.role === "User" &&<Link to="/userdashboard">User Dashboard</Link>}
        {!loading && user.role === "Admin" && <Link to="/admindashboard">Admin Dashboard</Link>}
        <Link to="/kafkalogs">Kafka Logs</Link>
        <Link to="/login">Login</Link>
      </div>

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
