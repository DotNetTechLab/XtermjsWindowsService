using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
        private const string FirewallRuleName = "Management Portal - Remote Console Port";
        private readonly ILogger<RemoteConsoleManager> _logger;
        private readonly IFirewallAgent _firewallAgent;
        private readonly IPlatform _platform;

        public string Url { get; private set; }
        public bool IsRunning { get; private set; }

        public RemoteConsoleManager(ILogger<RemoteConsoleManager> logger, IFirewallAgent firewallAgent, IPlatform platform)
        {
            _logger = logger;
            _firewallAgent = firewallAgent;
            _platform = platform;
        }

        public (bool result, string output) Start()
        {
            var port = GetNextPort();

            _firewallAgent.AddRuleForPort(FirewallRuleName, port);

            var exitCode = _platform.ShellExecute(
                false,
                "run.bat",
                port.ToString(),
                "C:\\GitHubRepos\\DotNetTechLab\\XtermjsWindowsService\\Publish\\xtermjs-binary"
                );

            Url = $"http://localhost:{port}";
            IsRunning = true;
            return (true, "started");
        }

        public (bool result, string output) Stop()
        {
            _firewallAgent.DeleteRule(FirewallRuleName);

            var killedProcessCount = _platform.KillProcess("node", null);

            IsRunning = false;
            return (true, $"stopped {killedProcessCount} process(es).");
        }

        private static int GetNextPort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }
    }
}
