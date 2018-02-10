using System.Threading.Tasks;
using ManagementPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ManagementPortalTest.Controllers
{
    public class HomeControllerTest
    {
        [Fact]
        public void GetRequest_NotAuthorized()
        {
            var controller = new HomeControllerBuilder().Build();

            var result = (ViewResult)controller.Index(null, null);

            var expectedModel = new HomePageViewModel()
            {
                Authorized = false,
                Pin = string.Empty,
            };
            Assert.Equal<HomePageViewModel>(expectedModel, (HomePageViewModel)result.Model);
        }

        [Fact]
        public void WrongPin_NotAuthorized()
        {
            var controller = new HomeControllerBuilder().Build();

            var result = (ViewResult)controller.Index("wrong pin", null);

            var expectedModel = new HomePageViewModel()
            {
                Authorized = false,
                Pin = string.Empty,
            };
            Assert.Equal<HomePageViewModel>(expectedModel, (HomePageViewModel)result.Model);
        }

    }
}
