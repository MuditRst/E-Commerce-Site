import React, { useState, useEffect } from "react";
import Order from "../../Interface/Order";

type UpdateOrdersProps = {
  orderToEdit?: Order;
  handleUpdateOrder: (updatedOrder: Order) => Promise<void>;
};

function UpdateOrder({ orderToEdit, handleUpdateOrder }: UpdateOrdersProps) {
  const [order, setOrder] = useState<Order | undefined>(orderToEdit);

  useEffect(() => {
    setOrder(orderToEdit);
  }, [orderToEdit]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setOrder((prev) =>
      prev
        ? { ...prev, [name]: name === "quantity" ? Number(value) : value }
        : prev
    );
  };

  const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (order) {
      handleUpdateOrder(order);
    }
  };

  if (!order) return null;

  return (
    <div>
      <h2>Edit An Order</h2>
      <form onSubmit={onSubmit}>
        <input
          type="text"
          name="item"
          value={order.item}
          onChange={handleChange}
          placeholder="Order Name"
        />
        <input
          type="number"
          name="quantity"
          value={order.quantity}
          onChange={handleChange}
          placeholder="Quantity"
        />
        <button type="submit">Edit Order</button>
      </form>
    </div>
  );
}

export default UpdateOrder;
