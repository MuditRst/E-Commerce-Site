import { Key } from "react";

type KafkaLogs = {
  id: Key | number;
  topic: string;
  message: string;
  timestamp: string;
}

export default KafkaLogs;