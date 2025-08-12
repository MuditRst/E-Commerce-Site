import { Key } from "react";
import { Orderstatus } from "./OrderStatus";
import User from "./User";

type Order = {
    orderID: Key|number;
    item: string;
    quantity: number;
    orderStatus?: Orderstatus;
    user : string | User;
};
export default Order;
