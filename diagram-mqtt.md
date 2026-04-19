# MQTT – Sekvenční diagram komunikace

Detailní pohled na MQTT komunikaci v `CommunicationManager` – Master (publish) i Slave (publish) režim.

```mermaid
sequenceDiagram
    participant CM as CommunicationManager
    participant Client as MQTTClient<br/>(mqttClient)
    participant Broker as MQTT Broker<br/>(IP:Port)
    participant PLC as PLC / Druhá strana
    participant Data as Sdílená data
    participant OM as OutputMapper<br/>(OnMessage callback)

    Note over CM,PLC: === MQTT MASTER / SLAVE REŽIM ===
    Note over CM: Oba režimy jsou téměř identické<br/>Oba publishují vstupy, oba přijímají výstupy přes OnMessage

    CM->>Client: Kontrola: client != null && IsConnected?
    
    alt Klient nepřipojen
        CM->>CM: await Task.Delay(200ms)
        CM->>CM: continue (další iterace)
    end

    Note over CM,Broker: === PUBLISH VSTUPŮ (v hlavní smyčce) ===

    rect rgb(200, 230, 255)
        CM->>Data: Čtení CrossroadData vstupů
        CM->>Client: Publish "JAN0837/Crossroad/Input"
        Client->>Broker: MQTT Publish (QoS=AtLeastOnce, Retain=true)
        
        CM->>Data: Čtení CrosswalkData vstupů
        CM->>Client: Publish "JAN0837/Crosswalk/Input"
        Client->>Broker: MQTT Publish (QoS=AtLeastOnce, Retain=true)
        
        CM->>Data: Čtení RegulatorData vstupů
        Note over CM: btnReset, switchstate, order,<br/>R1, R2, C1, C2, Uc1, Uc2, Td, Ts
        CM->>Client: Publish "JAN0837/Regulator/Input"
        Client->>Broker: MQTT Publish (QoS=AtLeastOnce, Retain=true)

        CM->>CM: PlantModel.ComputePlantStep()

        CM->>Data: Čtení CarLightData vstupů
        CM->>Client: Publish "JAN0837/CarLight/Input"
        Client->>Broker: MQTT Publish (QoS=AtLeastOnce, Retain=true)
    end

    CM->>CM: SetStatus("All data published successfully")

    Note over Broker,OM: === PŘÍJEM VÝSTUPŮ (asynchronně přes OnMessage) ===

    rect rgb(255, 230, 200)
        PLC->>Broker: Publish "JAN0837/Crossroad/Output"
        Broker->>Client: Deliver message
        Client->>OM: OnMessage callback
        OM->>Data: OutputMapper: Deserialize JSON → CrossroadData

        PLC->>Broker: Publish "JAN0837/Crosswalk/Output"
        Broker->>Client: Deliver message
        Client->>OM: OnMessage callback
        OM->>Data: OutputMapper: Deserialize JSON → CrosswalkData

        PLC->>Broker: Publish "JAN0837/Regulator/Output"
        Broker->>Client: Deliver message
        Client->>OM: OnMessage callback
        OM->>Data: OutputMapper: Deserialize JSON → RegulatorData.Uin

        PLC->>Broker: Publish "JAN0837/CarLight/Output"
        Broker->>Client: Deliver message
        Client->>OM: OnMessage callback
        OM->>Data: OutputMapper: Deserialize JSON → CarLightData
    end
```

## MQTT Topics

| Topic | Směr | Obsah |
|-------|------|-------|
| `JAN0837/Crossroad/Input` | App → PLC | btnStart, btnPause, btnStop, btnWestCrosswalk1/2, btnSouthCrosswalk1/2 |
| `JAN0837/Crossroad/Output` | PLC → App | crossroadType, trafficLights (N/S/W/E × R/Y/G), pedestrians |
| `JAN0837/Crosswalk/Input` | App → PLC | start, pause, stop, cw1, cw2 |
| `JAN0837/Crosswalk/Output` | PLC → App | crosswalkType, trafficLight1/2, pedestrian1/2 |
| `JAN0837/Regulator/Input` | App → PLC | btnReset, switchstate, order, R1, R2, C1, C2, Uc1, Uc2, Td, Ts |
| `JAN0837/Regulator/Output` | PLC → App | Uin |
| `JAN0837/CarLight/Input` | App → PLC | btnReset, error, sensorLight, sensorConnectorConnected |
| `JAN0837/CarLight/Output` | PLC → App | lowBeamLight, highBeamLight, turnLight, result |

## Specifika MQTT

- **QoS**: AtLeastOnce (1) – zprávy jsou doručeny minimálně jednou
- **Retain**: true – broker uchovává poslední zprávu pro nové subscribery
- **Výstupy**: Přijímány **asynchronně** přes `OnMessage` callback v `MQTTClient`, ne v hlavní smyčce
- **OutputMapper**: Třída v `comMQTT.cs` která deserializuje JSON a mapuje na statické datové třídy
- **Serializace**: `System.Text.Json.JsonSerializer` pro publish, `Newtonsoft.Json` (JsonConvert) pro receive
- **Error handling**: `OperationCanceledException` se propaguje (throw), ostatní exceptions logují a pokračují s 500ms delay
