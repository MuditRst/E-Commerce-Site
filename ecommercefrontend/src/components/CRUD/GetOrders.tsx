
import React, { useState, useEffect } from "react";
import {getOrders } from "../../api";
import Order from "../../Interface/Order";

type GetOrdersProps = {
    handleDeleteOrder: (orderID: string) => void;
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
          <tr key={order.id}>
            <td>{order.Item}</td>
            <td>{order.Quantity}</td>
            <td>{order.orderStatus}</td>
            <td className="action-buttons">
              <button onClick={() => handleUpdateOrder(order)}>Edit</button>
              <button className="delete" onClick={() => handleDeleteOrder(order.id.toString())}>Delete</button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
    );
}

export default GetOrders;