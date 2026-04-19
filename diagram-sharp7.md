# Sharp7 – Sekvenční diagram komunikace

Detailní pohled na Sharp7 (S7 Protocol) komunikaci – přímé čtení/zápis datového bloku PLC.

```mermaid
sequenceDiagram
    participant CM as CommunicationManager
    participant S7 as comSharp7<br/>(S7Client wrapper)
    participant PLC as Siemens PLC<br/>(S7-1200/1500)
    participant Data as Sdílená data
    participant Plant as PlantModel

    Note over CM,PLC: === SHARP7 – PŘÍMÉ DB ČTENÍ/ZÁPIS ===
    Note over CM: activeDBnumber = 1<br/>readBuffer = byte[182]<br/>writeBuffer = byte[182]

    CM->>S7: Kontrola: client.Connected?

    alt PLC nepřipojeno
        CM->>S7: connectToPLC(ipAddress)
        S7->>PLC: S7 Connection request
        alt Připojení OK (return 0)
            PLC-->>S7: Connected
            CM->>CM: SetStatus("Reconnected")
        else Připojení selhalo
            CM->>CM: SetStatus("Error, retrying...")
            CM->>CM: await Task.Delay(500ms)
            CM->>CM: continue
        end
    end

    rect rgb(200, 255, 200)
        Note over CM,PLC: 1) ČTENÍ CELÉHO DATOVÉHO BLOKU
        CM->>S7: readDB(DB1, readBuffer[182], offset=0)
        S7->>PLC: S7 ReadArea (DB1, 182 bytes)
        PLC-->>S7: 182 bytes dat
        S7-->>CM: return code (0 = OK)

        alt ReadDB OK (return 0)
            Note over CM,Data: Dekódování bitových polí přes Sharp7.S7.GetBitAt()
            
            CM->>Data: CrossroadData výstupy (21 bitů)
            Note over CM: GetBitAt(readBuffer, address, bit)<br/>crossroadType, trafficLights, pedestrians

            CM->>Data: CrosswalkData výstupy (11 bitů)
            Note over CM: crosswalkType, trafficLight1/2, pedestrian1/2

            CM->>Data: RegulatorData.Uin
            Note over CM: GetRealAt(readBuffer, address) → float

            CM->>Data: CarLightData výstupy (4 bity)
            Note over CM: lowBeamLight, highBeamLight, turnLight, result
        else ReadDB chyba
            CM->>CM: SetStatus("Error in ReadDB")
            CM->>CM: Logger.LogError()
        end
    end

    rect rgb(255, 255, 200)
        Note over CM,Plant: 2) VÝPOČET PLANT MODELU
        CM->>Plant: PlantModel.ComputePlantStep()
        Plant->>Data: Aktualizace Uc1, Uc2
    end

    rect rgb(255, 200, 200)
        Note over CM,PLC: 3) ZÁPIS CELÉHO DATOVÉHO BLOKU
        Note over CM,Data: Kódování do writeBuffer přes Sharp7.S7.SetBitAt/SetRealAt/SetIntAt

        CM->>Data: Čtení CrossroadData (vstupy + výstupy)
        Note over CM: SetBitAt(writeBuffer, address, bit, value)<br/>btnStart, btnPause, btnStop, btnCrosswalk...<br/>+ crossroadType, trafficLights, pedestrians

        CM->>Data: Čtení CrosswalkData (vstupy + výstupy)
        Note over CM: SetBitAt pro všechny bity

        CM->>Data: Čtení RegulatorData (vstupy + výstupy)
        Note over CM: SetBitAt: btnReset, switchstate<br/>SetIntAt: order<br/>SetRealAt: R1, R2, C1, C2, Uc1, Uc2, Td, Ts, Uin

        CM->>Data: Čtení CarLightData (vstupy + výstupy)
        Note over CM: SetBitAt: btnReset, error, sensors,<br/>lowBeam, highBeam, turnLight, result

        CM->>S7: writeDB(DB1, writeBuffer[182], offset=0)
        S7->>PLC: S7 WriteArea (DB1, 182 bytes)
        PLC-->>S7: Write confirmation
        S7-->>CM: return code (0 = OK)

        alt WriteDB OK
            CM->>CM: SetStatus("WriteDB OK")
        else WriteDB chyba
            CM->>CM: SetStatus("Error in WriteDB")
            CM->>CM: Logger.LogError()
        end
    end
```

## Struktura datového bloku (DB1, 182 bytes)

```mermaid
graph LR
    subgraph "DB1 – 182 bytes"
        subgraph "CrossroadData"
            CR_IN["Vstupy (bity):<br/>btnStart, btnPause, btnStop<br/>btnWestCrosswalk1/2<br/>btnSouthCrosswalk1/2"]
            CR_OUT["Výstupy (bity):<br/>crossroadType<br/>trafficLights N/S/W/E × R/Y/G<br/>pedestrians S1/S2/W1/W2"]
        end
        subgraph "CrosswalkData"
            CW_IN["Vstupy (bity):<br/>btnStart, btnPause, btnStop<br/>btnCrosswalk1/2"]
            CW_OUT["Výstupy (bity):<br/>crosswalkType<br/>trafficLight1/2<br/>pedestrian1/2"]
        end
        subgraph "RegulatorData"
            RG_IN["Vstupy:<br/>btnReset (bit), switchstate (bit)<br/>order (INT, 2B)<br/>R1,R2,C1,C2,Uc1,Uc2,Td,Ts (REAL, 4B each)"]
            RG_OUT["Výstupy:<br/>Uin (REAL, 4B)"]
        end
        subgraph "CarLightData"
            CL_IN["Vstupy (bity):<br/>btnReset, error<br/>sensorLight, sensorConnectorConnected"]
            CL_OUT["Výstupy (bity):<br/>lowBeamLight, highBeamLight<br/>turnLight, result"]
        end
    end
```

## Adresování v Sharp7

Každá datová třída obsahuje vnořenou třídu `Sharp7Addresses` s konstantami:

```
Sharp7.S7.GetBitAt(buffer, address_byte, bit_position)  → bool
Sharp7.S7.SetBitAt(buffer, address_byte, bit_position, value)
Sharp7.S7.GetRealAt(buffer, address_byte)                → float (4 bytes)
Sharp7.S7.SetRealAt(buffer, address_byte, value)
Sharp7.S7.SetIntAt(buffer, address_byte, value)          → short (2 bytes)
```

## Specifika Sharp7

- **Přímý přístup**: Čtení/zápis celého datového bloku najednou (182 bytes)
- **Bez Master/Slave**: Žádný přepínač – vždy klientský režim
- **Bitová úroveň**: Přístup k jednotlivým bitům přes `GetBitAt`/`SetBitAt`
- **Float (REAL)**: 4 bytes v Big-Endian formátu (S7 konvence)
- **INT**: 2 bytes signed integer
- **Obousměrný zápis**: WriteDB zapisuje **vstupy i výstupy** zpět do PLC
- **Reconnect**: Automatický pokus o reconnect s `connectToPLC(ip)` při ztrátě spojení
- **DB číslo**: Pevně nastaveno na DB1 (`activeDBnumber = 1`)
- **PPI**: Typ připojení používá `S7Client.ConnectionType.PPI`
