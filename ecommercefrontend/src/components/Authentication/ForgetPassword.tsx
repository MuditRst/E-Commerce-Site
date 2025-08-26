import React from "react";
import { resetPassword } from "./LoginAPI";
import { useNavigate } from "react-router-dom";
import "../styles/Login.css";

function ForgetPassword() {
    const navigate = useNavigate();
    const handleResetPassword = async (event:React.FormEvent<HTMLFormElement>) => {
  event.preventDefault();
  const newPassword = event.currentTarget["new-password"].value;
  const confirmNewPassword = event.currentTarget["confirm-new-password"].value;
  const userName = event.currentTarget.username.value;

  if (newPassword !== confirmNewPassword) {
    console.error("Passwords do not match");
    return;
  }

  try {
    await resetPassword(userName, newPassword);
    console.log("Password reset successful");
    navigate("/login");
  } catch (error) {
    console.error("Error resetting password:", error);
    navigate("/error");
  }
};

  return (
    <div className="login-container">
      <div className="login-box">
        <h1>Reset Password</h1>
        <form onSubmit={handleResetPassword}>
          <div>
            <label htmlFor="username">Username</label>
            <input type="text" id="username" name="username" required />
          </div>

          <div>
            <label htmlFor="password">New Password</label>
            <input type="password" id="password" name="password" required />
          </div>

          <button type="submit">Reset Password</button>
        </form>

        <p>Remembered your password?</p>
        <button
          className="link-btn"
          onClick={() => navigate("/login")}
        >
          Back to Login
        </button>
      </div>
    </div>
  );
}

export default ForgetPassword;