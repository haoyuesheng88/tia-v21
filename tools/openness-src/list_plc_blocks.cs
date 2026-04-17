using System;
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
    var process = TiaPortal.GetProcess(pid, 10000);
    using (var portal = process.Attach()) {
      var project = portal.Projects.First();
      foreach (Device device in project.Devices) {
        foreach (DeviceItem item in device.DeviceItems) {
          var c = item.GetService<SoftwareContainer>();
          var plc = c == null ? null : c.Software as PlcSoftware;
          if (plc == null || plc.Name != plcName) continue;
          foreach (PlcBlock block in plc.BlockGroup.Blocks) {
            Console.WriteLine("BLOCK|" + block.Name + "|" + block.GetType().FullName);
          }
          return 0;
        }
      }
    }
    return 1;
  }
}
