using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Runtime.Settings.Test
{
    [TestClass]
    public class HostSettingsTests
    {
        [TestMethod]
        public async Task Test_01_String()
        {
            await HostSettings.SetAsync("UnitTest", "Test.String", "Hello");
            Assert.AreEqual("Hello", await HostSettings.GetAsync("UnitTest", "Test.String", string.Empty));
            Assert.AreEqual("Hello", await HostSettings.GetAsync("UnitTest", "Test.String", (object)null));
        }

        [TestMethod]
        public async Task Test_02_Long()
        {
            await HostSettings.SetAsync("UnitTest", "Test.Int64", 123L);
            Assert.AreEqual(123L, await HostSettings.GetAsync("UnitTest", "Test.Int64", 0L));
            Assert.AreEqual(123L, await HostSettings.GetAsync("UnitTest", "Test.Int64", (object)null));
        }

        [TestMethod]
        public async Task Test_03_Boolean()
        {
            await HostSettings.SetAsync("UnitTest", "Test.Bool", true);
            Assert.AreEqual(true, await HostSettings.GetAsync("UnitTest", "Test.Bool", false));
            Assert.AreEqual(true, await HostSettings.GetAsync("UnitTest", "Test.Bool", (object)null));
        }

        [TestMethod]
        public async Task Test_04_DateTime()
        {
            DateTime TP = DateTime.Now;
            await HostSettings.SetAsync("UnitTest", "Test.DateTime", TP);
            Assert.AreEqual(TP, await HostSettings.GetAsync("UnitTest", "Test.DateTime", DateTime.MinValue));
            Assert.AreEqual(TP, await HostSettings.GetAsync("UnitTest", "Test.DateTime", (object)null));
        }

        [TestMethod]
        public async Task Test_05_TimeSpan()
        {
            TimeSpan TS = DateTime.Now.TimeOfDay;
            await HostSettings.SetAsync("UnitTest", "Test.TimeSpan", TS);
            Assert.AreEqual(TS, await HostSettings.GetAsync("UnitTest", "Test.TimeSpan", TimeSpan.Zero));
            Assert.AreEqual(TS, await HostSettings.GetAsync("UnitTest", "Test.TimeSpan", (object)null));
        }

        [TestMethod]
        public async Task Test_06_Double()
        {
            await HostSettings.SetAsync("UnitTest", "Test.Double", 3.1415927);
            Assert.AreEqual(3.1415927, await HostSettings.GetAsync("UnitTest", "Test.Double", 0.0));
            Assert.AreEqual(3.1415927, await HostSettings.GetAsync("UnitTest", "Test.Double", (object)null));
        }

        [TestMethod]
        public async Task Test_07_Enum()
        {
            await HostSettings.SetAsync("UnitTest", "Test.Enum", TypeCode.SByte);
            Assert.AreEqual(TypeCode.SByte, await HostSettings.GetAsync("UnitTest", "Test.Enum", default(TypeCode)));
            Assert.AreEqual(TypeCode.SByte, await HostSettings.GetAsync("UnitTest", "Test.Enum", (object)null));
        }

        [TestMethod]
        public async Task Test_08_Object()
        {
            TestObject Obj = new()
            {
                S = "Hello",
                D = 3.1415927,
                I = 123
            };

            await HostSettings.SetAsync("UnitTest", "Test.Object", Obj);
            TestObject Obj2 = (await HostSettings.GetAsync("UnitTest", "Test.Object", (object)null)) as TestObject;

            Assert.IsNotNull(Obj2);
            Assert.AreEqual(Obj.S, Obj2.S);
            Assert.AreEqual(Obj.D, Obj2.D);
            Assert.AreEqual(Obj.I, Obj2.I);
        }
    }
}
