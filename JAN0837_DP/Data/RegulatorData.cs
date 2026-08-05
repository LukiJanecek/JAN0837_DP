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

        // NRR
        public static bool nrrEnabled { get; set; } = false; // Bool -> enable NRR mode (if true, Uin is ignored and set to Y)
        public static string K1 { get; set; } = "1.0"; // Real -> gain 1st stage (can be used for gain tuning)
        public static string K2 { get; set; } = "1.0"; // Real -> gain 2nd stage (can be used for gain tuning)
        public static string PlantGain { get; set; } = "1.0"; // Real -> G(s) gain
        public static string Theta { get; set; } = "0.0"; // Real -> adaptive parameter (can be either tau1 or tau1+tau2 based on order)
        public static string Tau1 { get; set; } = "0.0"; // Real -> time constant 1st order
        public static string Tau2 { get; set; } = "0.0"; // Real -> time constant 2nd order (0.0 if 1st order)
        public static string TransferFunction { get; set; } = "0"; // String -> transfer function in human-readable form (e.g. "K / (tau1*s + 1)" or "K / ((tau1*s + 1)(tau2*s + 1))")
        public static string A1 { get; set; } = "0.0"; // Real -> discretization parameter for 1st order (a1 = exp(-Ts/tau1))
        public static string A2 { get; set; } = "0.0"; // Real -> discretization parameter for 2nd order (a2 = exp(-Ts/tau2), 0.0 if 1st order)
        public static string Y { get; set; } = "0.0"; // Real - feedback for regulator (can be either Uc1 or Uc2 based on order)
        public static string SuggestedKp { get; set; } = "0.0"; // Real -> suggested Kp for PID tuning based on current plant parameters
        public static string SuggestedKi { get; set; } = "0.0"; // Real -> suggested Ki for PID tuning based on current plant parameters
        public static string SuggestedKd { get; set; } = "0.0"; // Real -> suggested Kd for PID tuning based on current plant parameters
        public static string SuggestedTi { get; set; } = "0.0"; // Real -> suggested Ti for PID tuning based on current plant parameters
        public static string SuggestedTd { get; set; } = "0.0"; // Real -> suggested Td for PID tuning based on current plant parameters
        public static string PlantChanged { get; set; } = "false"; // Bool -> indicates if plant parameters have changed since last reset (used to trigger PID retuning)
        public static double _prevR1 { get; set; } = double.NaN; // Real -> previous R1 for change detection
        public static double _prevC1 { get; set; } = double.NaN; // Real -> previous C1 for change detection
        public static double _prevR2 { get; set; } = double.NaN; // Real -> previous R2 for change detection
        public static double _prevC2 { get; set; } = double.NaN; // Real -> previous C2 for change detection


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

        public static class Sharp7Addresses
        {
            public static int address_btnReset = 74;
            public static int bit_btnReset = 0;
            public static int address_switchstate = 74;
            public static int bit_switchstate = 1;
            public static int address_order = 76;
            public static int bit_order = 0;
            public static int address_R1 = 78;
            public static int bit_R1 = 0;
            public static int address_R2 = 82;
            public static int bit_R2 = 0;
            public static int address_C1 = 86;
            public static int bit_C1 = 0;
            public static int address_C2 = 90;
            public static int bit_C2 = 0;
            public static int address_Uc1 = 94;
            public static int bit_Uc1 = 0;
            public static int address_Uc2 = 98;
            public static int bit_Uc2 = 0;
            public static int address_Td = 102;
            public static int bit_Td = 0;
            public static int address_Ts = 106;
            public static int bit_Ts = 0;

            public static int address_Uin = 126;
            public static int bit_Uin = 0;
        }

        public static class OpcUaNodeIds
        {
            // Inputs
            public static string btnReset { get; set; } = "ns=4;i=88";
            public static string switchstate { get; set; } = "ns=4;i=89";
            public static string order { get; set; } = "ns=4;i=90";
            public static string R1 { get; set; } = "ns=4;i=91";
            public static string R2 { get; set; } = "ns=4;i=92";
            public static string C1 { get; set; } = "ns=4;i=93";
            public static string C2 { get; set; } = "ns=4;i=94";
            public static string Uc1 { get; set; } = "ns=4;i=95";
            public static string Uc2 { get; set; } = "ns=4;i=96";
            public static string Td { get; set; } = "ns=4;i=97";
            public static string Ts { get; set; } = "ns=4;i=98";

            // Outputs 
            public static string Uin { get; set; } = "ns=4;i=106";
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
            => v.ToString("G", CultureInfo.InvariantCulture);
        private static int ParseOrder(string s, int fallback = 1)
        {
            if (string.IsNullOrWhiteSpace(s))
                return fallback;

            s = s.Trim();

            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                return i;

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return (int)Math.Round(d);

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.GetCultureInfo("cs-CZ"), out d))
                return (int)Math.Round(d);

            return fallback;
        }

        public static void ComputePlantStep()
        {
            if (RegulatorData.nrrEnabled)
            {
                ComputePlantWithAdaptive();
            }
            else
            {
                RegulatorData.Update(() =>
                {
                    if (ParseBool(RegulatorData.btnReset))
                    {
                        RegulatorData.Uc1 = "0.0";
                        RegulatorData.Uc2 = "0.0";
                        RegulatorData.btnReset = "false";
                        return;
                    }

                    bool enable = ParseBool(RegulatorData.switchstate);
                    int order = ParseOrder(RegulatorData.order, 1);

                    double Ts = ParseDouble(RegulatorData.Ts, 0.1);
                    if (Ts <= 0.0)
                    {
                        return;
                    }

                    double R1 = ParseDouble(RegulatorData.R1, 0.0);
                    double C1 = ParseDouble(RegulatorData.C1, 0.0) * 1e-6;
                    double R2 = ParseDouble(RegulatorData.R2, 0.0);
                    double C2 = ParseDouble(RegulatorData.C2, 0.0) * 1e-6;

                    double u = enable ? ParseDouble(RegulatorData.Uin, 0.0) : 0.0;

                    double uc1 = ParseDouble(RegulatorData.Uc1, 0.0);
                    double uc2 = ParseDouble(RegulatorData.Uc2, 0.0);

                    if (R1 <= 0.0 || C1 <= 0.0)
                    {
                        return;
                    }

                    if (order == 1)
                    {
                        double a1 = Math.Exp(-Ts / (R1 * C1));
                        uc1 = a1 * uc1 + (1.0 - a1) * u;

                        RegulatorData.Uc1 = ToStr(uc1);
                        RegulatorData.Uc2 = "0.0";
                        return;
                    }

                    if (order == 2)
                    {
                        if (R2 <= 0.0 || C2 <= 0.0)
                        {
                            return;
                        }

                        // Stav na začátku kroku
                        double uc1Old = uc1;
                        double uc2Old = uc2;

                        // Diferenciální rovnice RCRC
                        double duc1 =
                            ((u - uc1Old) / R1 - (uc1Old - uc2Old) / R2) / C1;

                        double duc2 =
                            ((uc1Old - uc2Old) / R2) / C2;

                        // Eulerův krok
                        uc1 = uc1Old + Ts * duc1;
                        uc2 = uc2Old + Ts * duc2;

                        RegulatorData.Uc1 = ToStr(uc1);
                        RegulatorData.Uc2 = ToStr(uc2);
                        return;
                    }

                    RegulatorData.Uc1 = "0.0";
                    RegulatorData.Uc2 = "0.0";
                });
            }
        }

        public static void ComputePlantWithAdaptive()
        {
            RegulatorData.Update(() =>
            {
                if (ParseBool(RegulatorData.btnReset))
                {
                    RegulatorData.Uc1 = "0.0";
                    RegulatorData.Uc2 = "0.0";
                    RegulatorData.Y = "0.0";

                    RegulatorData.PlantGain = "1.0";
                    RegulatorData.Tau1 = "0.0";
                    RegulatorData.Tau2 = "0.0";
                    RegulatorData.Theta = "0.0";
                    RegulatorData.TransferFunction = "0";

                    RegulatorData.SuggestedKp = "0.0";
                    RegulatorData.SuggestedKi = "0.0";
                    RegulatorData.SuggestedKd = "0.0";
                    RegulatorData.SuggestedTi = "0.0";
                    RegulatorData.SuggestedTd = "0.0";

                    RegulatorData.PlantChanged = "false";

                    RegulatorData._prevR1 = double.NaN;
                    RegulatorData._prevC1 = double.NaN;
                    RegulatorData._prevR2 = double.NaN;
                    RegulatorData._prevC2 = double.NaN;

                    RegulatorData.btnReset = "false";
                    return;
                }

                bool enable = ParseBool(RegulatorData.switchstate);
                int order = ParseOrder(RegulatorData.order, 1);

                double Ts = ParseDouble(RegulatorData.Ts, 0.1);
                if (Ts <= 0.0)
                {
                    return;
                }

                double R1 = ParseDouble(RegulatorData.R1, 0.0);
                double C1 = ParseDouble(RegulatorData.C1, 0.0) * 1e-6; // µF -> F
                double R2 = ParseDouble(RegulatorData.R2, 0.0);
                double C2 = ParseDouble(RegulatorData.C2, 0.0) * 1e-6; // µF -> F

                double K1 = ParseDouble(RegulatorData.K1, 1.0);
                double K2 = ParseDouble(RegulatorData.K2, 1.0);

                double u = enable ? ParseDouble(RegulatorData.Uin, 0.0) : 0.0;

                double uc1 = ParseDouble(RegulatorData.Uc1, 0.0);
                double uc2 = ParseDouble(RegulatorData.Uc2, 0.0);

                if (R1 <= 0.0 || C1 <= 0.0)
                {
                    return;
                }

                bool secondOrderValid = (order == 2 && R2 > 0.0 && C2 > 0.0);

                const double eps = 1e-12;

                bool plantChanged =
                    double.IsNaN(RegulatorData._prevR1) ||
                    Math.Abs(R1 - RegulatorData._prevR1) > eps ||
                    Math.Abs(C1 - RegulatorData._prevC1) > eps ||
                    Math.Abs(R2 - RegulatorData._prevR2) > eps ||
                    Math.Abs(C2 - RegulatorData._prevC2) > eps;
                RegulatorData.PlantChanged = plantChanged ? "true" : "false";

                double tau1 = R1 * C1;
                double tau2 = secondOrderValid ? R2 * C2 : 0.0;

                double plantGain = secondOrderValid ? (K1 * K2) : K1;

                // Ekvivalentní parametry pro návrh PID
                double Teq;
                double theta;
                string transferFunction;

                if (secondOrderValid)
                {
                    // 2. řád aproximujeme pro návrh PID jako FOPDT
                    Teq = tau1 + tau2;
                    theta = 0.5 * (tau1 + tau2);

                    transferFunction =
                        $"{ToStr(plantGain)} / (({ToStr(tau1)} * s + 1) * ({ToStr(tau2)} * s + 1))";
                }
                else
                {
                    Teq = tau1;
                    theta = tau1;

                    transferFunction =
                        $"{ToStr(plantGain)} / ({ToStr(tau1)} * s + 1)";
                }

                // Bezpečnost proti dělení nulou / nesmyslným hodnotám
                if (plantGain <= 0.0)
                {
                    plantGain = 1.0;
                }

                if (Teq <= 0.0)
                {
                    Teq = Ts;
                }

                if (theta <= 0.0)
                {
                    theta = Ts;
                }

                if (plantChanged)
                {
                    // Konzervativní PID návrh
                    double kp = 1.2 * (Teq / (plantGain * theta));
                    double ti = 2.0 * theta;
                    double td = 0.5 * theta;

                    // Přepočet na Ki, Kd
                    double ki = (ti > 0.0) ? (kp / ti) : 0.0;
                    double kd = kp * td;

                    RegulatorData.SuggestedKp = ToStr(kp);
                    RegulatorData.SuggestedKi = ToStr(ki);
                    RegulatorData.SuggestedKd = ToStr(kd);

                    RegulatorData.SuggestedTi = ToStr(ti);
                    RegulatorData.SuggestedTd = ToStr(td);

                    // uložení nových parametrů jako referenčních
                    RegulatorData._prevR1 = R1;
                    RegulatorData._prevC1 = C1;
                    RegulatorData._prevR2 = R2;
                    RegulatorData._prevC2 = C2;
                }

                double a1 = Math.Exp(-Ts / tau1);
                uc1 = a1 * uc1 + K1 * (1.0 - a1) * u;

                if (secondOrderValid)
                {
                    double a2 = Math.Exp(-Ts / tau2);
                    uc2 = a2 * uc2 + K2 * (1.0 - a2) * uc1;

                    RegulatorData.Uc1 = ToStr(uc1);
                    RegulatorData.Uc2 = ToStr(uc2);
                    RegulatorData.Y = ToStr(uc2);
                }
                else
                {
                    uc2 = 0.0;

                    RegulatorData.Uc1 = ToStr(uc1);
                    RegulatorData.Uc2 = "0.0";
                    RegulatorData.Y = ToStr(uc1);
                }

                RegulatorData.PlantGain = ToStr(plantGain);
                RegulatorData.Tau1 = ToStr(tau1);
                RegulatorData.Tau2 = ToStr(tau2);
                RegulatorData.Theta = ToStr(theta);
                RegulatorData.TransferFunction = transferFunction;
            });
        }
    }
}
