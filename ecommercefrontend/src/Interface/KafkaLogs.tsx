import { Key } from "react";

type KafkaLogs = {
  LogID: Key | number;
  topic: string;
  message: string;
  timestamp: string;
}

export default KafkaLogs;