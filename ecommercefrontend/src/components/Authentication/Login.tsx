import React from "react";
import { login } from "./LoginAPI";
import axios from "axios";
import { useNavigate } from "react-router-dom";

function Login() {
  const navigate = useNavigate();
  const HandleLogin = async (event:React.FormEvent<HTMLFormElement>) => {
  event.preventDefault();
  const username = event.currentTarget.username.value;
  const password = event.currentTarget.password.value;
  var loginData = {
    username: username,
    password: password,
  };
  var res = await login(loginData.username, loginData.password);
  if (res.status === 200) {
    console.log("Login successful");
    navigate("/userdashboard");
  } else {
    console.error("Login failed");
    //navigate to error page
  }
};

  return (
    <div>
      <h1>Login Page</h1>
      <form onSubmit={HandleLogin}>
        <div>
          <label htmlFor="username">Username:</label>
          <input type="text" id="username" name="username" required />
        </div>
        <div>
          <label htmlFor="password">Password:</label>
          <input type="password" id="password" name="password" required />
        </div>
        <button type="submit">Login</button>
      </form>
      <p>Don't have an account? Register here</p>
      <button onClick={() => navigate("/register",{state: { fromLogin: true }})}>Register</button>
      <p>Or</p>
      <p>Forgot your password? Reset it here</p>
      <button onClick={() => navigate("/forgot-password")}>Forgot Password</button>
    </div>
  );
}

export default Login;