using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.Settings.Test
{
	[TestClass]
	public class SettingsTests
	{
		private static FilesProvider filesProvider = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(RuntimeSettings).Assembly,
				typeof(SettingsTests).Assembly);

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);

			await Database.Clear("Settings");
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			filesProvider?.Dispose();
			filesProvider = null;
		}

		[TestMethod]
		public async Task Test_01_String()
		{
			await RuntimeSettings.SetAsync("Test.String", "Hello");
			Assert.AreEqual("Hello", await RuntimeSettings.GetAsync("Test.String", string.Empty));
			Assert.AreEqual("Hello", await RuntimeSettings.GetAsync("Test.String", (object)null));
		}

		[TestMethod]
		public async Task Test_02_Long()
		{
			await RuntimeSettings.SetAsync("Test.Int64", 123L);
			Assert.AreEqual(123L, await RuntimeSettings.GetAsync("Test.Int64", 0L));
			Assert.AreEqual(123L, await RuntimeSettings.GetAsync("Test.Int64", (object)null));
		}

		[TestMethod]
		public async Task Test_03_Boolean()
		{
			await RuntimeSettings.SetAsync("Test.Bool", true);
			Assert.AreEqual(true, await RuntimeSettings.GetAsync("Test.Bool", false));
			Assert.AreEqual(true, await RuntimeSettings.GetAsync("Test.Bool", (object)null));
		}

		[TestMethod]
		public async Task Test_04_DateTime()
		{
			DateTime TP = DateTime.Now;
			await RuntimeSettings.SetAsync("Test.DateTime", TP);
			Assert.AreEqual(TP, await RuntimeSettings.GetAsync("Test.DateTime", DateTime.MinValue));
			Assert.AreEqual(TP, await RuntimeSettings.GetAsync("Test.DateTime", (object)null));
		}

		[TestMethod]
		public async Task Test_05_TimeSpan()
		{
			TimeSpan TS = DateTime.Now.TimeOfDay;
			await RuntimeSettings.SetAsync("Test.TimeSpan", TS);
			Assert.AreEqual(TS, await RuntimeSettings.GetAsync("Test.TimeSpan", TimeSpan.Zero));
			Assert.AreEqual(TS, await RuntimeSettings.GetAsync("Test.TimeSpan", (object)null));
		}

		[TestMethod]
		public async Task Test_06_Double()
		{
			await RuntimeSettings.SetAsync("Test.Double", 3.1415927);
			Assert.AreEqual(3.1415927, await RuntimeSettings.GetAsync("Test.Double", 0.0));
			Assert.AreEqual(3.1415927, await RuntimeSettings.GetAsync("Test.Double", (object)null));
		}

		[TestMethod]
		public async Task Test_07_Enum()
		{
			await RuntimeSettings.SetAsync("Test.Enum", TypeCode.SByte);
			Assert.AreEqual(TypeCode.SByte, await RuntimeSettings.GetAsync("Test.Enum", default(TypeCode)));
			Assert.AreEqual(TypeCode.SByte, await RuntimeSettings.GetAsync("Test.Enum", (object)null));
		}

		[TestMethod]
		public async Task Test_08_Object()
		{
			TestObject Obj = new TestObject()
			{
				S = "Hello",
				D = 3.1415927,
				I = 123
			};

			await RuntimeSettings.SetAsync("Test.Object", Obj);
			TestObject Obj2 = (await RuntimeSettings.GetAsync("Test.Object", (object)null)) as TestObject;

			Assert.IsNotNull(Obj2);
			Assert.AreEqual(Obj.S, Obj2.S);
			Assert.AreEqual(Obj.D, Obj2.D);
			Assert.AreEqual(Obj.I, Obj2.I);
		}
	}
}
