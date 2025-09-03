import { useState,useEffect } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";
import Order from "../../Interface/Order";
import AdminPieChart from "./AdminPieChart";

function RealTimeOrders() {
    const [orders, setOrders] = useState<Order[]>([]);
    const [refresh, setRefresh] = useState(0);

    useEffect(() => {
        const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5097/hubs/orders",{withCredentials: true})
        .withAutomaticReconnect()
        .build();
        connection.start().then(()=> console.log("Connected to SignalR hub")).catch((err) => console.error("SignalR connection failed:", err));
        connection.on("ReceiveOrder", (orderJson) => {
            const order = JSON.parse(orderJson);
            setOrders(prevOrders => [order, ...prevOrders]);
            setRefresh((v) => v + 1);
        });

        return () => {
            connection.stop().then(() => console.log("Disconnected from SignalR hub"));
        };
    }, []);


    return (
        <div>
            <h2>Real-Time Orders</h2>
            <ul>
                {orders.map((order) => {
                    console.log("Order:", order);
                    return <li key={order.id}>{order.item} - {order.quantity}</li>
                })}
            </ul>

            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 20 }}>
                <AdminPieChart refresh={refresh} />
            </div>
        </div>
    );
}

export default RealTimeOrders;