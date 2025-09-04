import React, { useState, useEffect } from "react";
import { getAllOrders, updateOrderStatus } from "../../api";
import Order from "../../Interface/Order";
import RealTimeOrders from "./RealTimeOrders";
import UserListActions from "./UserListActions";
import KafkaLogsDashboard from "./KafkaLogsDashboard";
import "../../components/styles/Admindashboard.css";


function AdminDashBoard() {

    const [orders, setOrders] = useState<Order[]>([]);
    const [statusInputs, setStatusInputs] = useState<{ [key: string]: string }>({});


    const handleStatusChange = (orderId: string, value: string) => {
    setStatusInputs((prev) => ({
      ...prev,
      [orderId]: value,
    }));
  };

    useEffect(() => {
            getAllOrders().then(res => {
                setOrders(res.data);
                console.log("Fetched Orders:", res.data);
            }).catch(console.error);
    }, []);

    return (
    <div className="admin-dashboard">
      <h2>Admin Dashboard</h2>

      <div className="orders-panel">
        <ul>
          {orders.map((order) => (
            <li key={order.id}>
              <div className="order-meta">
                <span className="chip">ID: {order.id}</span>
                <span className="chip">Item: {order.item}</span>
                <span className="chip">Qty: {order.quantity}</span>
                <span className="chip status">{order.orderStatus}</span>
                <span className="chip">User: {order.userId?.toString()}</span>
              </div>

              <div className="order-actions">
                <select
                  value={statusInputs[order.id] ?? order.orderStatus}
                  onChange={(e) => handleStatusChange(order.id.toString(), e.target.value)}
                >
                  <option value={0}>Created</option>
                  <option value={1}>Pending</option>
                  <option value={2}>Processing</option>
                  <option value={3}>Completed</option>
                  <option value={4}>Cancelled</option>
                </select>

                <button
                  onClick={async () => {
                    console.log("Updating Order Status:", {
                      orderID: order.id,
                      newStatus: Number(statusInputs[order.id]),
                    });
                    await updateOrderStatus(
                      order.id.toString(),
                      Number(statusInputs[order.id])
                    );
                    window.location.reload();
                  }}
                >
                  Update Order Status
                </button>
              </div>
            </li>
          ))}
        </ul>
      </div>

      <div className="grid cols-2">
        <div className="section">
          <h3>User Actions</h3>
          <UserListActions />
        </div>
        <div className="section">
          <h3>Kafka Logs</h3>
          <KafkaLogsDashboard />
        </div>
      </div>

      <div className="grid cols-2" style={{ marginTop: "20px" }}>
        <div className="section" style={{ height:"450px" }}>
          <h3>Real-Time Orders</h3>
          <RealTimeOrders />
        </div>
        </div>
      </div>
    );
}

export default AdminDashBoard;