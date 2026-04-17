using System;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiConnections;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            int pid = int.Parse(args[0]);
            string hmiName = args.Length > 1 ? args[1] : "HMI_RT_2";
            string connName = args.Length > 2 ? args[2] : "CodexConn";
            string driver = args.Length > 3 ? args[3] : "SIMATIC S7 1200/1500";

            var process = TiaPortal.GetProcess(pid, 10000);
            using (var portal = process.Attach())
            {
                var project = portal.Projects.First();
                var hmi = FindUnified(project, hmiName);
                if (hmi == null)
                {
                    Console.WriteLine("ERROR|HMI_NOT_FOUND|" + hmiName);
                    return 2;
                }

                var conn = hmi.Connections.Find(connName) ?? hmi.Connections.Create(connName);
                conn.CommunicationDriver = driver;

                Console.WriteLine("CONN|" + conn.Name + "|" + conn.CommunicationDriver + "|" + conn.Station + "|" + conn.Partner + "|" + conn.Node + "|" + conn.InitialAddress);
                foreach (var dp in conn.DriverProperties)
                {
                    Console.WriteLine("DRVPROP|" + dp.PropertyName + "|" + dp.Value + "|" + dp.Info);
                }
                foreach (var vr in conn.Validate())
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
