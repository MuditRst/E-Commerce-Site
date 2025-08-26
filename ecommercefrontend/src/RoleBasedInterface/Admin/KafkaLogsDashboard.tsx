import { useEffect, useState } from "react";
import KafkaLogs from "../../Interface/KafkaLogs";
import { getKafkalogs } from "../../api";

function KafkaLogsDashboard() {
    const [logs, setLogs] = useState<KafkaLogs[]>([]);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        getKafkalogs().then(res => {
            setLogs(res.data);
            setLoading(false);
            console.log("Fetched Kafka Logs:", res.data);
        }).catch(error => {
            console.error("Error fetching Kafka logs:", error);
            setLoading(false);
        });
    }, []);

    if (loading) {
        return <p>Loading Kafka logs...</p>;
    }
    
    return (
        <div>
            <h2>Kafka Logs Dashboard</h2>
                <ul>
                    {logs.map((log , index) => (
                        <li key={log.LogID ?? index}>
                            <strong>Topic:</strong> {log.topic} <br />
                            <strong>Message:</strong> {log.message} <br />
                            <strong>Timestamp:</strong> {log.timestamp}
                        </li>
                    ))}
                </ul>
        </div>
    );
}

export default KafkaLogsDashboard;