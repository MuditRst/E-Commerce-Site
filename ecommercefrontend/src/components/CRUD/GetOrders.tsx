
import React, { useState, useEffect } from "react";
import {getOrders } from "../../api";
import Order from "../../Interface/Order";

type GetOrdersProps = {
    handleDeleteOrder: (order: Order) => void;
    handleUpdateOrder: (order: Order) => void;
};

function GetOrders({ handleDeleteOrder, handleUpdateOrder }: GetOrdersProps) {
    const [orders, setOrders] = useState<Order[]>([]);
    useEffect(() => {
        getOrders().then(res => {setOrders(res.data)}).catch(console.error);
    }, []);
    return (
        <div>
        <h3>Orders:</h3>
            <ul>
                {orders.map((order : Order, index) => (
                    <li key={order.orderID ?? index}>
                        {order.item} - {order.quantity}{" "}
                        <button onClick={() => handleDeleteOrder(order)}>Delete</button>
                        <button onClick={() => handleUpdateOrder(order)}>Edit</button>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default GetOrders;