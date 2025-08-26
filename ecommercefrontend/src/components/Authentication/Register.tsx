
import { useNavigate,useLocation } from "react-router-dom";
import { register } from "./LoginAPI";
import { useEffect } from "react";
import "../styles/Login.css";

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
    <div className="login-container">
      <div className="login-box">
        <h1>Register</h1>
        <form onSubmit={handleRegister}>
          <div>
            <label htmlFor="username">Username</label>
            <input type="text" id="username" name="username" required />
          </div>

          <div>
            <label htmlFor="password">Password</label>
            <input type="password" id="password" name="password" required />
          </div>
          <div>
            <label htmlFor="confirmPassword">Confirm Password</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              required
            />
          </div>

          <button type="submit">Register</button>
        </form>

        <p>Already have an account?</p>
        <button
          className="link-btn"
          onClick={() => navigate("/login")}
        >
          Login
        </button>
      </div>
    </div>
  );
}

export default Register;
