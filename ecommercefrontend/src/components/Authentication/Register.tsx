import axios from "axios";
import { useNavigate,useLocation } from "react-router-dom";
import { register } from "./LoginAPI";
import { use, useEffect } from "react";

function Register() {
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        if (!location.state?.fromLogin) {
            navigate("/login");
        }
    }, [location,navigate]);

    const handleRegister = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const username = event.currentTarget.username.value;
    const password = event.currentTarget.password.value;
    const confirmPassword = event.currentTarget.confirmPassword.value;
    if (password !== confirmPassword) {
        console.error("Passwords do not match");
        return;
    }
    try {
      const response = await register(username, password);
      console.log("Registration successful:", response.data);
      navigate("/userdashboard");
    } catch (error) {
      console.error("Error during registration:", error);
      alert("Registration failed. Please try again.");
    }
  };

  return (
    <form onSubmit={handleRegister}>
      <input type="text" name="username" placeholder="Username" required />
      <input type="password" name="password" placeholder="Password" required />
      <input type="password" name="confirmPassword" placeholder="Confirm Password" required />
      <button type="submit">Register</button>
    </form>
  );
}

export default Register;
