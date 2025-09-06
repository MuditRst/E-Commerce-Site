import { Orderstatus } from "./OrderStatus";

type Order = {
    id: string;
    item: string;
    quantity: number;
    orderStatus?: Orderstatus;
    userId : string;
};
export default Order;
