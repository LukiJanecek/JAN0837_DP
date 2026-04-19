# Tok dat – Sekvenční diagram

Sekvenční diagram zobrazující tok dat mezi všemi vrstvami aplikace.

```mermaid
sequenceDiagram
    participant React as React Frontend<br/>(port 3000)
    participant API as OWIN API Server<br/>(port 5000)
    participant Data as Sdílená data<br/>(Static Classes)
    participant CM as CommunicationManager<br/>(50ms smyčka)
    participant Plant as PlantModel<br/>(RC simulace)
    participant Protocol as Komunikační protokol<br/>(MQTT/OPCUA/Modbus/Sharp7)
    participant PLC as PLC<br/>(Siemens)

    Note over React,PLC: === CYKLUS KOMUNIKACE (každých 50ms) ===

    React->>+API: HTTP GET /api/data
    API->>Data: Čtení výstupních hodnot
    Data-->>API: CrossroadData, RegulatorData, ...
    API-->>-React: JSON response (outputs)

    React->>+API: HTTP POST /api/data
    API->>Data: Zápis vstupních hodnot (tlačítka, parametry)
    API-->>-React: OK

    Note over CM,PLC: === KOMUNIKAČNÍ SMYČKA ===

    loop while communicationThreadRunningFlag && !token.IsCancellationRequested
        CM->>Data: Čtení vstupů (btnStart, btnPause, ...)
        
        alt MQTT
            CM->>Protocol: Publish Input topics
            Protocol->>PLC: MQTT Publish
            PLC-->>Protocol: MQTT Publish (Output topics)
            Protocol->>Data: OnMessage → OutputMapper
        else OPC UA
            Protocol->>PLC: BulkRead (všechny výstupy)
            PLC-->>Protocol: Output hodnoty
            Protocol->>Data: Zápis výstupů
        else Modbus TCP/IP
            CM->>Protocol: WriteRegisters (vstupy)
            Protocol->>PLC: Modbus Write
            PLC-->>Protocol: Modbus Read
            Protocol->>Data: ReadRegisters (výstupy)
        else Sharp7
            Protocol->>PLC: ReadDB (182 bytes)
            PLC-->>Protocol: Data block
            Protocol->>Data: Dekódování bitů/floatů
        end

        CM->>Plant: ComputePlantStep()
        Plant->>Data: Aktualizace Uc1, Uc2

        alt OPC UA
            CM->>Protocol: BulkWrite (všechny vstupy)
            Protocol->>PLC: OPC UA Write
        else Sharp7
            CM->>Data: Čtení všech hodnot
            CM->>Protocol: WriteDB (182 bytes)
            Protocol->>PLC: S7 Write
        end

        CM->>CM: await Task.Delay(50ms)
    end
```

## Klíčové body

1. **React ↔ API**: Polling přes HTTP GET/POST, data se sdílí přes statické třídy
2. **CommunicationManager**: Běží v samostatném vlákně s 50ms intervalem
3. **PlantModel**: Výpočet RC obvodu probíhá **mezi** čtením výstupů a zápisem vstupů
4. **Asynchronní výstupy (MQTT)**: Výstupy z PLC přicházejí přes `OnMessage` callback, ne v hlavní smyčce
