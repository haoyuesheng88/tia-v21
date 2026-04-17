using System;
using Siemens.Engineering;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            Console.WriteLine("Probe start");

            int pid;
            if (args.Length > 0 && int.TryParse(args[0], out pid))
            {
                var process = TiaPortal.GetProcess(pid, 10000);
                if (process == null)
                {
                    Console.WriteLine("PROCESS_NOT_FOUND");
                    return 2;
                }

                using (var portal = process.Attach())
                {
                    Console.WriteLine("ATTACH_OK");
                    Console.WriteLine("Projects=" + portal.Projects.Count);
                }
            }
            else
            {
                using (var portal = new TiaPortal(TiaPortalMode.WithoutUserInterface))
                {
                    Console.WriteLine("HEADLESS_OK");
                    Console.WriteLine("Projects=" + portal.Projects.Count);
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
}
