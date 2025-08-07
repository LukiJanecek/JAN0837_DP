import { HubConnectionBuilder } from "@microsoft/signalr";
import React, { useEffect, useState } from "react";

export function useSignalR() {
  const [connection, setConnection] = useState(null);
  const [state, setState] = useState(null);

  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl("http://localhost:5000/signalr")
      .withAutomaticReconnect()
      .build();

    conn.start()
      .then(() => console.log("SignalR connected"))
      .catch(console.error);

    // přijmi zprávy od serveru
    conn.on("ReceiveState", latest => {
      setState(latest);
    });

    setConnection(conn);
    return () => conn.stop();
  }, []);

  // funkce pro odeslání příkazu na server
  const sendCommand = (cmd, payload) => {
    if (!connection) return;
    connection.invoke("SendCommand", cmd, payload)
      .catch(console.error);
  };

  return { state, sendCommand };
}
