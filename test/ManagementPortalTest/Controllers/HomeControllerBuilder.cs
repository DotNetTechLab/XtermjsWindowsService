using ManagementPortal.Controllers;
using ManagementPortal.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManagementPortalTest.Controllers
{
    public class HomeControllerBuilder
    {
        private class RemoteConsoleManagerMock : IRemoteConsoleManager
        {
            public Func<(bool, string)> StartDelegate { get; set; } = () => (true, "started");
            public Func<(bool, string)> StopDelegate { get; set; } = () => (true, "stopped");

            public string Url { get; set; } = string.Empty;

            public bool IsRunning { get; set; }

            public (bool result, string output) Start()
            {
                return StartDelegate();
            }

            public (bool result, string output) Stop()
            {
                return StopDelegate();
            }
        }

        private readonly RemoteConsoleManagerMock _remoteConsole = new RemoteConsoleManagerMock();

        public HomeControllerBuilder SetRemoteConsoleUrl(string url)
        {
            _remoteConsole.Url = url;
            return this;
        }

        public HomeControllerBuilder SetRemoteConsoleIsRunning(bool isRunning)
        {
            _remoteConsole.IsRunning = isRunning;
            return this;
        }

        public HomeControllerBuilder SetRemoteConsoleStartMethod(Func<(bool, string)> startMethod)
        {
            _remoteConsole.StartDelegate = startMethod;
            return this;
        }

        public HomeControllerBuilder SetRemoteConsoleStopMethod(Func<(bool, string)> stopMethod)
        {
            _remoteConsole.StopDelegate = stopMethod;
            return this;
        }

        public HomeController Build()
        {
            return new HomeController(_remoteConsole);
        }
    }
}
