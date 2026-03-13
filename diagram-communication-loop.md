# Stavový diagram komunikační smyčky

Stavový/vývojový diagram hlavní metody `CommunicationManager.Communication()` – řídící while smyčka s výběrem protokolu.

```mermaid
flowchart TD
    Start([Start Communication]) --> CheckFlags{communicationThreadRunningFlag<br/>&& !token.IsCancellationRequested}
    
    CheckFlags -->|false| End([Konec smyčky])
    CheckFlags -->|true| Switch{switch<br/>communicationFlag}

    Switch -->|"MQTT"| MQTT_GetClient[client = _ucCommunicationControl._mqttClient]
    Switch -->|"OPCUA"| OPCUA_CheckMode{checkBoxMaster?}
    Switch -->|"ModbusTCPIP"| Modbus_CheckMode{checkBoxMaster?}
    Switch -->|"RESTAPI"| REST_Status[SetStatus: Already running]
    Switch -->|"Sharp7"| Sharp7_CheckConn{_sharp7.client.Connected?}
    Switch -->|default| LogUnknown[Logger.LogError: Unknown type]

    %% ════════════════ MQTT ════════════════
    MQTT_GetClient --> MQTT_ParsePort{Port validní?}
    MQTT_ParsePort -->|ne| MQTT_InvalidPort[SetStatus: Port not valid] --> End
    MQTT_ParsePort -->|ano| MQTT_CheckMode{checkBoxMaster?}
    
    MQTT_CheckMode -->|Master| MQTT_M_Connected{client != null<br/>&& IsConnected?}
    MQTT_M_Connected -->|ne| MQTT_M_Wait[await 200ms] --> Delay50
    MQTT_M_Connected -->|ano| MQTT_M_Publish["Publish Input topics:<br/>Crossroad/Input<br/>Crosswalk/Input<br/>Regulator/Input"]
    MQTT_M_Publish --> MQTT_M_Plant[PlantModel.ComputePlantStep]
    MQTT_M_Plant --> MQTT_M_CarLight[Publish CarLight/Input]
    MQTT_M_CarLight --> MQTT_M_Status[SetStatus: All published] --> Delay50

    MQTT_CheckMode -->|Slave| MQTT_S_Connected{client != null<br/>&& IsConnected?}
    MQTT_S_Connected -->|ne| MQTT_S_Wait[await 200ms] --> Delay50
    MQTT_S_Connected -->|ano| MQTT_S_Publish["Publish Input topics:<br/>Crossroad/Input<br/>Crosswalk/Input<br/>Regulator/Input"]
    MQTT_S_Publish --> MQTT_S_Plant[PlantModel.ComputePlantStep]
    MQTT_S_Plant --> MQTT_S_CarLight[Publish CarLight/Input]
    MQTT_S_CarLight --> MQTT_S_Status[SetStatus: All published] --> Delay50
    
    MQTT_CheckMode -->|else| MQTT_NoSelect[SetStatus: Choose device] --> Delay50

    %% ════════════════ OPC UA ════════════════
    OPCUA_CheckMode -->|Master| OPCUA_M_NotImpl[SetStatus: Not implemented<br/>break] --> Delay50
    
    OPCUA_CheckMode -->|Slave| OPCUA_S_Connected{opcuaClient != null<br/>&& connected?}
    OPCUA_S_Connected -->|ne| OPCUA_S_Wait[await 1000ms] --> Delay50
    OPCUA_S_Connected -->|ano| OPCUA_BulkRead{BulkReadAllOutputs()}
    OPCUA_BulkRead -->|failed| OPCUA_ReadFail[await 1000ms] --> Delay50
    OPCUA_BulkRead -->|ok| OPCUA_Plant[PlantModel.ComputePlantStep]
    OPCUA_Plant --> OPCUA_BulkWrite[BulkWriteAllInputs]
    OPCUA_BulkWrite --> OPCUA_CheckSession{connected?}
    OPCUA_CheckSession -->|ne| OPCUA_SessionLost[await 1000ms] --> Delay50
    OPCUA_CheckSession -->|ano| OPCUA_Status[SetStatus: Synchronized] --> Delay50
    
    OPCUA_CheckMode -->|else| OPCUA_NoSelect[SetStatus: Choose device] --> Delay50

    %% ════════════════ Modbus ════════════════
    Modbus_CheckMode -->|Master| Modbus_M_Check{_modbusServer.slave != null?}
    Modbus_M_Check -->|ne| Modbus_M_Wait[await 500ms] --> Delay50
    Modbus_M_Check -->|ano| Modbus_M_Write["SetRegisters (vstupy):<br/>reg 1-7, 8-12, 13-31, 32-35"]
    Modbus_M_Write --> Modbus_M_Read["GetRegisters (výstupy):<br/>reg 40-60, 61-71, 72-73"]
    Modbus_M_Read --> Modbus_M_Plant[PlantModel.ComputePlantStep]
    Modbus_M_Plant --> Modbus_M_CarLight[GetRegisters: reg 74-77] --> Modbus_M_Status[SetStatus: Synchronized] --> Delay50
    
    Modbus_CheckMode -->|Slave| Modbus_S_Check{_modbusClient.IsConnected?}
    Modbus_S_Check -->|ne| Modbus_S_Connect{ConnectToSlave()}
    Modbus_S_Connect -->|failed| Modbus_S_Wait[await 500ms] --> Delay50
    Modbus_S_Connect -->|ok| Modbus_S_Write
    Modbus_S_Check -->|ano| Modbus_S_Write["WriteMultipleRegisters<br/>(vstupy na server)"]
    Modbus_S_Write --> Modbus_S_Read["ReadHoldingRegisters<br/>(výstupy ze serveru)"]
    Modbus_S_Read --> Modbus_S_Plant[PlantModel.ComputePlantStep]
    Modbus_S_Plant --> Modbus_S_CarLight[ReadHoldingRegisters:<br/>CarLight výstupy] --> Modbus_S_Status[SetStatus: Synchronized] --> Delay50
    
    Modbus_CheckMode -->|else| Modbus_NoSelect[SetStatus: Choose mode] --> Delay50

    %% ════════════════ Sharp7 ════════════════
    Sharp7_CheckConn -->|ne| Sharp7_Reconnect{connectToPLC(ip)}
    Sharp7_Reconnect -->|failed| Sharp7_Wait[await 500ms] --> Delay50
    Sharp7_Reconnect -->|ok=0| Sharp7_Read
    Sharp7_CheckConn -->|ano| Sharp7_Read[readDB - DB1, 182 bytes]
    Sharp7_Read --> Sharp7_Decode["Dekódování:<br/>GetBitAt, GetRealAt"]
    Sharp7_Decode --> Sharp7_Plant[PlantModel.ComputePlantStep]
    Sharp7_Plant --> Sharp7_Encode["Kódování:<br/>SetBitAt, SetRealAt, SetIntAt"]
    Sharp7_Encode --> Sharp7_Write[writeDB - DB1, 182 bytes]
    Sharp7_Write --> Delay50

    %% ════════════════ REST API ════════════════
    REST_Status --> Delay50

    %% ════════════════ Common ════════════════
    LogUnknown --> Delay50

    Delay50[await Task.Delay 50ms] --> CheckFlags

    %% ════════════════ Error Handling ════════════════
    MQTT_M_Publish -.->|Exception| ErrorHandler["catch Exception:<br/>Logger.LogException<br/>await 500ms<br/>continue"]
    MQTT_S_Publish -.->|Exception| ErrorHandler
    OPCUA_BulkRead -.->|Exception| ErrorHandler
    Modbus_M_Write -.->|Exception| ErrorHandler
    Modbus_S_Write -.->|Exception| ErrorHandler
    Sharp7_Read -.->|Exception| ErrorHandler
    ErrorHandler --> Delay50

    style Start fill:#9f9,stroke:#333
    style End fill:#f99,stroke:#333
    style Delay50 fill:#ff9,stroke:#333,stroke-width:2px
    style ErrorHandler fill:#f66,stroke:#333,color:#fff
```

## Popis stavů

| Stav | Popis |
|------|-------|
| **CheckFlags** | Kontrola `communicationThreadRunningFlag` a `CancellationToken` |
| **Switch** | Výběr protokolu podle `internalVariables.communicationFlag` |
| **Master/Slave** | Výběr režimu podle `checkBoxMaster` / `checkBoxSlave` |
| **Connected?** | Kontrola připojení k serveru/brokeru/PLC |
| **Read/Write** | Přenos dat mezi aplikací a PLC |
| **PlantModel** | Výpočet RC obvodu (vždy mezi čtením a zápisem) |
| **Delay 50ms** | Pauza na konci každé iterace smyčky |
| **ErrorHandler** | Logování chyby + 500ms pauza + continue |
