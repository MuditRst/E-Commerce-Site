import React from "react";
import { useNavigate } from "react-router-dom";

type ErrorPageProps = {
  message: string;
};

const ErrorPage: React.FC<ErrorPageProps> = ({ message }) => {
const navigate = useNavigate();
  return (
    <div style={styles.container}>
      <div style={styles.card}>
        <h1 style={styles.title}>⚠ Oops!</h1>
        <p style={styles.message}>{message}</p>
        <button style={styles.button} onClick={() => navigate("/")}>
          ⬅ Go Back
        </button>
      </div>
    </div>
  );
};

const styles: { [key: string]: React.CSSProperties } = {
  container: {
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    height: "100vh",
    backgroundColor: "#f8f9fa",
  },
  card: {
    backgroundColor: "#fff",
    padding: "2rem",
    borderRadius: "12px",
    boxShadow: "0 6px 20px rgba(0,0,0,0.1)",
    textAlign: "center",
    maxWidth: "400px",
    width: "90%",
  },
  title: {
    fontSize: "2rem",
    color: "#dc3545",
    marginBottom: "1rem",
  },
  message: {
    fontSize: "1.2rem",
    marginBottom: "1.5rem",
    color: "#333",
  },
  button: {
    backgroundColor: "#007bff",
    color: "white",
    border: "none",
    padding: "0.6rem 1.2rem",
    borderRadius: "6px",
    cursor: "pointer",
    fontSize: "1rem",
  },
};

export default ErrorPage;
