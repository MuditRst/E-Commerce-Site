import { Orderstatus } from "./OrderStatus";

type NewOrder = {
    Item: string;
    Quantity: number;
    orderStatus?: Orderstatus;
    userId: string;
};
export default NewOrder;