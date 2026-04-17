using System;
using System.Linq;
using System.Reflection;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;

class Program {
  [STAThread]
  static int Main(string[] args) {
    int pid = int.Parse(args[0]);
    var process = TiaPortal.GetProcess(pid, 10000);
    using (var portal = process.Attach()) {
      var project = portal.Projects.First();
      foreach (Device device in project.Devices) {
        foreach (DeviceItem item in device.DeviceItems) {
          var c = item.GetService<SoftwareContainer>();
          if (c == null || c.Software == null) continue;
          if (c.Software.Name != "HMI_RT_2") continue;
          var scriptsProp = c.Software.GetType().GetProperty("Scripts");
          var scripts = scriptsProp.GetValue(c.Software, null);
          var countProp = scripts.GetType().GetProperty("Count");
          Console.WriteLine("COUNT|" + countProp.GetValue(scripts, null));
          foreach (var module in (System.Collections.IEnumerable)scripts) {
            var t = module.GetType();
            Console.WriteLine("MODULE|" + t.FullName + "|" + t.GetProperty("Name").GetValue(module, null));
            foreach (var p in t.GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(x => x.Name)) {
              Console.WriteLine("PROP|" + p.PropertyType.FullName + "|" + p.Name);
            }
            foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).OrderBy(x => x.Name)) {
              var ps = string.Join(", ", m.GetParameters().Select(pp => pp.ParameterType.FullName + " " + pp.Name));
              Console.WriteLine("METHOD|" + m.ReturnType.FullName + "|" + m.Name + "|" + ps);
            }
          }
          return 0;
        }
      }
    }
    return 1;
  }
}
