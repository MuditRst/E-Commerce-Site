import axios from "axios";

const API = axios.create({
  baseURL: "http://localhost:5097",
});


export const postOrder = (data : any) => API.post("api/orders", data,{withCredentials: true});

export const getOrders = () => API.get("api/orders");

export const deleteOrder = async (order: { item: string; quantity: number }) => {
  return await fetch("http://localhost:5097/api/orders", {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(order),
    credentials: "include",
  });
};

export const updateOrder = (id: number, data: any) => API.put(`api/orders/${id}`, data,{withCredentials: true});
