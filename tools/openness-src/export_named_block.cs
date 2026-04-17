using System;
using System.IO;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;

class Program {
  [STAThread]
  static int Main(string[] args) {
    int pid = int.Parse(args[0]);
    string plcName = args[1];
    string blockName = args[2];
    string outFile = args[3];
    var process = TiaPortal.GetProcess(pid, 10000);
    using (var portal = process.Attach()) {
      var project = portal.Projects.First();
      foreach (Device device in project.Devices) {
        foreach (DeviceItem item in device.DeviceItems) {
          var c = item.GetService<SoftwareContainer>();
          var plc = c == null ? null : c.Software as PlcSoftware;
          if (plc == null || plc.Name != plcName) continue;
          PlcBlock block = plc.BlockGroup.Blocks.Find(blockName);
          if (block == null) { Console.WriteLine("BLOCK_NOT_FOUND"); return 2; }
          block.Export(new FileInfo(outFile), ExportOptions.WithDefaults);
          Console.WriteLine("EXPORTED|" + outFile);
          return 0;
        }
      }
    }
    return 1;
  }
}
