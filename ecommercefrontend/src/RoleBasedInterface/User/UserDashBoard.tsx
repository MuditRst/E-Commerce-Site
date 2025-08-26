import React, { useEffect, useState } from "react";

import { deleteOrder, getOrders ,postOrder, updateOrder} from "../../api";
import Order from "../../Interface/Order";
import GetOrders from "../../components/CRUD/GetOrders";
import UpdateOrder from "../../components/CRUD/UpdateOrders";
import AddOrder from "../../components/CRUD/AddOrder";
import "../../components/styles/userDashboard.css";

function UserDashBoard(){
    const [orders, setOrders] = useState<Order[]>([]);
    const [editOrder, setEditOrder] = useState<Order>();
    const [viewMode, setViewMode] = useState<"showAllOrders"|"addAnOrder"|"deleteAnOrder"|"editOrder">("showAllOrders");

    useEffect(() => {
        if(viewMode === "showAllOrders") {
            getOrders().then(res => {setOrders(res.data)}).catch(console.error);
        }
    }, [viewMode]);

    const handleDeleteOrder = async (order:Order) => {

        if (!order.item || !order.quantity) {
            console.error(`Error Deleting Order: Order ${order.item} and ${order.quantity} are required`);
            return;
        }

        try {
            const res = await deleteOrder({item: order.item, quantity: Number(order.quantity)});
            setViewMode("showAllOrders");
            window.location.reload();
        } catch (error) {
            console.error("Error deleting order:", error);
        }
    };

    const OnAddHandler= async (order:Order) => {
        try {
            const orderExists = orders.some(o => o.item === order.item);
            if (orderExists) {
                console.error(`Order with item ${order.item} already exists.`);
                setViewMode("showAllOrders");
                return;
            }
            const res = await postOrder({ Item: order.item, quantity: order.quantity });
            console.log("Order submitted:", res.data);
            setViewMode("showAllOrders");
        } catch (err) {
            console.error("Error:", err);
        }
    };

    const handleEditClick = (order: Order) => {
        setEditOrder(order);
        setViewMode("editOrder");
    };

    const handleUpdateSubmit = async (order: Order) => {
        try {
            if (order.orderID !== undefined) {
                const res = await updateOrder(Number(order.orderID), {
                    item: order.item,
                    quantity: order.quantity,
                });
                console.log("Updated:", res.data);
                setEditOrder(res.data);
                setViewMode("showAllOrders");
                window.location.reload();
            }
        } catch (err) {
        console.error("Update failed", err);
        }
    };

    return (
    <div className="user-dashboard">
        <h2>Dashboard</h2>
        <button onClick={() => setViewMode("addAnOrder")}>Add Order</button>
        {viewMode === "showAllOrders" && (
        <div className="orders-list">
            <GetOrders handleDeleteOrder={handleDeleteOrder} handleUpdateOrder={handleEditClick} />
        </div>
      )}

        {viewMode === "addAnOrder" && (
            <div className="form-container">
                <AddOrder handleAddOrder={OnAddHandler}/>
            </div>
        )}

        {viewMode === "editOrder" && (
            <div className="form-container">
                <UpdateOrder orderToEdit={editOrder} handleUpdateOrder={handleUpdateSubmit} />
            </div>
        )}
        </div>
    );
}

export default UserDashBoard;