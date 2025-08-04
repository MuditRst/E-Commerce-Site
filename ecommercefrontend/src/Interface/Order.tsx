import { Key } from "react";

type Order = {
    orderID: Key|number;
    item: string;
    quantity: number;
};
export default Order;
