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
    string moduleName = args[2];
    var process = TiaPortal.GetProcess(pid, 10000);
    using (var portal = process.Attach()) {
      var project = portal.Projects.First();
      foreach (Device device in project.Devices) {
        foreach (DeviceItem item in device.DeviceItems) {
          var c = item.GetService<SoftwareContainer>();
          if (c == null || c.Software == null) continue;
          if (c.Software.Name != "HMI_RT_2") continue;
          var scripts = c.Software.GetType().GetProperty("Scripts").GetValue(c.Software, null);
          foreach (var module in (System.Collections.IEnumerable)scripts) {
            var t = module.GetType();
            var name = (string)t.GetProperty("Name").GetValue(module, null);
            if (name != moduleName) continue;
            var export = t.GetMethod("Export", new Type[] { typeof(DirectoryInfo), typeof(string) });
            var files = (System.Collections.Generic.IEnumerable<FileInfo>)export.Invoke(module, new object[] { new DirectoryInfo(dir), moduleName });
            foreach (var f in files) Console.WriteLine("EXPORTED|" + f.FullName);
            return 0;
          }
        }
      }
    }
    return 1;
  }
}
