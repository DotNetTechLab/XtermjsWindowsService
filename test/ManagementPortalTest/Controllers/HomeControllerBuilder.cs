using ManagementPortal.Controllers;
using ManagementPortal.Models;
using ManagementPortal.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ManagementPortalTest.Controllers
{
    public class HomeControllerBuilder
    {
        public class RemoteConsoleManagerMock : IRemoteConsoleManager
        {
            public Func<RemoteConsoleManagerMock, (bool, string)> StartDelegate { get; set; } = (_) => (true, "started");
            public Func<RemoteConsoleManagerMock, (bool, string)> StopDelegate { get; set; } = (_) => (true, "stopped");

            public string Url { get; set; } = string.Empty;

            public bool IsRunning { get; set; }

            public (bool result, string output) Start()
            {
                return StartDelegate(this);
            }

            public (bool result, string output) Stop()
            {
                return StopDelegate(this);
            }
        }

        private readonly RemoteConsoleManagerMock _remoteConsole = new RemoteConsoleManagerMock();
        public static string CorrectPin => "blablabla";

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

        public HomeControllerBuilder SetRemoteConsoleStartMethod(Func<RemoteConsoleManagerMock, (bool, string)> startMethod)
        {
            _remoteConsole.StartDelegate = startMethod;
            return this;
        }

        public HomeControllerBuilder SetRemoteConsoleStopMethod(Func<RemoteConsoleManagerMock, (bool, string)> stopMethod)
        {
            _remoteConsole.StopDelegate = stopMethod;
            return this;
        }

        public HomeController Build()
        {
            AuthorizationSettings authorizationSettings;
            const string salt = "f0rtunE";
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(salt + CorrectPin + salt));
                var memorySettings = new Dictionary<string, string>()
                {
                    ["PinHash"] = new Guid(hash).ToString("N")
                };
                var configuration = new ConfigurationBuilder()
                                        .AddInMemoryCollection(memorySettings)
                                        .Build();
                authorizationSettings = new AuthorizationSettings(configuration);
            }

            return new HomeController(authorizationSettings, _remoteConsole);
        }
    }
}
