import React, { useEffect, useState } from "react";

import { deleteOrder, getOrders ,postOrder, updateOrder} from "../../api";
import Order from "../../Interface/Order";
import GetOrders from "../../components/CRUD/GetOrders";
import UpdateOrder from "../../components/CRUD/UpdateOrders";
import AddOrder from "../../components/CRUD/AddOrder";
import "../../components/styles/userDashboard.css";
import NewOrder from "../../Interface/NewOrder";

function UserDashBoard(){
    const [orders, setOrders] = useState<Order[]>([]);
    const [editOrder, setEditOrder] = useState<Order>();
    const [viewMode, setViewMode] = useState<"showAllOrders"|"addAnOrder"|"deleteAnOrder"|"editOrder">("showAllOrders");

    useEffect(() => {
        if(viewMode === "showAllOrders") {
            getOrders().then(res => {setOrders(res.data)}).catch(console.error);
        }
    }, [viewMode]);

    const handleDeleteOrder = async (orderID:string) => {

        if (!orderID) {
            console.error(`Error Deleting Order: Order ID ${orderID} is required`);
            return;
        }

        try {
            await deleteOrder(orderID.toString());
            setOrders(prevOrders => prevOrders.filter(o => o.id !== orderID));
            setViewMode("showAllOrders");
        } catch (error) {
            console.error("Error deleting order:", error);
        }
    };

    const OnAddHandler= async (order:NewOrder) => {
        try {
            const orderExists = orders.some(o => o.Item === order.Item);
            if (orderExists) {
                console.error(`Order with item ${order.Item} already exists.`);
                setViewMode("showAllOrders");
                return;
            }
            const res = await postOrder({ Item: order.Item, Quantity: order.Quantity });
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
            if (order.id !== undefined) {
                const res = await updateOrder(order.id.toString(), {
                    Item: order.Item,
                    Quantity: order.Quantity,
                });
                console.log("Updated:", res.data);
                setEditOrder(res.data);
                setViewMode("showAllOrders");
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