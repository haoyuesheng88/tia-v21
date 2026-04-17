using System;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiTags;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            int pid = int.Parse(args[0]);
            string hmiName = args.Length > 1 ? args[1] : "HMI_RT_2";
            string tagName = args.Length > 2 ? args[2] : "Start_PB";
            string connection = args.Length > 3 ? args[3] : "CodexConn";
            string plcTag = args.Length > 4 ? args[4] : "\"DB_HMI\".Start_PB";
            string address = args.Length > 5 ? args[5] : null;
            string dataType = args.Length > 6 ? args[6] : "Bool";
            if (plcTag == "__NONE__") plcTag = null;
            if (address == "__NONE__") address = null;

            var process = TiaPortal.GetProcess(pid, 10000);
            using (var portal = process.Attach())
            {
                var project = portal.Projects.First();
                var hmi = FindUnified(project, hmiName);
                var table = hmi.TagTables.Find("CodexTags") ?? hmi.TagTables.Create("CodexTags");
                var tag = hmi.Tags.Find(tagName) ?? hmi.Tags.Create(tagName, table.Name);
                tag.Connection = connection;
                tag.DataType = dataType;
                if (!string.IsNullOrEmpty(address))
                {
                    tag.AccessMode = HmiAccessMode.AbsoluteAccess;
                    tag.Address = address;
                }
                if (!string.IsNullOrEmpty(plcTag))
                {
                    tag.AccessMode = HmiAccessMode.SymbolicAccess;
                    tag.PlcTag = plcTag;
                }
                Console.WriteLine("TAG|" + tag.Name + "|" + tag.Connection + "|" + tag.DataType + "|" + tag.HmiDataType + "|" + tag.PlcTag + "|" + tag.Address);
                foreach (var vr in tag.Validate())
                {
                    foreach (var err in vr.Errors) Console.WriteLine("VALID_ERR|" + vr.PropertyName + "|" + err);
                    foreach (var warn in vr.Warnings) Console.WriteLine("VALID_WARN|" + vr.PropertyName + "|" + warn);
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

    static HmiSoftware FindUnified(Project project, string hmiName)
    {
        foreach (Device device in project.Devices)
        {
            foreach (DeviceItem item in device.DeviceItems)
            {
                SoftwareContainer container = null;
                try { container = item.GetService<SoftwareContainer>(); } catch { }
                var hmi = container == null ? null : container.Software as HmiSoftware;
                if (hmi != null && string.Equals(hmi.Name, hmiName, StringComparison.OrdinalIgnoreCase))
                    return hmi;
            }
        }
        return null;
    }
}
