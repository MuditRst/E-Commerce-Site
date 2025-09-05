import React from "react";
import {  login } from "./LoginAPI";
import "../styles/Login.css";
import { useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

function Login() {
  const navigate = useNavigate();
  const {setUser} = useAuth();

  const HandleLogin = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const username = event.currentTarget.username.value;
    const password = event.currentTarget.password.value;

    try {
      const res = await login(username, password);
      if (res.status === 200) {
        setUser(res.data);
        const { role } = res.data;
        if (role === "User") {
          console.log("Login successful as User");
          navigate("/userdashboard");
        } else if (role === "Admin") {
          console.log("Login successful as Admin");
          navigate("/admindashboard");
        } else {
          console.error("Unknown role:", role);
          navigate("/unauthorized");
        }
      } else {
        console.error("Login failed:", res.status);
      }
    } catch (error) {
      console.error("Error occurred during login:", error);
      navigate("/error");
    }
  };

  return (
    <div className="login-container">
      <div className="login-box">
        <h1>Login</h1>
        <form onSubmit={HandleLogin}>
          <div>
            <label htmlFor="username">Username</label>
            <input type="text" id="username" name="username" required />
          </div>
          <div>
            <label htmlFor="password">Password</label>
            <input type="password" id="password" name="password" required />
          </div>
          <button type="submit">Login</button>
        </form>

        <p>Don't have an account?</p>
        <button
          className="link-btn"
          onClick={() => navigate("/register", { state: { fromLogin: true } })}
        >
          Register
        </button>

        <p>Forgot your password?</p>
        <button
          className="link-btn"
          onClick={() => navigate("/forgetpassword")}
        >
          Reset Password
        </button>
      </div>
    </div>
  );
}

export default Login;
