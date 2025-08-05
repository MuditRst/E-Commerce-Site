import axios from "axios";

const API = axios.create({
  baseURL: "http://localhost:5097",
});

export const login = (username: string, password: string) => {
  return API.post("api/auth/login", { username, password },{withCredentials: true});
};

export const register = (username: string, password: string) => {
  return API.post("api/auth/register", { username, password });
};

export const getUserDetails = () => {
  return API.get("api/user/me", { withCredentials: true });
};