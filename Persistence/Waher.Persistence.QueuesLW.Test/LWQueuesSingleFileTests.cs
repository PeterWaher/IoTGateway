using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Script;

#if !LW
using Waher.Persistence.Queues.Test.Classes;
namespace Waher.Persistence.Queues.Test
#else
using Waher.Persistence.Queues;
using Waher.Persistence.QueuesLW.Test.Classes;
namespace Waher.Persistence.QueuesLW.Test
#endif
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
		public async Task Test_01_EnqueueDequeue_ValueTypes()
		{
			await this.queue.Enqueue(1);
			Assert.AreEqual(1, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_02_EnqueueDequeue_Multiple_ValueTypes()
		{
			int i;

			for (i = 0; i < 10; i++)
				await this.queue.Enqueue(i);

			for (i = 0; i < 10; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_03_DequeueEnqueue_ValueTypes()
		{
			_ = Task.Run(async () =>
			{
				await Task.Delay(1000, CancellationToken.None);
				await this.queue.Enqueue(1);
			}, CancellationToken.None);

			Assert.AreEqual(1, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_04_DequeueEnqueue_Multiple_ValueTypes()
		{
			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 10; i++)
				{
					await Task.Delay(100, CancellationToken.None);
					await this.queue.Enqueue(i);
				}
			}, CancellationToken.None);

			for (int i = 0; i < 10; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_05_EnqueueDequeue_ReferenceTypes()
		{
			await this.queue.Enqueue(new Simple(1, "Object 1"));
			Simple Item = await this.queue.Dequeue(10000) as Simple;
			Assert.IsNotNull(Item);
			Assert.AreEqual(1, Item.Counter);
			Assert.AreEqual("Object 1", Item.Message);
		}

		[TestMethod]
		public async Task Test_06_EnqueueDequeue_Multiple_ReferenceTypes()
		{
			int i;

			for (i = 0; i < 10; i++)
				await this.queue.Enqueue(new Simple(i, "Object " + i));

			for (i = 0; i < 10; i++)
			{
				Simple Item = await this.queue.Dequeue(10000) as Simple;
				Assert.IsNotNull(Item);
				Assert.AreEqual(i, Item.Counter);
				Assert.AreEqual("Object " + i, Item.Message);
			}
		}

		[TestMethod]
		public async Task Test_07_DequeueEnqueue_ReferenceTypes()
		{
			_ = Task.Run(async () =>
			{
				await Task.Delay(1000, CancellationToken.None);
				await this.queue.Enqueue(new Simple(1, "Object 1"));
			}, CancellationToken.None);

			Simple Item = await this.queue.Dequeue(10000) as Simple;
			Assert.IsNotNull(Item);
			Assert.AreEqual(1, Item.Counter);
			Assert.AreEqual("Object 1", Item.Message);
		}

		[TestMethod]
		public async Task Test_08_DequeueEnqueue_Multiple_ReferenceTypes()
		{
			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 10; i++)
				{
					await Task.Delay(100, CancellationToken.None);
					await this.queue.Enqueue(new Simple(i, "Object " + i));
				}
			}, CancellationToken.None);

			for (int i = 0; i < 10; i++)
			{
				Simple Item = await this.queue.Dequeue(10000) as Simple;
				Assert.IsNotNull(Item);
				Assert.AreEqual(i, Item.Counter);
				Assert.AreEqual("Object " + i, Item.Message);
			}
		}

		// TODO: Wait while enqueueing if max file size is reached.
		// TODO: Test Timeout
	}
}
