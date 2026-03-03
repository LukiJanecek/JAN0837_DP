using Microsoft.VisualBasic.Devices;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Data
{
    public static class RegulatorData
    {
        // Inputs 
        public static string btnReset { get; set; } = "false"; // Bool
        public static string switchstate { get; set; } = "false"; // Bool
        public static string order { get; set; } = "1"; // int
        public static string R1 { get; set; } = "0.0"; // Real
        public static string R2 { get; set; } = "0.0"; // Real
        public static string C1 { get; set; } = "0.0"; // Real
        public static string C2 { get; set; } = "0.0"; // Real
        public static string Uc1 { get; set; } = "0.0"; // Real
        public static string Uc2 { get; set; } = "0.0"; // Real
        public static string Td { get; set; } = "0.0"; // Real -> transport delay                                                  
        public static string Ts { get; set; } = "0.1"; // sampling time

        // Outputs 
        public static string Uin { get; set; } = "0.0"; // Real

        // Thread safety 
        private static readonly object _lock = new();

        // Snapshot
        public readonly record struct State(
            string btnReset,
            string switchstate,
            string order,
            string R1,
            string R2,
            string C1,
            string C2,
            string Uc1,
            string Uc2,
            string Td,
            string Ts,
            string Uin
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    btnReset,
                    switchstate,
                    order,
                    R1,
                    R2,
                    C1,
                    C2,
                    Uc1,
                    Uc2,
                    Td,
                    Ts,
                    Uin
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                if (s.Uin != null) Uin = s.Uin;
            }
        }

        public static void Update(Action updater)
        {
            if (updater is null) return;
            lock (_lock) updater();
        }

        public static class OpcUaNodeIds
        {
            // Inputs
            public static string btnReset { get; set; } = "ns=4;i=?";
            public static string switchstate { get; set; } = "ns=4;i=?";
            public static string order { get; set; } = "ns=4;i=?";
            public static string R1 { get; set; } = "ns=4;i=?";
            public static string R2 { get; set; } = "ns=4;i=?";
            public static string C1 { get; set; } = "ns=4;i=?";
            public static string C2 { get; set; } = "ns=4;i=?";
            public static string Uc1 { get; set; } = "ns=4;i=?";
            public static string Uc2 { get; set; } = "ns=4;i=?";
            public static string Td { get; set; } = "ns=4;i=?";
            public static string Ts { get; set; } = "ns=4;i=?";

            // Outputs 
            public static string Uin { get; set; } = "ns=4;i=?";
        }
    }

    public static class PlantModel
    {
        private static double ParseDouble(string s, double fallback = 0.0)
            => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;

        private static int ParseInt(string s, int fallback = 1)
            => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

        private static bool ParseBool(string s)
            => bool.TryParse(s, out var v) && v;

        private static string ToStr(double v)
            => v.ToString("0.#####", CultureInfo.InvariantCulture);

        /// <summary>
        /// One simulation step (RC 1st/2nd order cascade) using exact discrete-time update.
        /// Updates RegulatorData.Uc1 and RegulatorData.Uc2 under lock.
        /// Transport delay (Td) intentionally ignored.
        /// </summary>
        public static void ComputePlantStep()
        {
            RegulatorData.Update(() =>
            {
                // Reset handling (from FE/app side)
                if (ParseBool(RegulatorData.btnReset))
                {
                    RegulatorData.Uc1 = "0.0";
                    RegulatorData.Uc2 = "0.0";

                    // pokud chceš, aby reset byl "one-shot":
                    RegulatorData.btnReset = "false";
                    return;
                }

                bool enable = ParseBool(RegulatorData.switchstate);
                if (!enable)
                {
                    // simulace stojí -> drž poslední Uc1/Uc2
                    return;
                }

                int order = ParseInt(RegulatorData.order, 1);

                double Ts = ParseDouble(RegulatorData.Ts, 0.1);
                if (Ts <= 0) return;

                double R1 = ParseDouble(RegulatorData.R1, 0.0);
                double C1 = ParseDouble(RegulatorData.C1, 0.0);
                double R2 = ParseDouble(RegulatorData.R2, 0.0);
                double C2 = ParseDouble(RegulatorData.C2, 0.0);

                // vstup z PID (LMN)
                double u = ParseDouble(RegulatorData.Uin, 0.0);

                // aktuální stavy
                double uc1 = ParseDouble(RegulatorData.Uc1, 0.0);
                double uc2 = ParseDouble(RegulatorData.Uc2, 0.0);

                // Validace
                if (R1 <= 0 || C1 <= 0) return;
                if (order == 2 && (R2 <= 0 || C2 <= 0)) return;

                // 1st stage (RC)
                double a1 = Math.Exp(-Ts / (R1 * C1));
                uc1 = a1 * uc1 + (1.0 - a1) * u;

                if (order == 1)
                {
                    RegulatorData.Uc1 = ToStr(uc1);
                    RegulatorData.Uc2 = "0.0"; // volitelně
                    return;
                }

                // 2nd stage (cascade) input = uc1
                double a2 = Math.Exp(-Ts / (R2 * C2));
                uc2 = a2 * uc2 + (1.0 - a2) * uc1;

                RegulatorData.Uc1 = ToStr(uc1);
                RegulatorData.Uc2 = ToStr(uc2);
            });
        }
    }
}
