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

            void CreateServer()
            {
                server = new();
                server.FileName = "H:\\LaSalle\\VSProjects5\\MonopolyDealOnline\\MonopolyDealServer\\bin\\Debug\\net8.0-windows10.0.22000.0\\MonopolyDealServer.exe";
                server.WorkingDirectory = "H:\\LaSalle\\VSProjects5\\MonopolyDealOnline\\MonopolyDealServer\\bin\\Debug\\net8.0-windows10.0.22000.0\\";
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

                    info.FileName = "H:\\LaSalle\\VSProjects5\\MonopolyDealOnline\\MonopolyDealClient\\bin\\Debug\\net8.0-windows10.0.22000.0\\MonopolyDealClient.exe";
                    info.WorkingDirectory = "H:\\LaSalle\\VSProjects5\\MonopolyDealOnline\\MonopolyDealClient\\bin\\Debug\\net8.0-windows10.0.22000.0\\";
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    info.CreateNoWindow = true;
                    info.ArgumentList.Clear();
                    info.ArgumentList.Add((i + 8).ToString());

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
