import React, { useState, useEffect, use } from "react";
import { deleteOrder, getOrders, postOrder, updateOrder, updateOrderStatus } from "../../api";
import Order from "../../Interface/Order";
import { Orderstatus } from "../../Interface/OrderStatus";
import RealTimeOrders from "./RealTimeOrders";


function AdminDashBoard() {

    const [orders, setOrders] = useState<Order[]>([]);
    const [statusInputs, setStatusInputs] = useState<{ [key: number]: string }>({});


    const handleStatusChange = (orderId: number, value: string) => {
    setStatusInputs((prev) => ({
      ...prev,
      [orderId]: value,
    }));
  };

    useEffect(() => {
            getOrders().then(res => {
                setOrders(res.data);
                console.log("Fetched Orders:", res.data);
            }).catch(console.error);
    }, []);

    return (
    <div>
      <h2>Admin Dashboard</h2>
      <ul>
        {orders.map((order) => (
          <li key={order.orderID}>
            Order ID: {order.orderID}, Item: {order.item}, Quantity: {order.quantity}, Status: {order.orderStatus}, User: {order.user.toString()}
            
            <select
                value={statusInputs[Number(order.orderID)] ?? order.orderStatus}
                onChange={(e) => handleStatusChange(Number(order.orderID), e.target.value)}
            >
               <option value={0}>Created</option>
               <option value={1}>Pending</option>
               <option value={2}>Processing</option>
               <option value={3}>Completed</option>
               <option value={4}>Cancelled</option>
            </select>

            <button
              onClick={() => {
                console.log("Updating Order Status:", {
                  orderID: Number(order.orderID),
                  newStatus: Number(statusInputs[Number(order.orderID)]),
                });
                updateOrderStatus(Number(order.orderID), Number(statusInputs[Number(order.orderID)]));
                window.location.reload();
              }}
            >
              Update Order Status
            </button>
          </li>
        ))}
      </ul>
      <RealTimeOrders />
    </div>
    );
}

export default AdminDashBoard;