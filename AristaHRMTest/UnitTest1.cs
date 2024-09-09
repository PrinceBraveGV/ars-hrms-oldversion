using System;
using AristaHRM;
using AristaHRM.Controllers;
using AristaHRM.Models;
using AristaHRM.Interfaces;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AristaHRMTest
{
    [TestClass]
    public class UnitTest1
    {
        private AdminController _admin;
        private HomeController _home;
        private InputController _input;
        private MasterController _master;

        [TestInitialize]
        public void Initialize()
        {
            _admin = new AdminController();
            _home = new HomeController();
            _input = new InputController();
            _master = new MasterController();

            // pengendali lainnya

        }

        [TestMethod]
        public void TestHome()
        {
            // Arrange Mode

            // Test Mode
            ViewResult vr = _home.Index() as ViewResult;

            // Assert Mode
            Assert.AreEqual("Index", vr.ViewData["CurrentUser"]);
        }

        [TestMethod]
        public void TestLogin()
        {
            ViewResult vr = _home.Login() as ViewResult;

            Assert.AreEqual("Login", vr.ViewData["CurrentUser"]);
        }

        [TestMethod]
        public void TestLoginWithUser()
        {
            // Arrange Mode
            // bibit nomor acak
            var rand = new Random();
            var point = rand.NextDouble();
            String userid = (point == 1 ? point - 1 * 10000 : point * 10000).ToString("00000");

            string password = "";

            var context = MockHelpers.MockHttpContext();
            var routedata = RouteTable.Routes.GetRouteData(context);

            var model = new LoginModel()
            {
                NIK = userid,
                Password = password
            };

            // Test Mode
            var result = _home.Login(model) as ViewResult;

            // Assert Mode
            Assert.AreEqual("Index", result.ViewName);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsNull(result.ViewData["ErrorMsg"]);

        }

        [TestMethod]
        public void TestPengajuan()
        {
            // Arrange
            // bibit nomor acak
            var rand = new Random();
            var point = rand.NextDouble();
            String res = (point == 1 ? point - 1 * 10000 : point * 10000).ToString("00000");

            String nama = String.Empty;

            var model = new CutiModel()
            {
                NIK = res,

            };

            // Test
            ViewResult vr = _admin.Pengajuan(model) as ViewResult;

            // Assert
            Assert.AreEqual("PengajuanSukses", vr.ViewData["CurrentUser"]);
        }

        [TestMethod]
        public void TestPersetujuanKhusus()
        {

        }

        [TestMethod]
        public void TestPersetujuanTahunan()
        {

        }
    }
}
