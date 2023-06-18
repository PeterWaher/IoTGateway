using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Threading.Tasks;

namespace Waher.Networking.Modbus.Test
{
	[TestClass]
	public class LocalTests
	{
		private static ModBusTcpServer server;
		private static ModbusTcpClient client;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			server = await ModBusTcpServer.CreateAsync(502, ExternalTests.sniffer);
			client = await ModbusTcpClient.Connect("localhost", 502);
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			client?.Dispose();
			client = null;

			server?.Dispose();
			server = null;
		}

		[TestMethod]
		public async Task Test_01_ReadCoils()
		{
			static Task ReadCoils(object Sender, ReadBitsEventArgs e)
			{
				Assert.AreEqual(1, e.UnitAddress);
				Assert.AreEqual(200, e.NrBits);
				Assert.AreEqual(1000, e.ReferenceNr);

				int i;

				for (i = 0; i < e.NrBits; i++)
					e[e.ReferenceNr + i] = (i & 1) != 0;

				return Task.CompletedTask;
			};

			server.OnReadCoils += ReadCoils;
			try
			{
				BitArray Result = await client.ReadCoils(1, 1000, 200);

				Assert.AreEqual(200, Result.Length);

				int i;

				for (i = 0; i < 200; i++)
					Assert.AreEqual((i & 1) != 0, Result[i]);
			}
			finally
			{
				server.OnReadCoils -= ReadCoils;
			}
		}

		[TestMethod]
		public async Task Test_02_ReadInputDiscretes()
		{
			static Task ReadInputDiscretes(object Sender, ReadBitsEventArgs e)
			{
				Assert.AreEqual(2, e.UnitAddress);
				Assert.AreEqual(100, e.NrBits);
				Assert.AreEqual(2000, e.ReferenceNr);

				int i;

				for (i = 0; i < e.NrBits; i++)
					e[e.ReferenceNr + i] = (i & 1) != 0;

				return Task.CompletedTask;
			};

			server.OnReadInputDiscretes += ReadInputDiscretes;
			try
			{
				BitArray Result = await client.ReadInputDiscretes(2, 2000, 100);

				Assert.AreEqual(104, Result.Length);

				int i;

				for (i = 0; i < 100; i++)
					Assert.AreEqual((i & 1) != 0, Result[i]);

				for (; i < 104; i++)
					Assert.IsFalse(Result[i]);
			}
			finally
			{
				server.OnReadInputDiscretes -= ReadInputDiscretes;
			}
		}

		[TestMethod]
		public async Task Test_03_ReadInputRegisters()
		{
			static Task ReadInputRegisters(object Sender, ReadWordsEventArgs e)
			{
				Assert.AreEqual(3, e.UnitAddress);
				Assert.AreEqual(10, e.NrWords);
				Assert.AreEqual(3000, e.ReferenceNr);

				int i;

				for (i = 0; i < e.NrWords; i++)
					e[e.ReferenceNr + i] = (ushort)(i + 1234);

				return Task.CompletedTask;
			};

			server.OnReadInputRegisters += ReadInputRegisters;
			try
			{
				ushort[] Result = await client.ReadInputRegisters(3, 3000, 10);

				Assert.AreEqual(10, Result.Length);

				int i;

				for (i = 0; i < 10; i++)
					Assert.AreEqual((ushort)(i + 1234), Result[i]);
			}
			finally
			{
				server.OnReadInputRegisters -= ReadInputRegisters;
			}
		}

		[TestMethod]
		public async Task Test_04_ReadMultipleRegisters()
		{
			static Task ReadMultipleRegisters(object Sender, ReadWordsEventArgs e)
			{
				Assert.AreEqual(4, e.UnitAddress);
				Assert.AreEqual(20, e.NrWords);
				Assert.AreEqual(4000, e.ReferenceNr);

				int i;

				for (i = 0; i < e.NrWords; i++)
					e[e.ReferenceNr + i] = (ushort)(i ^ 1234);

				return Task.CompletedTask;
			};

			server.OnReadMultipleRegisters += ReadMultipleRegisters;
			try
			{
				ushort[] Result = await client.ReadMultipleRegisters(4, 4000, 20);

				Assert.AreEqual(20, Result.Length);

				int i;

				for (i = 0; i < 20; i++)
					Assert.AreEqual((ushort)(i ^ 1234), Result[i]);
			}
			finally
			{
				server.OnReadMultipleRegisters -= ReadMultipleRegisters;
			}
		}

		[TestMethod]
		public async Task Test_05_WriteCoil()
		{
			static Task WriteCoil(object Sender, WriteBitEventArgs e)
			{
				Assert.AreEqual(5, e.UnitAddress);
				Assert.AreEqual(5000, e.ReferenceNr);
				Assert.IsTrue(e.Value);

				return Task.CompletedTask;
			};

			server.OnWriteCoil += WriteCoil;
			try
			{
				bool Result = await client.WriteCoil(5, 5000, true);

				Assert.IsTrue(Result);
			}
			finally
			{
				server.OnWriteCoil -= WriteCoil;
			}
		}

		[TestMethod]
		public async Task Test_06_WriteCoil_2()
		{
			static Task WriteCoil(object Sender, WriteBitEventArgs e)
			{
				Assert.AreEqual(5, e.UnitAddress);
				Assert.AreEqual(5000, e.ReferenceNr);
				Assert.IsTrue(e.Value);

				e.Value = false;

				return Task.CompletedTask;
			};

			server.OnWriteCoil += WriteCoil;
			try
			{
				bool Result = await client.WriteCoil(5, 5000, true);

				Assert.IsFalse(Result);
			}
			finally
			{
				server.OnWriteCoil -= WriteCoil;
			}
		}

		[TestMethod]
		public async Task Test_07_WriteRegister()
		{
			static Task WriteRegister(object Sender, WriteWordEventArgs e)
			{
				Assert.AreEqual(6, e.UnitAddress);
				Assert.AreEqual(6000, e.ReferenceNr);
				Assert.AreEqual((ushort)12345, e.Value);

				return Task.CompletedTask;
			};

			server.OnWriteRegister += WriteRegister;
			try
			{
				ushort Result = await client.WriteRegister(6, 6000, 12345);

				Assert.AreEqual((ushort)12345, Result);
			}
			finally
			{
				server.OnWriteRegister -= WriteRegister;
			}
		}

		[TestMethod]
		public async Task Test_08_WriteRegister_2()
		{
			static Task WriteRegister(object Sender, WriteWordEventArgs e)
			{
				Assert.AreEqual(6, e.UnitAddress);
				Assert.AreEqual(6000, e.ReferenceNr);
				Assert.AreEqual((ushort)12345, e.Value);

				e.Value = 23456;

				return Task.CompletedTask;
			};

			server.OnWriteRegister += WriteRegister;
			try
			{
				ushort Result = await client.WriteRegister(6, 6000, 12345);

				Assert.AreEqual((ushort)23456, Result);
			}
			finally
			{
				server.OnWriteRegister -= WriteRegister;
			}
		}

		[TestMethod]
		public async Task Test_09_WriteMultipleRegisters()
		{
			TaskCompletionSource<bool>[] Registers = new TaskCompletionSource<bool>[]
			{
				new TaskCompletionSource<bool>(),
				new TaskCompletionSource<bool>(),
				new TaskCompletionSource<bool>(),
				new TaskCompletionSource<bool>()
			};

			Task WriteRegister(object Sender, WriteWordEventArgs e)
			{
				Assert.AreEqual(7, e.UnitAddress);
				
				switch (e.ReferenceNr)
				{
					case 7000:
						Registers[0].TrySetResult(e.Value == 123);
						break;

					case 7001:
						Registers[1].TrySetResult(e.Value == 234);
						break;

					case 7002:
						Registers[2].TrySetResult(e.Value == 345);
						break;

					case 7003:
						Registers[3].TrySetResult(e.Value == 456);
						break;

					default:
						Assert.Fail();
						break;
				}

				return Task.CompletedTask;
			};

			server.OnWriteRegister += WriteRegister;
			try
			{
				await client.WriteMultipleRegisters(7, 7000, 123, 234, 345, 456);

				Assert.IsTrue(await Registers[0].Task);
				Assert.IsTrue(await Registers[1].Task);
				Assert.IsTrue(await Registers[2].Task);
				Assert.IsTrue(await Registers[3].Task);
			}
			finally
			{
				server.OnWriteRegister -= WriteRegister;
			}
		}

	}
}
