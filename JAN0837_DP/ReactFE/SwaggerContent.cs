using System;

namespace JAN0837_DP.ReactFE
{
    internal static class SwaggerContent
    {
        public static string GetOpenApiSpec(string host)
        {
            return $$"""
            {
              "openapi": "3.0.3",
              "info": {
                "title": "JAN0837_DP API",
                "description": "REST API for communication between React FE and WinForms backend.\n\nGET returns all data grouped by category. POST accepts partial updates in the same nested structure.",
                "version": "1.0.0"
              },
              "servers": [
                { "url": "http://{{host}}/api", "description": "Current server" }
              ],
              "paths": {
                "/data": {
                  "get": {
                    "summary": "Get all data",
                    "description": "Returns current state of all data categories: TestData, CrossroadData, CrosswalkData, RegulatorData, CarWash, WashingMachine, CarLight.",
                    "responses": {
                      "200": {
                        "description": "Current state of all categories",
                        "content": {
                          "application/json": {
                            "schema": { "$ref": "#/components/schemas/AllData" }
                          }
                        }
                      }
                    }
                  },
                  "post": {
                    "summary": "Update data",
                    "description": "Send partial updates grouped by category. Only include categories and keys you want to change.\n\nExample: `{ \"CarWash\": { \"btnStart\": \"true\" }, \"RegulatorData\": { \"R\": \"100\" } }`",
                    "requestBody": {
                      "required": true,
                      "content": {
                        "application/json": {
                          "schema": { "$ref": "#/components/schemas/UpdateRequest" },
                          "example": {
                            "CrossroadData": { "btnCrossroadStart": "true" },
                            "CarWash": { "btnStart": "true", "Mode": "1" },
                            "RegulatorData": { "R": "100", "C": "50" }
                          }
                        }
                      }
                    },
                    "responses": {
                      "200": { "description": "Update applied successfully" }
                    }
                  }
                }
              },
              "components": {
                "schemas": {
                  "AllData": {
                    "type": "object",
                    "properties": {
                      "TestData": { "$ref": "#/components/schemas/TestData" },
                      "CrossroadData": { "$ref": "#/components/schemas/CrossroadData" },
                      "CrosswalkData": { "$ref": "#/components/schemas/CrosswalkData" },
                      "RegulatorData": { "$ref": "#/components/schemas/RegulatorData" },
                      "CarWash": { "$ref": "#/components/schemas/CarWash" },
                      "WashingMachine": { "$ref": "#/components/schemas/WashingMachine" },
                      "CarLight": { "$ref": "#/components/schemas/CarLight" }
                    }
                  },
                  "UpdateRequest": {
                    "type": "object",
                    "description": "Nested object – top-level keys are category names, values are key-value pairs to update.",
                    "additionalProperties": {
                      "type": "object",
                      "additionalProperties": { "type": "string" }
                    },
                    "example": {
                      "CarWash": { "btnStart": "true" }
                    }
                  },
                  "TestData": {
                    "type": "object",
                    "properties": {
                      "number": { "type": "integer" },
                      "text": { "type": "string" },
                      "toggle": { "type": "string" }
                    }
                  },
                  "CrossroadData": {
                    "type": "object",
                    "properties": {
                      "crossroadType": { "type": "string" },
                      "btnStart": { "type": "string" },
                      "btnPause": { "type": "string" },
                      "btnStop": { "type": "string" },
                      "btnCrosswalk1": { "type": "string" },
                      "btnCrosswalk2": { "type": "string" },
                      "trafficLight1_green": { "type": "string" },
                      "trafficLight1_yellow": { "type": "string" },
                      "trafficLight1_red": { "type": "string" },
                      "trafficLight2_green": { "type": "string" },
                      "trafficLight2_yellow": { "type": "string" },
                      "trafficLight2_red": { "type": "string" },
                      "pedestrian1_green": { "type": "string" },
                      "pedestrian1_red": { "type": "string" },
                      "pedestrian2_green": { "type": "string" },
                      "pedestrian2_red": { "type": "string" }
                    }
                  },
                  "CrosswalkData": {
                    "type": "object",
                    "properties": {
                      "crosswalkType": { "type": "string" },
                      "btnStart": { "type": "string" },
                      "btnPause": { "type": "string" },
                      "btnStop": { "type": "string" },
                      "btnCrosswalk1": { "type": "string" },
                      "btnCrosswalk2": { "type": "string" },
                      "trafficLight1_green": { "type": "string" },
                      "trafficLight1_yellow": { "type": "string" },
                      "trafficLight1_red": { "type": "string" },
                      "trafficLight2_green": { "type": "string" },
                      "trafficLight2_yellow": { "type": "string" },
                      "trafficLight2_red": { "type": "string" },
                      "pedestrian1_green": { "type": "string" },
                      "pedestrian1_red": { "type": "string" },
                      "pedestrian2_green": { "type": "string" },
                      "pedestrian2_red": { "type": "string" }
                    }
                  },
                  "RegulatorData": {
                    "type": "object",
                    "properties": {
                      "switchstate": { "type": "string" },
                      "R": { "type": "string" },
                      "C": { "type": "string" },
                      "U": { "type": "string" },
                      "Td": { "type": "string" },
                      "Uc": { "type": "string" }
                    }
                  },
                  "CarWash": {
                    "type": "object",
                    "properties": {
                      "btnEmergencyStop": { "type": "string" },
                      "btnStart": { "type": "string" },
                      "btnStop": { "type": "string" },
                      "ErrorSystem": { "type": "string" },
                      "CarPosition": { "type": "string" },
                      "ShowerPosition": { "type": "string" },
                      "Mode": { "type": "string" },
                      "Light_green": { "type": "string" },
                      "Light_yellow": { "type": "string" },
                      "Light_red": { "type": "string" },
                      "Door1_Up": { "type": "string" },
                      "Door1_Down": { "type": "string" },
                      "Door2_Up": { "type": "string" },
                      "Door2_Down": { "type": "string" },
                      "ChemicalsFront": { "type": "string" },
                      "ChemicalsSides": { "type": "string" },
                      "ChemicalsBack": { "type": "string" },
                      "Prewash": { "type": "string" },
                      "Water": { "type": "string" },
                      "Wax": { "type": "string" },
                      "Dry": { "type": "string" },
                      "Brushes": { "type": "string" },
                      "Soap": { "type": "string" },
                      "ActiveFoam": { "type": "string" },
                      "TimeDoorMovement": { "type": "string" },
                      "MEMDoor": { "type": "string" },
                      "MEMDoorTrig": { "type": "string" },
                      "MEMDoorClosingtrig": { "type": "string" }
                    }
                  },
                  "WashingMachine": {
                    "type": "object",
                    "properties": {
                      "btnEmergencyStop": { "type": "string" },
                      "btnStart": { "type": "string" },
                      "btnStop": { "type": "string" },
                      "ErrorSystem": { "type": "string" },
                      "Mode": { "type": "string" },
                      "Light_green": { "type": "string" },
                      "Light_yellow": { "type": "string" },
                      "Light_red": { "type": "string" },
                      "DoorClosed": { "type": "string" },
                      "Chemicals": { "type": "string" },
                      "Prewash": { "type": "string" },
                      "Water": { "type": "string" },
                      "Dry": { "type": "string" },
                      "Brushes": { "type": "string" },
                      "Soap": { "type": "string" },
                      "ActiveFoam": { "type": "string" }
                    }
                  },
                  "CarLight": {
                    "type": "object",
                    "properties": {
                      "btnStart": { "type": "string" },
                      "btnReset": { "type": "string" },
                      "markerLight": { "type": "string" },
                      "brakeLight": { "type": "string" },
                      "turnLight": { "type": "string" },
                      "sensorPosition": { "type": "string" },
                      "sensorConnectorConnected": { "type": "string" },
                      "done": { "type": "string" }
                    }
                  }
                }
              }
            }
            """;
        }

        public static string GetSwaggerHtml(string specUrl)
        {
            return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8"/>
                <title>JAN0837_DP – Swagger UI</title>
                <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css"/>
                <style>body{margin:0;padding:0}</style>
            </head>
            <body>
                <div id="swagger-ui"></div>
                <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
                <script>
                    SwaggerUIBundle({
                        url: "{{specUrl}}",
                        dom_id: '#swagger-ui',
                        deepLinking: true,
                        presets: [SwaggerUIBundle.presets.apis, SwaggerUIBundle.SwaggerUIStandalonePreset],
                        layout: "BaseLayout",
                        tryItOutEnabled: true
                    });
                </script>
            </body>
            </html>
            """;
        }
    }
}
