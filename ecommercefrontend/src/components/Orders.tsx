import React, { useEffect, useState } from "react";

import { getOrders ,postOrder} from "../api";

function GetOrders(){
    const [orders, setOrders] = useState<Array<{ item: string; quantity: number }>>([]);
    const [viewMode, setViewMode] = useState<"none"|"showAllOrders"|"addAnOrder">("none");

    useEffect(() => {
        if(viewMode === "showAllOrders") {
            getOrders().then(res => setOrders(res.data)).catch(console.error);
        }
    }, [viewMode]);

    const handleSelectChange = (event:React.ChangeEvent<HTMLSelectElement>) => {
        console.log("Selected:", event.target.value);
        if (event.target.value === "showAllOrders") {
            setViewMode("showAllOrders");
        } else if (event.target.value === "addAnOrder") {
            setViewMode("addAnOrder");
        }else{
            setViewMode("none");
        }
    };

    const OnSubmitHandler= async (event:React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        const formData = new FormData(event.currentTarget);
        const orderName = formData.get("orderName") as string;
        const quantity = formData.get("quantity") as string;

        if (!orderName || !quantity) {
            console.error("Order name and quantity are required");
            return;
        }
        try {
            const res = await postOrder({ Item: orderName, quantity: Number(quantity) });
            console.log("Order submitted:", ( res).data);
            setViewMode("showAllOrders");
        } catch (error) {
            console.error("Error submitting order:", error);
        }
    };

    return (
    <div style={{ padding: "1rem" }}>
        <select onChange={handleSelectChange} defaultValue="">
            <option value="" disabled>
            Select an option
            </option>
            <option value="showAllOrders">Show all Orders</option>
            <option value="addAnOrder">Add an Order</option>
        </select>
        {viewMode === "showAllOrders" && (
        <div>
          <h3>Orders:</h3>
            <ul>
                {orders.map((order : any, index : any) => (
                <li key={index}>
                    <span>{order.item} - {order.quantity}</span>
                </li>
                ))}
            </ul>

        </div>
      )}

        {viewMode === "addAnOrder" ? (
            <form onSubmit={OnSubmitHandler}>
                <input type="string" name="orderName" placeholder="Order Name" />
                <input type="number" name="quantity" placeholder="Quantity" />
                <button type="submit">Add Order</button>
            </form>
        ) : null}
    </div>
    );
}

export default GetOrders;