using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Data
{
    public class TestData
    {
        public static int number { get; set; } = 0;

        public static string text { get; set; } = "";

        public static string toggle { get; set; } = "";
    }

    public class TestDataSnapshot
    {
        public int number { get; set; }
        public string text { get; set; } = "";
        public string toggle { get; set; } = "";

        public bool ToggleBool =>
            !string.IsNullOrWhiteSpace(toggle) &&
            (toggle.Equals("true", System.StringComparison.OrdinalIgnoreCase) ||
             toggle.Equals("on", System.StringComparison.OrdinalIgnoreCase) ||
             toggle.Equals("1", System.StringComparison.OrdinalIgnoreCase) ||
             toggle.Equals("yes", System.StringComparison.OrdinalIgnoreCase));
    }
}
