import axios from "axios";

const API = axios.create({
  baseURL: "http://localhost:5297", // or whatever your backend is
});


export const postOrder = (data : any) => API.post("api/orders", data);

export const getOrders = () => API.get("api/orders");

export const deleteOrder = async (order: { item: string; quantity: number }) => {
  return await fetch("http://localhost:5297/api/orders", {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(order),
  });
};

export const updateOrder = (id: number, data: any) => API.put(`api/orders/${id}`, data);
