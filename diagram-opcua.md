# OPC UA – Sekvenční diagram komunikace

Detailní pohled na OPC UA komunikaci – Master (Server, neimplementováno) a Slave (Client, BulkRead/BulkWrite).

```mermaid
sequenceDiagram
    participant CM as CommunicationManager
    participant Client as opcuaKlient<br/>(OPC UA Client)
    participant PLC as OPC UA Server<br/>(PLC)
    participant Data as Sdílená data
    participant Plant as PlantModel

    Note over CM,PLC: === OPC UA MASTER (Server) – NEIMPLEMENTOVÁNO ===

    alt checkBoxMaster == true
        CM->>CM: SetStatus("Not implemented")
        CM->>CM: Logger.LogError()
        CM->>CM: break
    end

    Note over CM,PLC: === OPC UA SLAVE (Client) – OPTIMALIZOVANÝ REŽIM ===

    alt checkBoxSlave == true
        CM->>Client: Kontrola: client != null && connected?
        
        alt Klient odpojen
            CM->>CM: SetStatus("Disconnected, waiting...")
            CM->>CM: await Task.Delay(1000ms)
            CM->>CM: continue
        end

        rect rgb(200, 255, 200)
            Note over CM,PLC: 1) HROMADNÉ ČTENÍ VÝSTUPŮ Z PLC
            CM->>Client: BulkReadAllOutputs()
            Client->>PLC: OPC UA Read (batch request)
            PLC-->>Client: Všechny výstupní hodnoty
            Client->>Data: Zápis do CrossroadData, CrosswalkData,<br/>RegulatorData, CarLightData
            
            alt BulkRead failed
                CM->>CM: SetStatus("BulkRead failed")
                CM->>CM: await Task.Delay(1000ms)
                CM->>CM: continue
            end
        end

        rect rgb(255, 255, 200)
            Note over CM,Plant: 2) VÝPOČET PID REGULÁTORU
            CM->>Plant: PlantModel.ComputePlantStep()
            Plant->>Data: Aktualizace Uc1, Uc2
        end

        rect rgb(255, 200, 200)
            Note over CM,PLC: 3) HROMADNÝ ZÁPIS VSTUPŮ DO PLC
            CM->>Client: BulkWriteAllInputs()
            Client->>Data: Čtení ze CrossroadData, CrosswalkData,<br/>RegulatorData, CarLightData
            Client->>PLC: OPC UA Write (batch request)
            
            alt BulkWrite failed
                CM->>CM: Logger.LogWarning("partially or fully failed")
            end
        end

        CM->>Client: Kontrola: connected?
        alt Session ztracena
            CM->>CM: SetStatus("Session lost")
            CM->>CM: await Task.Delay(1000ms)
            CM->>CM: continue
        end

        CM->>CM: SetStatus("BulkRead → Compute → BulkWrite")
    end
```

## Architektura OPC UA komunikace

```mermaid
graph LR
    subgraph "C# Aplikace (Slave/Client)"
        CM[CommunicationManager]
        BulkRead[BulkReadAllOutputs]
        BulkWrite[BulkWriteAllInputs]
        Plant[PlantModel.ComputePlantStep]
    end

    subgraph "Sdílená data"
        CD[CrossroadData<br/>OpcUaNodeIds]
        CWD[CrosswalkData<br/>OpcUaNodeIds]
        RD[RegulatorData<br/>OpcUaNodeIds]
        CLD[CarLightData<br/>OpcUaNodeIds]
    end

    subgraph "PLC (Master/Server)"
        OPCUA[OPC UA Server]
        DB[Data Block]
    end

    CM -->|"1"| BulkRead
    BulkRead -->|"Read all outputs"| OPCUA
    OPCUA --> DB
    BulkRead -->|"Zápis výstupů"| CD
    BulkRead -->|"Zápis výstupů"| CWD
    BulkRead -->|"Zápis výstupů"| RD
    BulkRead -->|"Zápis výstupů"| CLD

    CM -->|"2"| Plant

    CM -->|"3"| BulkWrite
    CD -->|"Čtení vstupů"| BulkWrite
    CWD -->|"Čtení vstupů"| BulkWrite
    RD -->|"Čtení vstupů"| BulkWrite
    CLD -->|"Čtení vstupů"| BulkWrite
    BulkWrite -->|"Write all inputs"| OPCUA
```

## Specifika OPC UA

- **Optimalizace**: ~50+ individuálních volání zredukováno na 2 (1× BulkRead + 1× BulkWrite)
- **Node ID mapování**: Každá datová třída obsahuje vnořenou třídu `OpcUaNodeIds` s konstantami node ID
- **Typy hodnot**: Bool (ReadOPCUABool), Float (ReadOPCUAFloat), Int (ReadOPCUAInt)
- **Session monitoring**: Kontrola `connected` po každé operaci, automatický reconnect
- **Reconnect delay**: 1000ms při ztrátě spojení (delší než u MQTT – 200ms)
- **Master režim**: Zakomentovaný v `#region OPCUA Server`, plánován ale neimplementován
- **Error handling**: Exception catch s 500ms delay, pak continue
