import axios from "axios";

const API = axios.create({
  baseURL: process.env.REACT_APP_API_URL,
  withCredentials: true,
});

export const login = (username: string, password: string) => {
  return API.post("api/auth/login", { username, password }, {withCredentials:true});
};

export const register = (username: string, password: string) => {
  return API.post("api/auth/register", { username, password });
};

export const getUserDetails = () => {
  return API.get("/api/user/me" , {withCredentials : true});
};

export const logout = () => {
  return API.post("/api/auth/logout", {}, { withCredentials: true });
}

export const resetPassword = (username: string, newPassword: string) => {
  return API.put(`/api/auth/forgetpassword`, { username, newPassword });
};
