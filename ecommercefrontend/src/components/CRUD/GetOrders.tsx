
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
        <table>
      <thead>
        <tr>
          <th>Item</th>
          <th>Quantity</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {orders.map(order => (
          <tr key={order.orderID}>
            <td>{order.item}</td>
            <td>{order.quantity}</td>
            <td>{order.orderStatus}</td>
            <td className="action-buttons">
              <button onClick={() => handleUpdateOrder(order)}>Edit</button>
              <button className="delete" onClick={() => handleDeleteOrder(order)}>Delete</button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
    );
}

export default GetOrders;