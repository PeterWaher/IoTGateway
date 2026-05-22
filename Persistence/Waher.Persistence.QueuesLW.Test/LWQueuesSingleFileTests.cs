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
using System.Collections.Generic;
using System;



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
		}

		private async Task InitQueue(QueueThresholdMode Mode)
		{
			this.queue = await SingleFileQueue.Create(QueueFileName, true, MaxFileSize,
				Mode, this.fullSerialization.Serializers, this.provider);
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			KeyValuePair<long, long> P = await this.queue.GetFileState();
			long FilePosition = P.Key;
			long FileSize = P.Value;

			if (this.provider is not null)
			{
				await this.provider.DisposeAsync();
				this.provider = null;

				this.queue.Dispose();
				this.queue = null;
			}

			Assert.AreEqual(0L, FilePosition);
			Assert.AreEqual(0L, FileSize);
		}

		internal static void DeleteFiles()
		{
			if (File.Exists(QueueFileName))
				File.Delete(QueueFileName);
		}

		[TestMethod]
		public async Task Test_01_EnqueueDequeue_ValueTypes()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			Assert.IsTrue(await this.queue.Enqueue(1));
			Assert.AreEqual(1, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_02_EnqueueDequeue_Multiple_ValueTypes()
		{
			int i;

			await this.InitQueue(QueueThresholdMode.Ignore);

			for (i = 0; i < 10; i++)
				Assert.IsTrue(await this.queue.Enqueue(i));

			for (i = 0; i < 10; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_03_DequeueEnqueue_ValueTypes()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			_ = Task.Run(async () =>
			{
				await Task.Delay(1000, CancellationToken.None);
				Assert.IsTrue(await this.queue.Enqueue(1));
			}, CancellationToken.None);

			Assert.AreEqual(1, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_04_DequeueEnqueue_Multiple_ValueTypes()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 10; i++)
				{
					await Task.Delay(100, CancellationToken.None);
					Assert.IsTrue(await this.queue.Enqueue(i));
				}
			}, CancellationToken.None);

			for (int i = 0; i < 10; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_05_DequeueEnqueue_RandomMultiple_ValueTypes()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			Random Rnd = new();
			TaskCompletionSource<bool> EnqueueDone = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> DequeueDone = new TaskCompletionSource<bool>();

			_ = Task.Run(async () =>
			{
				try
				{
					for (int i = 0; i < 100; i++)
					{
						await Task.Delay(Rnd.Next(50, 150), CancellationToken.None);
						Assert.IsTrue(await this.queue.Enqueue(i));
					}

					EnqueueDone.TrySetResult(true);
				}
				catch (Exception ex)
				{
					EnqueueDone.TrySetException(ex);
				}
			}, CancellationToken.None);

			_ = Task.Run(async () =>
			{
				try
				{
					for (int i = 0; i < 100; i++)
					{
						await Task.Delay(Rnd.Next(50, 150), CancellationToken.None);
						Assert.AreEqual(i, await this.queue.Dequeue(10000));
					}

					DequeueDone.TrySetResult(true);
				}
				catch (Exception ex)
				{
					DequeueDone.TrySetException(ex);
				}
			}, CancellationToken.None);

			await EnqueueDone.Task;
			await DequeueDone.Task;
		}

		[TestMethod]
		public async Task Test_06_EnqueueDequeue_ReferenceTypes()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			Assert.IsTrue(await this.queue.Enqueue(new Simple(1, "Object 1")));
			Simple Item = await this.queue.Dequeue(10000) as Simple;
			Assert.IsNotNull(Item);
			Assert.AreEqual(1, Item.Counter);
			Assert.AreEqual("Object 1", Item.Message);
		}

		[TestMethod]
		public async Task Test_07_EnqueueDequeue_Multiple_ReferenceTypes()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			int i;

			for (i = 0; i < 10; i++)
				Assert.IsTrue(await this.queue.Enqueue(new Simple(i, "Object " + i)));

			for (i = 0; i < 10; i++)
			{
				Simple Item = await this.queue.Dequeue(10000) as Simple;
				Assert.IsNotNull(Item);
				Assert.AreEqual(i, Item.Counter);
				Assert.AreEqual("Object " + i, Item.Message);
			}
		}

		[TestMethod]
		public async Task Test_08_DequeueEnqueue_ReferenceTypes()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			_ = Task.Run(async () =>
			{
				await Task.Delay(1000, CancellationToken.None);
				Assert.IsTrue(await this.queue.Enqueue(new Simple(1, "Object 1")));
			}, CancellationToken.None);

			Simple Item = await this.queue.Dequeue(10000) as Simple;
			Assert.IsNotNull(Item);
			Assert.AreEqual(1, Item.Counter);
			Assert.AreEqual("Object 1", Item.Message);
		}

		[TestMethod]
		public async Task Test_09_DequeueEnqueue_Multiple_ReferenceTypes()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 10; i++)
				{
					await Task.Delay(100, CancellationToken.None);
					Assert.IsTrue(await this.queue.Enqueue(new Simple(i, "Object " + i)));
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

		[TestMethod]
		public async Task Test_10_DequeueEnqueue_RandomMultiple_ReferenceTypes()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			Random Rnd = new();
			TaskCompletionSource<bool> EnqueueDone = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> DequeueDone = new TaskCompletionSource<bool>();

			_ = Task.Run(async () =>
			{
				try
				{
					for (int i = 0; i < 100; i++)
					{
						await Task.Delay(Rnd.Next(50, 150), CancellationToken.None);
						Assert.IsTrue(await this.queue.Enqueue(new Simple(i, "Object " + i)));
					}

					EnqueueDone.TrySetResult(true);
				}
				catch (Exception ex)
				{
					EnqueueDone.TrySetException(ex);
				}
			}, CancellationToken.None);

			_ = Task.Run(async () =>
			{
				try
				{
					for (int i = 0; i < 100; i++)
					{
						await Task.Delay(Rnd.Next(50, 150), CancellationToken.None);
						Simple Item = await this.queue.Dequeue(10000) as Simple;
						Assert.IsNotNull(Item);
						Assert.AreEqual(i, Item.Counter);
						Assert.AreEqual("Object " + i, Item.Message);
					}

					DequeueDone.TrySetResult(true);
				}
				catch (Exception ex)
				{
					DequeueDone.TrySetException(ex);
				}
			}, CancellationToken.None);

			await EnqueueDone.Task;
			await DequeueDone.Task;
		}

		[TestMethod]
		public async Task Test_11_Threshold_Ignore()
		{
			int i = 0;

			await this.InitQueue(QueueThresholdMode.Ignore);

			while (await this.queue.Enqueue(i))
				i++;

			Assert.AreEqual(this.queue.MaxFileSize / 64, i);    // 64 is the minimum block size used by SerialFile.

			for (int j = 0; j < i; j++)
				Assert.AreEqual(j, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_12_Threshold_Exception()
		{
			int i = 0;

			await this.InitQueue(QueueThresholdMode.Exception);

			await Assert.ThrowsAsync<IOException>(async () =>
			{
				while (await this.queue.Enqueue(i))
					i++;
			});

			Assert.AreEqual(this.queue.MaxFileSize / 64, i);    // 64 is the minimum block size used by SerialFile.

			for (int j = 0; j < i; j++)
				Assert.AreEqual(j, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_13_Threshold_Clear()
		{
			int i = 0;

			await this.InitQueue(QueueThresholdMode.Clear);

			while (await this.queue.Enqueue(i) && i < 1000)
				i++;

			Assert.AreEqual(1000, i);

			int j = 1000 / (this.queue.MaxFileSize / 64) * 160;

			while (j <= i)
				Assert.AreEqual(j++, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_14_Timeout()
		{
			await this.InitQueue(QueueThresholdMode.Ignore);

			Assert.IsNull(await this.queue.Dequeue(2000));
		}

		// TODO: Test Threshold mode: Wait, NewFile
	}
}
