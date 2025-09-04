import axios from "axios";
import { Orderstatus } from "./Interface/OrderStatus";

const API = axios.create({
  baseURL: process.env.REACT_APP_API_URL,
  withCredentials: true,
});

export const postOrder = (data: any) =>
  API.post("api/orders", data);

export const getOrders = () =>
  API.get("api/orders");

export const getAllOrders = () =>
  API.get("api/orders/all");

export const deleteOrder = (id: string) =>
  API.delete(`api/orders/${id}`);

export const updateOrder = (id: string, data: any) =>
  API.put(`api/orders/${id}`, data);

export const updateOrderStatus = (id: string, orderStatus: Orderstatus) =>
  API.put(`api/orders/${id}/status`, {newStatus : Number(orderStatus)}, {
    headers: { "Content-Type": "application/json" },
  });

export const getUsers = () =>
  API.get("api/users");

export const deleteUser = (id: string) =>
  API.delete(`api/users/${id}`);

export const getKafkalogs = () =>
  API.get("api/kafka/logs");

export const getStats = () =>
  API.get("api/orders/stats");
