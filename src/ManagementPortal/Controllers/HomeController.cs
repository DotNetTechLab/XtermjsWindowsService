using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ManagementPortal.Models;
using ManagementPortal.Services;
using System.Text;

namespace ManagementPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRemoteConsoleManager _remoteConsole;

        public HomeController(IRemoteConsoleManager remoteConsole)
        {
            _remoteConsole = remoteConsole;
        }

        public IActionResult Index([FromForm]string pin, [FromForm]string operation)
        {
            pin = pin ?? string.Empty;
            operation = operation ?? string.Empty;

            var model = new HomePageViewModel()
            {
                Pin = pin,
            };

            model.Authorized = Authorize(model.Pin);
            if (!model.Authorized)
            {
                model.RemoteConsoleUrl = string.Empty;
                model.Output = string.Empty;
                return View(model);
            }

            switch(operation)
            {
                case "start":
                    if (_remoteConsole.IsRunning)
                    {
                        model.Output = "RemoteConsole is already running.";
                    }
                    else
                    {
                        try
                        {
                            (var result, var output) = _remoteConsole.Start();
                            model.Output = output;
                        }
                        catch(Exception ex)
                        {
                            model.Output = "Failed to start RemoteConsole. Exception: " + ex.ToString();
                        }
                    }
                    break;
                case "stop":
                    if (!_remoteConsole.IsRunning)
                    {
                        model.Output = "RemoteConsole was not running.";
                    }
                    else
                    {
                        try
                        {
                            (var result, var output) = _remoteConsole.Stop();
                            model.Output = output;
                        }
                        catch(Exception ex)
                        {
                            model.Output = "Failed to stop RemoteConsole. Exception: " + ex.ToString();
                        }
                    }
                    break;
            }

            model.RemoteConsoleUrl = _remoteConsole.IsRunning ? _remoteConsole.Url : string.Empty;
            model.Output = model.Output ?? string.Empty;

            return View(model);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private bool Authorize(string pin)
        {
            return pin == "blablabla";
        }
    }
}
