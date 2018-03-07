using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementPortal.Services
{
    public interface IFirewallAgent
    {
        void AddRuleForPort(string ruleName, int port);
        void DeleteRule(string ruleName);
    }

    public class WindowsFirewallAgent : IFirewallAgent
    {
        private readonly ILogger<WindowsFirewallAgent> _logger;
        private readonly IPlatform _platform;
        private readonly string _windowsSystem32Path;

        public WindowsFirewallAgent(ILogger<WindowsFirewallAgent> logger, IPlatform platform)
        {
            _logger = logger;
            _platform = platform;
            _windowsSystem32Path = Environment.ExpandEnvironmentVariables(@"%windir%\system32");
        }

        public void AddRuleForPort(string ruleName, int port)
        {
            var (exitCode, output, error) = _platform.Execute(
                true,
                "netsh",
                $"advfirewall firewall add rule name=\"{ruleName}\" protocol=TCP dir=in localport={port} action=allow",
                _windowsSystem32Path);
            _logger.LogInformation("Create Firewall Rule:{exitCode} Output:{output} Error:{error}", exitCode, output, error);
        }

        public void DeleteRule(string ruleName)
        {
            var (exitCode, output, error) = _platform.Execute(
                true,
                "netsh",
                $"advfirewall firewall delete rule name=\"{ruleName}\"",
                _windowsSystem32Path);
            _logger.LogInformation("Delete Firewall Rule:{exitCode} Output:{output} Error:{error}", exitCode, output, error);
        }

    }
}
