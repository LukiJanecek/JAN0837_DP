using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JAN0837_DP.Data
{
    public class TestData
    {
        public int number { get; set; } = 0;

        public string text { get; set; } = "";

        public string toggle { get; set; } = "";

        public bool ToggleBool =>
           !string.IsNullOrWhiteSpace(toggle) &&
           (toggle.Equals("true", System.StringComparison.OrdinalIgnoreCase) ||
            toggle.Equals("on", System.StringComparison.OrdinalIgnoreCase) ||
            toggle.Equals("1", System.StringComparison.OrdinalIgnoreCase) ||
            toggle.Equals("yes", System.StringComparison.OrdinalIgnoreCase));

        public void Update(TestData other)
        {
            if (other == null)
            {
                return;
            }
            else
            {
                number = other.number;
                text = other.text ?? "";
                toggle = other.toggle ?? "";
            }
        }

        public static class AppState
        {
            private static readonly object _lock = new();
            private static TestData _data = new();

            public static TestData Get()
            {
                lock (_lock)
                {
                    return new TestData
                    {
                        number = _data.number,
                        text = _data.text,
                        toggle = _data.toggle
                    };
                }
            }

            public static void Set(TestData value)
            {
                if (value == null) return;
                lock (_lock)
                {
                    _data.Update(value);
                }
            }
        }
    }
}
