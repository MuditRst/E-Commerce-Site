import React from "react";
import Order from "../../Interface/Order";

type AddOrderProps = {
  orderToAdd?: Order;
  handleAddOrder: (newOrder: Order) => Promise<void>;
};

function AddOrder({ handleAddOrder, orderToAdd }: AddOrderProps) {
  const onSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const item = formData.get("orderName") as string;
    const quantity = Number(formData.get("quantity"));

    if (!item || isNaN(quantity)) {
      console.error("Invalid input");
      return;
    }

    const newOrder: Order = {
      orderID: orderToAdd?.orderID ?? 0,
      item,
      quantity,
    };

    await handleAddOrder(newOrder);
  };

  return (
    <>
      <h2>Add An Order</h2>
      <form onSubmit={onSubmit}>
        <input
          type="text"
          name="orderName"
          placeholder="Order Name"
          defaultValue={orderToAdd?.item}
        />
        <input
          type="number"
          name="quantity"
          placeholder="Quantity"
          defaultValue={orderToAdd?.quantity}
        />
        <button type="submit">Add Order</button>
      </form>
    </>
  );
}

export default AddOrder;
