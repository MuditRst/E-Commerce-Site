import React, { useEffect, useState } from "react";
import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { getStats } from "../../api";

function AdminPieChart({refresh} : {refresh: number}) {

    const COLORS = ["#8884d8", "#82ca9d", "#ffc658", "#ff7f7f", "#8dd1e1"];

        const [data, setData] = useState([]);

        useEffect(() => {
            getStats().then(res=>setData(res.data))
            .catch(err => console.log("Error fetching stats:", err));
        }, [refresh]);

    return (
    <div style={{ width: "100%", height: 300 }}>
      <h3>Orders by Status</h3>
      <ResponsiveContainer>
        <PieChart>
          <Pie
            data={data}
            dataKey="count"
            nameKey="status"
            cx="50%"
            cy="50%"
            outerRadius={100}
            label
          >
            {data.map((_, index) => (
              <Cell key={index} fill={COLORS[index % COLORS.length]} />
            ))}
          </Pie>
          <Tooltip />
          <Legend />
        </PieChart>
      </ResponsiveContainer>
    </div>
    );
}

export default AdminPieChart;
