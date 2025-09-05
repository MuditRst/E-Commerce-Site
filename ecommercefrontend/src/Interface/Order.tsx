import { Orderstatus } from "./OrderStatus";

type Order = {
    id: string;
    Item: string;
    Quantity: number;
    orderStatus?: Orderstatus;
    userId : string;
};
export default Order;
