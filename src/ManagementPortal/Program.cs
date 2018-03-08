using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
//using Microsoft.AspNetCore.Hosting.WindowsServices;
//using System.ServiceProcess;
using DasMulli.Win32.ServiceUtils;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ManagementPortal
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var isDebugging = Debugger.IsAttached || args.Contains("--debug");
            if (isDebugging)
            {
                var host = WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .Build();
                host.Run();
            }
            else
            {
                // We should use .RunAsService() in namespace Microsoft.AspNetCore.Hosting.WindowsServices.
                // But current ASPNET Core does not support .net 4.6.1 which is required by package Microsoft.AspNetCore.Hosting.WindowsServices.
                // So we use DasMulli.Win32.ServiceUtils package as a temporary workaround.

                // var exePath= System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                // var directoryPath = Path.GetDirectoryName(exePath);
                // var host = WebHost.CreateDefaultBuilder(args)
                //     .UseStartup<Startup>()
                //     .UseContentRoot(directoryPath)
                //     .Build();
                // host.RunAsService();

                var myService = new MyService();
                var serviceHost = new Win32ServiceHost(myService);
                serviceHost.Run();
            }
        }

        class MyService : IWin32Service
        {
            private IWebHost _host;

            public string ServiceName => "Remote Management Portal";

            public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
            {
                var exePath = Assembly.GetExecutingAssembly().Location;
                var directoryPath = Path.GetDirectoryName(exePath);

                var log = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .WriteTo.File(Path.Combine(directoryPath, "mainlog.txt"), flushToDiskInterval: TimeSpan.FromSeconds(1))
                    .CreateLogger();
                try
                {
                    _host = WebHost.CreateDefaultBuilder(startupArguments)
                        .UseStartup<Startup>()
                        .ConfigureAppConfiguration((context, logging) =>
                        {
                            // When it is started as a service. The current folder is "%windir%\system"
                            logging.AddJsonFile(Path.Combine(directoryPath, "appsettings.json"), true, false);
                            logging.AddJsonFile(Path.Combine(directoryPath, $"appsettings.{context.HostingEnvironment.EnvironmentName}.json"), true, false);
                        })
                        .UseSerilog((context, logger) =>
                        {
                            logger.Enrich.FromLogContext()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                .WriteTo.File(Path.Combine(directoryPath, "log.txt"), flushToDiskInterval: TimeSpan.FromSeconds(1))
                                .WriteTo.Console();
                        })
                        .UseUrls("http://*:15666")
                        .Build();
                    _host.RunAsync();
                }
                catch(Exception ex)
                {
                    log.Fatal(ex, "Failed to run webhost.");
                }
            }

            public void Stop()
            {
                _host.StopAsync().Wait();
            }
        }
    }
}
