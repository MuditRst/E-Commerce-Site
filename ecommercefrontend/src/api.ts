import axios from "axios";

const API = axios.create({
  baseURL: "http://localhost:5297", // or whatever your backend is
});


export const postOrder = (order: { Item: string; quantity: number }) => API.post("api/orders", order);

export const getOrders = () => API.get("api/orders");
