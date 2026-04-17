using System;
using System.IO;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.Compiler;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            int pid = args.Length > 0 ? int.Parse(args[0]) : 20460;
            string sourcePath = args.Length > 1 ? args[1] : @"C:\Users\Administrator\Documents\Codex\2026-04-18-1215c-plc-ktp1200-unified-comfort-vb\1215C_HMI_ClosedLoop_Full.scl";
            string plcName = args.Length > 2 ? args[2] : "PLC_1";
            string extName = "Codex_ClosedLoop_Inject";

            var process = TiaPortal.GetProcess(pid, 10000);
            using (var portal = process.Attach())
            using (var access = portal.ExclusiveAccess("Codex is injecting SCL closed-loop logic into PLC_1"))
            {
                Project project = portal.Projects[0];
                PlcSoftware plc = FindPlcSoftware(project, plcName);
                if (plc == null) throw new Exception("PLC software not found: " + plcName);

                PlcExternalSource existing = plc.ExternalSourceGroup.ExternalSources.Find(extName);
                if (existing != null) existing.Delete();

                PlcExternalSource external = plc.ExternalSourceGroup.ExternalSources.CreateFromFile(extName, sourcePath);
                external.GenerateBlocksFromSource(GenerateBlockOption.KeepOnError);
                Console.WriteLine("GENERATED|" + extName);

                ICompilable compilable = plc.GetService<ICompilable>();
                if (compilable != null)
                {
                    CompilerResult result = compilable.Compile();
                    Console.WriteLine("COMPILE_STATE|" + result.State);
                    foreach (CompilerResultMessage message in result.Messages)
                    {
                        Console.WriteLine("MSG|" + message.State + "|" + message.Path + "|" + message.Description);
                    }
                }
                else
                {
                    Console.WriteLine("COMPILE_SERVICE_NOT_FOUND");
                }
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

    static PlcSoftware FindPlcSoftware(Project project, string softwareName)
    {
        foreach (Device device in project.Devices)
        {
            foreach (DeviceItem item in device.DeviceItems)
            {
                PlcSoftware found = FindPlcSoftwareRecursive(item, softwareName);
                if (found != null) return found;
            }
        }
        return null;
    }

    static PlcSoftware FindPlcSoftwareRecursive(DeviceItem item, string softwareName)
    {
        SoftwareContainer container = null;
        try { container = item.GetService<SoftwareContainer>(); } catch { }
        if (container != null && container.Software is PlcSoftware && container.Software.Name == softwareName)
        {
            return (PlcSoftware)container.Software;
        }
        foreach (DeviceItem child in item.DeviceItems)
        {
            PlcSoftware found = FindPlcSoftwareRecursive(child, softwareName);
            if (found != null) return found;
        }
        return null;
    }
}
