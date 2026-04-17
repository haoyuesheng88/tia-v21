using System;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.Hmi.Communication;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiConnections;
using Siemens.Engineering.HmiUnified.HmiTags;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            int pid = args.Length > 0 ? int.Parse(args[0]) : 20460;
            var process = TiaPortal.GetProcess(pid, 10000);
            using (var portal = process.Attach())
            {
                var project = portal.Projects.First();
                foreach (Device device in project.Devices)
                {
                    foreach (DeviceItem item in device.DeviceItems)
                    {
                        SoftwareContainer container = null;
                        try { container = item.GetService<SoftwareContainer>(); } catch { }
                        if (container == null || container.Software == null) continue;

                        var classicHmi = container.Software as HmiTarget;
                        if (classicHmi != null)
                        {
                            Console.WriteLine("CLASSIC|" + classicHmi.Name);
                            Console.WriteLine("CLASSIC_CONN_COUNT|" + classicHmi.Connections.Count);
                            foreach (Connection conn in classicHmi.Connections)
                            {
                                Console.WriteLine("CLASSIC_CONN|" + conn.Name);
                                DumpObject(conn, "  ");
                            }

                            Console.WriteLine("CLASSIC_TAGTABLE_COUNT|" + classicHmi.TagFolder.TagTables.Count);
                            foreach (TagTable tt in classicHmi.TagFolder.TagTables)
                            {
                                Console.WriteLine("CLASSIC_TAGTABLE|" + tt.Name + "|" + tt.Tags.Count);
                                foreach (Tag tag in tt.Tags)
                                {
                                    Console.WriteLine("CLASSIC_TAG|" + tag.Name);
                                    DumpObject(tag, "  ");
                                }
                            }
                        }

                        var unifiedHmi = container.Software as HmiSoftware;
                        if (unifiedHmi != null)
                        {
                            Console.WriteLine("UNIFIED|" + unifiedHmi.Name);
                            Console.WriteLine("UNIFIED_CONN_COUNT|" + unifiedHmi.Connections.Count);
                            foreach (HmiConnection conn in unifiedHmi.Connections)
                            {
                                Console.WriteLine("UNIFIED_CONN|" + conn.Name + "|" + conn.CommunicationDriver + "|" + conn.Partner + "|" + conn.Station);
                            }

                            Console.WriteLine("UNIFIED_TAGTABLE_COUNT|" + unifiedHmi.TagTables.Count);
                            foreach (HmiTagTable tt in unifiedHmi.TagTables)
                            {
                                Console.WriteLine("UNIFIED_TAGTABLE|" + tt.Name + "|" + tt.Tags.Count);
                                foreach (HmiTag tag in tt.Tags)
                                {
                                    Console.WriteLine("UNIFIED_TAG|" + tag.Name + "|" + tag.Connection + "|" + tag.DataType + "|" + tag.Address);
                                }
                            }
                        }
                    }
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

    static void DumpObject(IEngineeringObject obj, string indent)
    {
        foreach (var info in obj.GetAttributeInfos())
        {
            try
            {
                var value = obj.GetAttribute(info.Name);
                Console.WriteLine(indent + "ATTR|" + info.Name + "|" + value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(indent + "ATTR_ERR|" + info.Name + "|" + ex.GetType().Name);
            }
        }
    }
}
