using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Persistence.Queues.Test
{
	[TestClass]
	public sealed class DBQueuesSingleFileTests
	{
		internal const string Folder = "Data";
		internal const string CollectionName = "Default";
		internal const string QueueFileName = "Data\\Test.queue";
		internal const int MaxFileSize = 10240;

		private FilesProvider provider;
		private SingleFileQueue queue;
		private FullSerialization fullSerialization;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(DBQueuesSingleFileTests).Assembly,
				typeof(Expression).Assembly,
				typeof(Script.Persistence.SQL.Select).Assembly,
				typeof(Duration).Assembly);

			Types.SetModuleParameter("Data", "Data");
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			DeleteFiles();

#if LW
			this.provider = await FilesProvider.CreateAsync(Folder, CollectionName, 8192, 8192, 4096, Encoding.UTF8, 8192);
#else
			this.provider = await FilesProvider.CreateAsync(Folder, CollectionName, 8192, 8192, 4096, Encoding.UTF8, 8192, true);
#endif
			this.fullSerialization = new FullSerialization();
			this.queue = await SingleFileQueue.Create(QueueFileName, true, MaxFileSize,
				this.fullSerialization.Serializers, this.provider);
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.provider is not null)
			{
				await this.provider.DisposeAsync();
				this.provider = null;

				this.queue.Dispose();
				this.queue = null;
			}
		}

		internal static void DeleteFiles()
		{
			if (File.Exists(QueueFileName))
				File.Delete(QueueFileName);
		}

		[TestMethod]
		public void Test_01_EnqueueDequeue()
		{
		}
	}
}
