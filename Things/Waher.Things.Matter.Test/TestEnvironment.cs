using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Networking.XMPP.Sensor;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Ip;
using Waher.Things.Metering;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Initializes the test environment: type inventory and an object database, used by the metering topology.
	/// </summary>
	[TestClass]
	public sealed class TestEnvironment
	{
		private static FilesProvider filesProvider = null;

		/// <summary>
		/// Initializes the test environment.
		/// </summary>
		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(InternetContent).Assembly,
				typeof(Translator).Assembly,
				typeof(INode).Assembly,
				typeof(MeteringTopology).Assembly,
				typeof(IpHost).Assembly,
				typeof(InternalReadoutRequest).Assembly,
				typeof(MatterControllerNode).Assembly);

			// On platforms other than Windows, encrypted object databases derive keys from a salt provided
			// in an environment variable. A test salt is used, if none is defined.

			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
				string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FILES_DB_SALT")))
			{
				Environment.SetEnvironmentVariable("FILES_DB_SALT", "Waher.Things.Matter.Test");
			}

			string Folder = Path.Combine(AppContext.BaseDirectory, "Data");

			if (Directory.Exists(Folder))
				Directory.Delete(Folder, true);

			filesProvider = await FilesProvider.CreateAsync(Folder, "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);
		}

		/// <summary>
		/// Cleans up the test environment.
		/// </summary>
		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await new MatterModule().Stop();

			if (!(filesProvider is null))
			{
				await filesProvider.DisposeAsync();
				filesProvider = null;
			}
		}
	}
}
