using System.Text;

namespace ManagementPortal.Services
{
    public interface IRemoteConsoleManager
    {
        string Url { get; }
        bool IsRunning { get; }
        (bool result, string output) Start();
        (bool result, string output) Stop();
    }

    public class RemoteConsoleManager : IRemoteConsoleManager
    {
        public string Url { get; private set; }
        public bool IsRunning { get; private set; }

        public (bool result, string output) Start()
        {
            Url = "http://localhost:6000";
            IsRunning = true;
            return (true, "started");
        }

        public (bool result, string output) Stop()
        {
            IsRunning = false;
            return (true, "stopped");
        }
    }
}
