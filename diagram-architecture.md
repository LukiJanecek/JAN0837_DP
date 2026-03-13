# Architektura řešení – Blokový diagram

Celkový přehled architektury aplikace JAN0837_DP: propojení React frontendu, C# WinForms backendu, komunikačních protokolů a PLC.

```mermaid
graph TB
    subgraph "React Frontend (port 3000)"
        ReactApp["React SPA<br/>(App.js)"]
        Pages["Stránky:<br/>CrossroadPage<br/>CrosswalkPage<br/>RegulatorPage<br/>CarLightPage<br/>CommunicationPage"]
        ReactApp --> Pages
    end

    subgraph "OWIN Self-Hosted Server (port 5000)"
        FEServer["FEserver.cs<br/>(OWIN Middleware)"]
        RESTAPI["REST API<br/>GET/POST /api/data"]
        StaticFiles["Statické soubory<br/>(React build)"]
        FEServer --> RESTAPI
        FEServer --> StaticFiles
    end

    subgraph "C# WinForms Application (.NET 8)"
        MainForm["MainForm.cs<br/>(Hlavní formulář)"]
        ucComm["ucCommunicationControl<br/>(Výběr protokolu)"]
        FECommCtrl["FEcommunicationControl<br/>(HttpListener API)"]
        CommManager["CommunicationManager<br/>(Komunikační smyčka)"]
        TIA["TIAcontrol<br/>(TIA Portal V20)"]
        MainForm --> ucComm
        MainForm --> FECommCtrl
        ucComm --> CommManager
    end

    subgraph "Sdílená datová vrstva (Static Classes)"
        CrossroadData["CrossroadData"]
        CrosswalkData["CrosswalkData"]
        RegulatorData["RegulatorData"]
        CarLightData["CarLightData"]
        PlantModel["PlantModel<br/>(RC obvod simulace)"]
        InternalVars["internalVariables<br/>(porty, flagy, nastavení)"]
    end

    subgraph "Komunikační protokoly"
        MQTT["MQTT<br/>(MQTTnet)"]
        OPCUA["OPC UA<br/>(OPC Foundation SDK)"]
        Modbus["Modbus TCP/IP<br/>(NModbus)"]
        Sharp7["Sharp7<br/>(S7 Protocol)"]
        TCPIP["TCP/IP<br/>(zakomentováno)"]
    end

    subgraph "PLC / Hardwarová vrstva"
        PLC["Siemens PLC<br/>(S7-1200/1500)"]
        MQTTBroker["MQTT Broker<br/>(externí/interní)"]
    end

    Pages <-->|"HTTP GET/POST<br/>/api/data"| RESTAPI
    RESTAPI <-->|"čtení/zápis"| CrossroadData
    RESTAPI <-->|"čtení/zápis"| CrosswalkData
    RESTAPI <-->|"čtení/zápis"| RegulatorData
    RESTAPI <-->|"čtení/zápis"| CarLightData

    CommManager <-->|"čtení/zápis"| CrossroadData
    CommManager <-->|"čtení/zápis"| CrosswalkData
    CommManager <-->|"čtení/zápis"| RegulatorData
    CommManager <-->|"čtení/zápis"| CarLightData
    CommManager -->|"ComputePlantStep()"| PlantModel
    CommManager -->|"switch(communicationFlag)"| MQTT
    CommManager -->|"switch(communicationFlag)"| OPCUA
    CommManager -->|"switch(communicationFlag)"| Modbus
    CommManager -->|"switch(communicationFlag)"| Sharp7

    MQTT <-->|"Publish/Subscribe"| MQTTBroker
    MQTTBroker <-->|"Publish/Subscribe"| PLC
    OPCUA <-->|"BulkRead/BulkWrite"| PLC
    Modbus <-->|"ReadRegisters/WriteRegisters"| PLC
    Sharp7 <-->|"ReadDB/WriteDB"| PLC

    TIA -.->|"Siemens.Engineering API"| PLC

    style CommManager fill:#f9f,stroke:#333,stroke-width:2px
    style PlantModel fill:#ff9,stroke:#333,stroke-width:2px
    style PLC fill:#9cf,stroke:#333,stroke-width:2px
```

## Popis vrstev

| Vrstva | Technologie | Popis |
|--------|-------------|-------|
| **Frontend** | React.js, Bootstrap | SPA s React Router, stránky pro každý model |
| **API Server** | OWIN Self-Hosted (port 5000) | Servíruje React build + REST API endpointy |
| **WinForms GUI** | .NET 8 WinForms | Hlavní formulář s ovládacími prvky komunikace |
| **Sdílená data** | Statické C# třídy | Mezivrstevní sdílení dat bez kopírování |
| **Komunikace** | MQTT, OPC UA, Modbus, Sharp7 | Výběr protokolu přes `internalVariables.communicationFlag` |
| **PLC** | Siemens S7-1200/1500 | Průmyslový řadič s logikou křižovatky/regulátoru |
