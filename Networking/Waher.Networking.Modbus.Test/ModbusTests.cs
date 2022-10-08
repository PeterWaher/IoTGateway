using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Console;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;

namespace Waher.Networking.Modbus.Test
{
	[TestClass]
	public class ModbusTests
	{
		private static ConsoleEventSink consoleEventSink = null;
		private static FilesProvider filesProvider = null;
		private static string host;
		private static int port;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(RuntimeSettings).Assembly);

			Log.Register(consoleEventSink = new ConsoleEventSink());

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000);
			Database.Register(filesProvider);
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			filesProvider?.Dispose();
			filesProvider = null;

			if (consoleEventSink != null)
			{
				Log.Unregister(consoleEventSink);
				consoleEventSink = null;
			}
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			//await RuntimeSettings.SetAsync("ModbusGateway.Host", ENTER IP ADDRESS HERE);

			host = await RuntimeSettings.GetAsync("ModbusGateway.Host", string.Empty);
			port = (int)await RuntimeSettings.GetAsync("ModbusGateway.Port", ModbusTcpClient.DefaultPort);

			if (string.IsNullOrEmpty(host))
				throw new Exception("Modbus host not configured. Configure host name, without checking in, using RuntimeSettings.SetAsync");
		}

		[TestMethod]
		public async Task Test_01_Connect()
		{
			using ModbusTcpClient Client = await ModbusTcpClient.Connect(host, port);
			Assert.IsTrue(Client.Connected);
		}
	}
}
