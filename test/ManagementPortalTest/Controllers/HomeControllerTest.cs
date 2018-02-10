using ManagementPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;
using System;

namespace ManagementPortalTest.Controllers
{
    public class HomeControllerTest
    {
        [Fact]
        public void GetRequest_NotAuthorized()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = false,
                Pin = string.Empty,
                Output = "",
                RemoteConsoleUrl = "",
            };
            var controller = new HomeControllerBuilder().Build();

            var result = (ViewResult)controller.Index(null, null);

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void WrongPin_NotAuthorized()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = false,
                Pin = "wrong pin",
                Output = "",
                RemoteConsoleUrl = "",
            };
            var controller = new HomeControllerBuilder().Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, null);

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void UnknownOperation_DoNoting()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = true,
                Pin = HomeControllerBuilder.CorrectPin,
                Output = "",
                RemoteConsoleUrl = "",
            };
            var controller = new HomeControllerBuilder().Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "unknown operation");

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void WrongPinAndStartOperation_NotAuthorizedAndNoOperationWasTaken()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = false,
                Pin = "wrong pin",
                Output = "",
                RemoteConsoleUrl = "",
            };
            var controller = new HomeControllerBuilder()
                .SetRemoteConsoleStartMethod((_) => throw new Exception())
                .Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "start");

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void SucceedToStart_ReturnCorrectUrl()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = true,
                Pin = HomeControllerBuilder.CorrectPin,
                Output = "succeeded",
                RemoteConsoleUrl = "url",
            };
            var controller = new HomeControllerBuilder()
                .SetRemoteConsoleStartMethod((remoteConsole) =>
                {
                    remoteConsole.IsRunning = true;
                    remoteConsole.Url = expectedModel.RemoteConsoleUrl;
                    return (true, expectedModel.Output);
                })
                .Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "start");

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void StartAgainWhenItIsAlreadyRunning_NoEffect()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = true,
                Pin = HomeControllerBuilder.CorrectPin,
                Output = "RemoteConsole is already running.",
                RemoteConsoleUrl = "url",
            };
            var controller = new HomeControllerBuilder()
                .SetRemoteConsoleIsRunning(true)
                .SetRemoteConsoleUrl(expectedModel.RemoteConsoleUrl)
                .Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "start");

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void FailedToStart_ReturnWithoutUrl()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = true,
                Pin = HomeControllerBuilder.CorrectPin,
                Output = "failed",
                RemoteConsoleUrl = string.Empty,
            };
            var controller = new HomeControllerBuilder()
                .SetRemoteConsoleStartMethod((remoteConsole) => (false, expectedModel.Output))
                .SetRemoteConsoleUrl("should not be returned")
                .Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "start");

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void FailedToStartWithException_ReturnWithoutUrl()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = true,
                Pin = HomeControllerBuilder.CorrectPin,
                Output = string.Empty,
                RemoteConsoleUrl = string.Empty,
            };
            var controller = new HomeControllerBuilder()
                .SetRemoteConsoleStartMethod((remoteConsole) => throw new Exception())
                .SetRemoteConsoleUrl("should not be returned")
                .Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "start");

            var actualModel = (HomePageViewModel)result.Model;
            actualModel.Authorized.Should().BeTrue();
            actualModel.Pin.Should().Be(HomeControllerBuilder.CorrectPin);
            actualModel.Output.Should().StartWith("Failed to start RemoteConsole. Exception: ");
            actualModel.RemoteConsoleUrl.Should().BeEmpty();
        }

        [Fact]
        public void SucceedToStop_ReturnEmptyUrl()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = true,
                Pin = HomeControllerBuilder.CorrectPin,
                Output = "succeeded",
                RemoteConsoleUrl = string.Empty,
            };
            var controller = new HomeControllerBuilder()
                .SetRemoteConsoleIsRunning(true)
                .SetRemoteConsoleUrl("url")
                .SetRemoteConsoleStopMethod((remoteConsole) =>
                {
                    remoteConsole.IsRunning = false;
                    return (true, expectedModel.Output);
                })
                .Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "stop");

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void StopAgainWhenItIsNotRunning_NoEffect()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = true,
                Pin = HomeControllerBuilder.CorrectPin,
                Output = "RemoteConsole was not running.",
                RemoteConsoleUrl = string.Empty,
            };
            var controller = new HomeControllerBuilder()
                .SetRemoteConsoleIsRunning(false)
                .Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "stop");

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void FailedToStop_ReturnWithoutUrl()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = true,
                Pin = HomeControllerBuilder.CorrectPin,
                Output = "failed",
                RemoteConsoleUrl = "url",
            };
            var controller = new HomeControllerBuilder()
                .SetRemoteConsoleIsRunning(true)
                .SetRemoteConsoleUrl(expectedModel.RemoteConsoleUrl)
                .SetRemoteConsoleStopMethod((remoteConsole) => (false, expectedModel.Output))
                .Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "stop");

            result.Model.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void FailedToStopWithException_ReturnWithoutUrl()
        {
            var expectedModel = new HomePageViewModel()
            {
                Authorized = true,
                Pin = HomeControllerBuilder.CorrectPin,
                Output = string.Empty,
                RemoteConsoleUrl = "url",
            };
            var controller = new HomeControllerBuilder()
                .SetRemoteConsoleIsRunning(true)
                .SetRemoteConsoleUrl(expectedModel.RemoteConsoleUrl)
                .SetRemoteConsoleStopMethod((remoteConsole) => throw new Exception())
                .Build();

            var result = (ViewResult)controller.Index(expectedModel.Pin, "stop");

            var actualModel = (HomePageViewModel)result.Model;
            actualModel.Authorized.Should().BeTrue();
            actualModel.Pin.Should().Be(expectedModel.Pin);
            actualModel.Output.Should().StartWith("Failed to stop RemoteConsole. Exception: ");
            actualModel.RemoteConsoleUrl.Should().Be(expectedModel.RemoteConsoleUrl);
        }

    }
}
