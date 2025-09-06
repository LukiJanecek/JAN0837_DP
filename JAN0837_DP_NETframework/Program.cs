using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JAN0837_DP_NETframework
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                if (args.Length < 4 || args[0] != "gen")
                {
                    Console.Error.WriteLine("Usage: OpennessBridge gen <projectDir> <projectName> <typeId> [--ui|--no-ui]");
                    return 2;
                }

                string projectDir = args[1];
                string projectName = args[2];
                string typeId = args[3];
                bool withUI = args.Any(a => a.Equals("--ui", StringComparison.OrdinalIgnoreCase));

                // (volitelné) jistič načítání správných DLL z PublicAPI
                AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
                {
                    var name = new System.Reflection.AssemblyName(e.Name).Name + ".dll";
                    var baseDir = @"C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19";
                    var path = Path.Combine(baseDir, name);
                    return File.Exists(path) ? System.Reflection.Assembly.LoadFrom(path) : null;
                };

                Directory.CreateDirectory(projectDir);

                using (var tia = new TiaPortal(withUI ? TiaPortalMode.WithUserInterface : TiaPortalMode.WithoutUserInterface))
                {
                    var project = tia.Projects.Create(new DirectoryInfo(projectDir), projectName);

                    var device = project.Devices.CreateWithItem(
                        typeId,            // např. OrderNumber:6ES7 212-1BD34-0XB0/V4.5
                        "PLC_1",
                        "PLC_" + projectName
                    );

                    var cpuItem = device.DeviceItems
                        .OfType<DeviceItem>()
                        .First(di => di.GetService<SoftwareContainer>() != null);

                    var plc = (PlcSoftware)cpuItem.GetService<SoftwareContainer>().Software;

                    // FB + instanční DB
                    var fb = plc.BlockGroup.Blocks.CreateFB("FB_test", true, 1, ProgrammingLanguage.LAD);
                    plc.BlockGroup.Blocks.CreateInstanceDB("DB_FB_test", true, 1, fb.Name);

                    // Globální DB z externího zdroje
                    var dbSrc = @"DATA_BLOCK ""DB_Process""
                        { S7_Optimized_Access := 'TRUE' }
                        VERSION : 0.1
                          VAR
                            MyBool : Bool := FALSE;
                            MyInt  : Int  := 42;
                            MyReal : Real := 3.1415;
                            MyText : String[20] := 'Ahoj';
                          END_VAR
                        BEGIN
                        END_DATA_BLOCK";

                    var tmp = Path.Combine(Path.GetTempPath(), "DB_Process.db");
                    File.WriteAllText(tmp, dbSrc, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

                    var ext = plc.ExternalSourceGroup.ExternalSources.CreateFromFile("DB_Process", tmp);
                    ext.GenerateBlocksFromSource();

                    project.Save();
                }

                Console.WriteLine("OK");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}
