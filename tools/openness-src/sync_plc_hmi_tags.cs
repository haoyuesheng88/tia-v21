using System;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Tags;

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
            using (var access = portal.ExclusiveAccess("Codex syncs PLC HMI tags"))
            {
                var plc = FindPlc(portal.Projects[0], plcName);
                var existing = plc.TagTableGroup.TagTables.Find("Codex_PlcTags");
                if (existing != null) existing.Delete();

                var table = plc.TagTableGroup.TagTables.Create("Codex_PlcTags");
                Create(table, "HMI_Start_PB", "Bool", "%M10.0");
                Create(table, "HMI_Stop_PB", "Bool", "%M10.1");
                Create(table, "HMI_Run_Cmd", "Bool", "%M10.2");
                Create(table, "HMI_Lamp_FB", "Bool", "%M10.3");
                Create(table, "HMI_SetValue", "Int", "%MW20");
                Create(table, "HMI_ActualValue", "Int", "%MW22");

                foreach (PlcTag tag in table.Tags)
                {
                    Console.WriteLine("PLC_TAG|" + tag.Name + "|" + tag.DataTypeName + "|" + tag.LogicalAddress);
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

    static void Create(PlcTagTable table, string name, string dataType, string address)
    {
        var tag = table.Tags.Create(name);
        tag.DataTypeName = dataType;
        tag.LogicalAddress = address;
        tag.ExternalAccessible = true;
        tag.ExternalVisible = true;
        tag.ExternalWritable = true;
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
