import { Orderstatus } from "./OrderStatus";

type NewOrder = {
    item: string;
    quantity: number;
    orderStatus?: Orderstatus;
    userId: string;
};
export default NewOrder;