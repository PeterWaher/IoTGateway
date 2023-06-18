using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;

namespace Waher.Networking.Modbus.Test
{
	[TestClass]
	public class ExternalTests
	{
		internal static ConsoleOutSniffer sniffer = null;
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

			sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal, LineEnding.NewLine);
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

			sniffer?.Dispose();
			sniffer = null;
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
			using ModbusTcpClient Client = await ModbusTcpClient.Connect(host, port, sniffer);
			Assert.IsTrue(Client.Connected);
		}

		[TestMethod]
		public async Task Test_02_ReadRegisters()
		{
			using ModbusTcpClient Client = await ModbusTcpClient.Connect(host, port, sniffer);
			Assert.IsTrue(Client.Connected);

			ushort[] Words = await Client.ReadMultipleRegisters(1, 0, 16);
			int i = 0;

			foreach (ushort Word in Words)
				Console.Out.WriteLine((i++).ToString("X2") + ": " + Word.ToString("X4"));
		}

		[TestMethod]
		public async Task Test_03_ReadCoils()
		{
			using ModbusTcpClient Client = await ModbusTcpClient.Connect(host, port, sniffer);
			Assert.IsTrue(Client.Connected);

			BitArray Coils = await Client.ReadCoils(1, 0, 100);
			int i = 0;
			int c = Coils.Length;

			for (i = 0; i < c; i++)
				Console.Out.WriteLine(i.ToString() + ": " + Coils[i].ToString());
		}

		[TestMethod]
		public async Task Test_04_ReadInputDiscretes()
		{
			using ModbusTcpClient Client = await ModbusTcpClient.Connect(host, port, sniffer);
			Assert.IsTrue(Client.Connected);

			BitArray Coils = await Client.ReadInputDiscretes(1, 0, 100);
			int i = 0;
			int c = Coils.Length;

			for (i = 0; i < c; i++)
				Console.Out.WriteLine(i.ToString() + ": " + Coils[i].ToString());
		}

		[TestMethod]
		public async Task Test_05_ReadInputRegisters()
		{
			using ModbusTcpClient Client = await ModbusTcpClient.Connect(host, port, sniffer);
			Assert.IsTrue(Client.Connected);

			ushort[] Words = await Client.ReadInputRegisters(1, 0, 16);
			int i = 0;

			foreach (ushort Word in Words)
				Console.Out.WriteLine((i++).ToString("X2") + ": " + Word.ToString("X4"));
		}

	}
}
