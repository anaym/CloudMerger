using System;
using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using FluentAssertions;
using NUnit.Framework;

namespace YandexFileHosting.Test
{
    public class HostingTestBase
    {
        protected const string Token = "AQAEA7qiAp8AAAQv83T09kxsZEXmiSIla-bv4ho";
        protected IHostingManager HostingManager = new YandexHostingManager();
        protected IHosting Hosting;
        protected string interfaceName = "Local Area Connection";

        [SetUp]
        public virtual void SetUp()
        {
            Hosting = HostingManager.GetFileHostingFor(new OAuthCredentials { Token = Token, Service = HostingManager.Name });
        }

        protected void AssertThrows<T>(Task task)
            where T : Exception
        {
            try
            {
                task.Wait();
            }
            catch (Exception)
            { }
            if (task.Exception == null)
                Assert.Fail($"Expected exception {typeof(T)}");
            task.Exception.InnerExceptions.First().GetType()
                .ShouldBeEquivalentTo(typeof(T), because:task.Exception.InnerExceptions.First().Message);
        }

        protected void EnableInternet()
        {
            System.Diagnostics.ProcessStartInfo psi =
                   new System.Diagnostics.ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" enable");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }

        protected void DisableInternet()
        {
            Assert.Ignore("Please, manually disable internet");
            System.Diagnostics.ProcessStartInfo psi =
                new System.Diagnostics.ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" disable");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }

        [TearDown]
        public void TearDown()
        {
            EnableInternet();
        }
    }
}