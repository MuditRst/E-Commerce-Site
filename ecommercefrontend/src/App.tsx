import React, { useEffect, useState } from "react";
import GetOrders from "./components/Orders";
import RealTimeOrders from "./components/RealTimeOrders";

function App() {

  return (
    <>
      <div>
        <h1>Please Select An Option</h1>
        <GetOrders />
        <RealTimeOrders />
      </div>
    </>
  );
}

export default App;
