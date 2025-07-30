import { useState,useEffect, use } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";
import Order from "../Interface/Order";

function RealTimeOrders() {
    const [orders, setOrders] = useState<Order[]>([]);
    useEffect(() => {
        const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5097/hubs/orders",{withCredentials: true})
        .withAutomaticReconnect()
        .build();
        connection.start().then(()=> console.log("Connected to SignalR hub")).catch((err) => console.error("SignalR connection failed:", err));
        connection.on("ReceiveOrder", (orderJSON) => {
            const order = JSON.parse(orderJSON);
            setOrders(prevOrders => [order, ...prevOrders]);
        });

        return () => {
            connection.stop().then(() => console.log("Disconnected from SignalR hub"));
        };
    }, []);


    return (
        <div>
            <h2>Real-Time Orders</h2>
            <ul>
                {orders.map((order, index) => (
                    <li key = {index}>{order.item  } - {order.quantity}</li>
                ))}
            </ul>
        </div>
    );
}

export default RealTimeOrders;