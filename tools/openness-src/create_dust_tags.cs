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
            string plcName = args[1];
            var process = TiaPortal.GetProcess(pid, 10000);
            using (var portal = process.Attach())
            using (var access = portal.ExclusiveAccess("Codex creates dust collector PLC tags"))
            {
                var plc = FindPlc(portal.Projects[0], plcName);
                var existing = plc.TagTableGroup.TagTables.Find("DustCollector_Tags");
                if (existing != null) existing.Delete();

                var table = plc.TagTableGroup.TagTables.Create("DustCollector_Tags");
                Create(table, "DC_Start_Cmd", "Bool", "%I0.0");
                Create(table, "DC_Stop_Cmd", "Bool", "%I0.1");
                Create(table, "DC_DiffPress_High", "Bool", "%I0.2");
                Create(table, "DC_Pulse_Feedback", "Bool", "%I0.3");
                Create(table, "DC_Fan_Run", "Bool", "%Q0.0");
                Create(table, "DC_Valve_1", "Bool", "%Q0.1");
                Create(table, "DC_Valve_2", "Bool", "%Q0.2");
                Create(table, "DC_Valve_3", "Bool", "%Q0.3");
                Create(table, "DC_Valve_4", "Bool", "%Q0.4");
                Create(table, "DC_Alarm", "Bool", "%Q0.5");
                Create(table, "DC_Cycle_Step", "Int", "%MW10");

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
