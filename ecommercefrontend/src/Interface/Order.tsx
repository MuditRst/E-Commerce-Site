import { Key } from "react";
import { Orderstatus } from "./OrderStatus";
import User from "./User";

type Order = {
    id: Key|string;
    item: string;
    quantity: number;
    orderStatus?: Orderstatus;
    userId : string | User;
};
export default Order;
