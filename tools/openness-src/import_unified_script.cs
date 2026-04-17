using System;
using System.Linq;
using System.IO;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;

class Program {
  [STAThread]
  static int Main(string[] args) {
    int pid = int.Parse(args[0]);
    string dir = args[1];
    string name = args.Length > 2 ? args[2] : null;
    var process = TiaPortal.GetProcess(pid, 10000);
    using (var portal = process.Attach()) {
      var project = portal.Projects.First();
      foreach (Device device in project.Devices) {
        foreach (DeviceItem item in device.DeviceItems) {
          var c = item.GetService<SoftwareContainer>();
          if (c == null || c.Software == null) continue;
          if (c.Software.Name != "HMI_RT_2") continue;
          var scripts = c.Software.GetType().GetProperty("Scripts").GetValue(c.Software, null);
          bool ok;
          if (string.IsNullOrEmpty(name)) {
            ok = (bool)scripts.GetType().GetMethod("Import", new Type[] { typeof(DirectoryInfo) }).Invoke(scripts, new object[] { new DirectoryInfo(dir) });
          } else {
            ok = (bool)scripts.GetType().GetMethod("Import", new Type[] { typeof(DirectoryInfo), typeof(string) }).Invoke(scripts, new object[] { new DirectoryInfo(dir), name });
          }
          Console.WriteLine("IMPORTED|" + ok);
          return 0;
        }
      }
    }
    return 1;
  }
}
