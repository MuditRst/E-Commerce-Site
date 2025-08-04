import axios from "axios";

const API = axios.create({
  baseURL: "http://localhost:5097",
});

export const login = (username: string, password: string) => {
  return API.post("api/auth/login", { username, password },{withCredentials: true});
};
