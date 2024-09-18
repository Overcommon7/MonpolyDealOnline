using System.Diagnostics;

namespace StartupProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ProcessStartInfo server = new();
            List<ProcessStartInfo> clients = new();

            const int clientsToCreate = 2;

            server.FileName = "H:\\LaSalle\\VSProjects5\\MonopolyDealOnline\\MonopolyDealServer\\bin\\Debug\\net8.0-windows10.0.22000.0\\MonopolyDealServer.exe";
            server.WorkingDirectory = "H:\\LaSalle\\VSProjects5\\MonopolyDealOnline\\MonopolyDealServer\\bin\\Debug\\net8.0-windows10.0.22000.0\\";
            server.WindowStyle = ProcessWindowStyle.Normal;
            server.CreateNoWindow = false;

            for (int i = 0; i < clientsToCreate; i++)
            {
                ProcessStartInfo info = new ProcessStartInfo();

                info.FileName = "H:\\LaSalle\\VSProjects5\\MonopolyDealOnline\\MonopolyDealClient\\bin\\Debug\\net8.0-windows10.0.22000.0\\MonopolyDealClient.exe";
                info.WorkingDirectory = "H:\\LaSalle\\VSProjects5\\MonopolyDealOnline\\MonopolyDealClient\\bin\\Debug\\net8.0-windows10.0.22000.0\\";
                info.WindowStyle = ProcessWindowStyle.Normal;
                info.CreateNoWindow = false;
                info.ArgumentList.Clear();
                info.ArgumentList.Add((i + 7).ToString());

                clients.Add(info);
            }

            List<Process?> processes = new();
            processes.Add(Process.Start(server));

            Thread.Sleep(500);

            for (int i = 0; i < clients.Count; i++)
            {
                processes.Add(Process.Start(clients[i]));
                Thread.Sleep(500);
            }

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
