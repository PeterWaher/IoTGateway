using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Security.LoginMonitor.Test
{
	[TestClass]
	public class LoginAuditorTests
	{
		private const string remoteEndpoint = "TestEP";
		private const string protocol = "Test";
		private static readonly TimeSpan Zero = TimeSpan.Zero;
		private static readonly TimeSpan OneH = TimeSpan.FromHours(1);
		private static readonly TimeSpan _23H = TimeSpan.FromHours(23);
		private static readonly TimeSpan _5D23H = new TimeSpan(5, 23, 0, 0, 0);
		private static FilesProvider filesProvider = null;
		private static LoginAuditor auditor = null;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(LoginAuditor).Assembly);

			filesProvider = new FilesProvider("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);

			auditor = new LoginAuditor("Login Auditor",
				new LoginInterval(5, TimeSpan.FromHours(1)),    // Maximum 5 failed login attempts in an hour
				new LoginInterval(2, TimeSpan.FromDays(1)),     // Maximum 2x5 failed login attempts in a day
				new LoginInterval(2, TimeSpan.FromDays(7)),     // Maximum 2x2x5 failed login attempts in a week
				new LoginInterval(2, TimeSpan.MaxValue));       // Maximum 2x2x2x5 failed login attempts in total, then blocked.
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			filesProvider?.Dispose();
			filesProvider = null;
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			await auditor.UnblockAndReset(remoteEndpoint, protocol);
			DateTime? TP = await auditor.GetEarliestLoginOpportunity(remoteEndpoint, protocol);
			Assert.IsNull(TP);
		}

		[TestMethod]
		public async Task Test_01_Fail1()
		{
			await this.TestFails(Zero);
		}

		[TestMethod]
		public async Task Test_02_Fail2()
		{
			await this.TestFails(Zero, Zero);
		}

		[TestMethod]
		public async Task Test_03_Fail5()
		{
			await this.TestFails(Zero, Zero, Zero, Zero, Zero);
		}

		[TestMethod]
		public async Task Test_04_Fail6()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH);
		}

		[TestMethod]
		public async Task Test_05_Fail7()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero);
		}

		[TestMethod]
		public async Task Test_06_Fail10()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero);
		}

		[TestMethod]
		public async Task Test_07_Fail11()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H);
		}

		[TestMethod]
		public async Task Test_08_Fail12()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero);
		}

		[TestMethod]
		public async Task Test_09_Fail15()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero);
		}

		[TestMethod]
		public async Task Test_10_Fail16()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH);
		}

		[TestMethod]
		public async Task Test_11_Fail17()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero);
		}

		[TestMethod]
		public async Task Test_12_Fail20()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero);
		}

		[TestMethod]
		public async Task Test_13_Fail21()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H);
		}

		[TestMethod]
		public async Task Test_14_Fail22()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero);
		}

		[TestMethod]
		public async Task Test_15_Fail25()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero);
		}

		[TestMethod]
		public async Task Test_16_Fail26()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH);
		}

		[TestMethod]
		public async Task Test_17_Fail27()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH, Zero);
		}

		[TestMethod]
		public async Task Test_18_Fail30()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero);
		}

		[TestMethod]
		public async Task Test_19_Fail31()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H);
		}

		[TestMethod]
		public async Task Test_20_Fail32()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero);
		}

		[TestMethod]
		public async Task Test_21_Fail35()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero);
		}

		[TestMethod]
		public async Task Test_22_Fail36()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH);
		}

		[TestMethod]
		public async Task Test_23_Fail37()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero);
		}

		[TestMethod]
		public async Task Test_24_Fail40()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public async Task Test_25_Fail41()
		{
			await this.TestFails(
				Zero, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_23H, Zero, Zero, Zero, Zero,
				OneH, Zero, Zero, Zero, Zero,
				_5D23H);
		}

		private async Task TestFails(params TimeSpan[] Spans)
		{
			DateTime TP = new DateTime(2020, 1, 1, 0, 0, 0);
			DateTime TP2;
			int Count = Spans.Length;
			int i;

			for (i = 0; i < Count; i++)
			{
				DateTime? Next = await auditor.GetEarliestLoginOpportunity(remoteEndpoint, protocol);
				if (Next.HasValue)
				{
					if (Next.Value == DateTime.MaxValue)
						throw new Exception("Endpoint has been blocked.");

					TP2 = Next.Value;
				}
				else
					TP2 = TP;

				Assert.AreEqual(Spans[i], TP2 - TP);
				TP = TP2;

				await auditor.ProcessLoginFailure(remoteEndpoint, protocol, TP);
			}
		}

		[TestMethod]
		public async Task Test_26_SparseFails()
		{
			DateTime TP = new DateTime(2020, 1, 1, 0, 0, 0);
			int i;

			for (i = 0; i < 41; i++)
			{
				DateTime? Next = await auditor.GetEarliestLoginOpportunity(remoteEndpoint, protocol);
				if (Next.HasValue)
				{
					if (Next.Value == DateTime.MaxValue)
					{
						Assert.AreEqual(40, i);
						return;
					}
					else
						TP = Next.Value;
				}

				await auditor.ProcessLoginFailure(remoteEndpoint, protocol, TP);
				TP = TP.AddDays(1);
			}

			Assert.Fail("Sparse login attempts not detected.");
		}

	}
}
