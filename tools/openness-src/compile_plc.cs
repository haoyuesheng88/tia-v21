using System;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.Compiler;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            int pid = int.Parse(args[0]);
            string plcName = args.Length > 1 ? args[1] : "PLC_1";
            var process = TiaPortal.GetProcess(pid, 10000);
            using (var portal = process.Attach())
            using (var access = portal.ExclusiveAccess("Codex compiles PLC"))
            {
                var plc = FindPlc(portal.Projects[0], plcName);
                var compilable = plc.GetService<ICompilable>();
                var result = compilable.Compile();
                Console.WriteLine("COMPILE_STATE|" + result.State);
                foreach (CompilerResultMessage message in result.Messages)
                    Console.WriteLine("MSG|" + message.State + "|" + message.Path + "|" + message.Description);
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR|" + ex.GetType().FullName + "|" + ex.Message);
            Console.WriteLine(ex);
            return 1;
        }
    }

    static PlcSoftware FindPlc(Project project, string softwareName)
    {
        foreach (Device device in project.Devices)
        foreach (DeviceItem item in device.DeviceItems)
        {
            var plc = FindPlcRecursive(item, softwareName);
            if (plc != null) return plc;
        }
        return null;
    }

    static PlcSoftware FindPlcRecursive(DeviceItem item, string softwareName)
    {
        SoftwareContainer container = null;
        try { container = item.GetService<SoftwareContainer>(); } catch { }
        if (container != null && container.Software is PlcSoftware && container.Software.Name == softwareName)
            return (PlcSoftware)container.Software;
        foreach (DeviceItem child in item.DeviceItems)
        {
            var plc = FindPlcRecursive(child, softwareName);
            if (plc != null) return plc;
        }
        return null;
    }
}
