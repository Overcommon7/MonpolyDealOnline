using System.Diagnostics;

namespace StartupProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ProcessStartInfo? server = null;
            List<Process?> processes = new();

            const int clientsToCreate = 1;


            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory is not null && !directory.GetFiles("*.sln").Any())
                directory = directory.Parent;

            if (directory is null)
                return;

            string path = directory.FullName;

            void CreateServer()
            {
                server = new();
                server.FileName = Path.Combine(path, "MonopolyDealServer\\bin\\Debug\\net8.0-windows10.0.22000.0\\MonopolyDealServer.exe");
                server.WorkingDirectory = Path.Combine(path, "MonopolyDealServer\\bin\\Debug\\net8.0-windows10.0.22000.0\\");
                server.WindowStyle = ProcessWindowStyle.Normal;
                server.CreateNoWindow = false;

                processes.Add(Process.Start(server));
            }

            void CreateClients()
            {
                for (int i = 0; i < clientsToCreate; i++)
                {
                    Thread.Sleep(500);

                    ProcessStartInfo info = new ProcessStartInfo();

                    info.FileName = Path.Combine(path, "MonopolyDealClient\\bin\\Debug\\net8.0-windows10.0.22000.0\\MonopolyDealClient.exe");
                    info.WorkingDirectory = Path.Combine(path, "WorkingDirForExtraClients\\");
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    info.CreateNoWindow = true;
                    info.ArgumentList.Clear();
                    info.ArgumentList.Add("--Number");
                    info.ArgumentList.Add((i + 8).ToString());
                    info.ArgumentList.Add("--Position");
                    info.ArgumentList.Add("2100");
                    info.ArgumentList.Add("475");

                    processes.Add(Process.Start(info));
                }
            }

            //CreateServer();
            CreateClients();
           
            for (int i = 0; i < processes.Count; i++)
            {
                processes[i]?.WaitForExit();
            }

            for (int i = 0; i < processes.Count; i++)
            {
                processes[i]?.Dispose();
            }
        }
    }
}
