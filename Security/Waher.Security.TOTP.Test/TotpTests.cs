using System.Text;
using Waher.Content.Xml;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Counters;
using Waher.Runtime.Inventory;
using Waher.Security.LoginMonitor;

namespace Waher.Security.TOTP.Test
{
	[TestClass]
	public sealed class TotpTests
	{
		private const string remoteEndpoint = "TestEP";
		private const string protocol = "TOTP";
		private static FilesProvider? filesProvider;
		private static LoginAuditor? auditor;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(RuntimeCounters).Assembly,
				typeof(LoginAuditor).Assembly);

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);

			auditor = new LoginAuditor("Login Auditor",
				new LoginInterval(5, TimeSpan.FromHours(1)),    // Maximum 5 failed login attempts in an hour
				new LoginInterval(2, TimeSpan.FromDays(1)),     // Maximum 2x5 failed login attempts in a day
				new LoginInterval(2, TimeSpan.FromDays(7)),     // Maximum 2x2x5 failed login attempts in a week
				new LoginInterval(2, TimeSpan.MaxValue));       // Maximum 2x2x2x5 failed login attempts in total, then blocked.
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			if (filesProvider is not null)
			{
				await filesProvider.DisposeAsync();
				filesProvider = null;
			}
		}

		internal static LoginAuditor? Auditor => auditor;

		[TestMethod]
		[DataRow(HashFunction.SHA1, 8, 30, "1970-01-01T00:00:59Z", "12345678901234567890", 94287082)]
		[DataRow(HashFunction.SHA256, 8, 30, "1970-01-01T00:00:59Z", "12345678901234567890123456789012", 46119246)]
		[DataRow(HashFunction.SHA512, 8, 30, "1970-01-01T00:00:59Z", "1234567890123456789012345678901234567890123456789012345678901234", 90693936)]
		[DataRow(HashFunction.SHA1, 8, 30, "2005-03-18T01:58:29Z", "12345678901234567890", 07081804)]
		[DataRow(HashFunction.SHA256, 8, 30, "2005-03-18T01:58:29Z", "12345678901234567890123456789012", 68084774)]
		[DataRow(HashFunction.SHA512, 8, 30, "2005-03-18T01:58:29Z", "1234567890123456789012345678901234567890123456789012345678901234", 25091201)]
		[DataRow(HashFunction.SHA1, 8, 30, "2005-03-18T01:58:31Z", "12345678901234567890", 14050471)]
		[DataRow(HashFunction.SHA256, 8, 30, "2005-03-18T01:58:31Z", "12345678901234567890123456789012", 67062674)]
		[DataRow(HashFunction.SHA512, 8, 30, "2005-03-18T01:58:31Z", "1234567890123456789012345678901234567890123456789012345678901234", 99943326)]
		[DataRow(HashFunction.SHA1, 8, 30, "2009-02-13T23:31:30Z", "12345678901234567890", 89005924)]
		[DataRow(HashFunction.SHA256, 8, 30, "2009-02-13T23:31:30Z", "12345678901234567890123456789012", 91819424)]
		[DataRow(HashFunction.SHA512, 8, 30, "2009-02-13T23:31:30Z", "1234567890123456789012345678901234567890123456789012345678901234", 93441116)]
		[DataRow(HashFunction.SHA1, 8, 30, "2033-05-18T03:33:20Z", "12345678901234567890", 69279037)]
		[DataRow(HashFunction.SHA256, 8, 30, "2033-05-18T03:33:20Z", "12345678901234567890123456789012", 90698825)]
		[DataRow(HashFunction.SHA512, 8, 30, "2033-05-18T03:33:20Z", "1234567890123456789012345678901234567890123456789012345678901234", 38618901)]
		[DataRow(HashFunction.SHA1, 8, 30, "2603-10-11T11:33:20Z", "12345678901234567890", 65353130)]
		[DataRow(HashFunction.SHA256, 8, 30, "2603-10-11T11:33:20Z", "12345678901234567890123456789012", 77737706)]
		[DataRow(HashFunction.SHA512, 8, 30, "2603-10-11T11:33:20Z", "1234567890123456789012345678901234567890123456789012345678901234", 47863826)]
		public void Test_01_Calculate(HashFunction HashFunction, int NrDigits, int TimeStepSeconds,
			string Timestamp, string Secret, int Password)
		{
			DateTime ParsedTimestamp = XML.ParseDateTime(Timestamp);
			TotpCalculator Hotp = new(NrDigits, Encoding.ASCII.GetBytes(Secret), HashFunction, TimeStepSeconds);
			int ComputedPassword = Hotp.Compute(ParsedTimestamp);

			Assert.AreEqual(Password, ComputedPassword);
		}
	}
}
