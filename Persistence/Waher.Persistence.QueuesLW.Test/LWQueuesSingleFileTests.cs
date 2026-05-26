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
using Waher.Runtime.Profiling;


#if !LW
using Waher.Persistence.Queues.Test.Classes;
using Waher.Persistence.Queues.Test.Workers;
namespace Waher.Persistence.Queues.Test
#else
using Waher.Persistence.Queues;
using Waher.Persistence.QueuesLW.Test.Classes;
using Waher.Persistence.QueuesLW.Test.Workers;
namespace Waher.Persistence.QueuesLW.Test
#endif
{
	[TestClass]
	public sealed class DBQueuesSingleFileTests
	{
		internal const string Folder = "Data";
		internal const string QueueFileName = "Data\\Test.queue";
		internal const string UmlFolder = "UmlSingle";
		internal const string CollectionName = "Default";
		internal const int MaxFileSize = 20480;

		private FilesProvider provider;
		private SingleFileQueue queue;
		private FullSerialization fullSerialization;
		private Profiler profiler;

		public TestContext TestContext { get; set; }

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
			this.profiler = new Profiler(ProfilerThreadType.Sequential);
			this.profiler.Start();
		}

		private async Task InitQueue(QueueThresholdMode Mode)
		{
			this.queue = await SingleFileQueue.Create(QueueFileName, true, MaxFileSize,
				Mode, this.fullSerialization.Serializers, this.provider, this.profiler);
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			KeyValuePair<long, long> P = await this.queue.GetFileState();
			int NrDequeuers = await this.queue.GetNrDequeuers();
			int NrEnqueuers = await this.queue.GetNrEnqueuers();
			long FilePosition = P.Key;
			long FileSize = P.Value;

			if (this.provider is not null)
			{
				await this.provider.DisposeAsync();
				this.provider = null;

				await this.queue.DisposeAsync();
				this.queue = null;
			}

			this.profiler.Stop();

			if (!Directory.Exists(UmlFolder))
				Directory.CreateDirectory(UmlFolder);

			string Uml = this.profiler.ExportPlantUml(TimeUnit.Seconds);
			string FileName = Path.Combine(UmlFolder, this.TestContext.TestName + ".uml");

			File.WriteAllText(FileName, Uml);
			Console.Out.WriteLine(Uml);

			Assert.AreEqual(0L, FilePosition, "Queue not cleared.");
			Assert.AreEqual(0L, FileSize, "Queue file not empty.");
			Assert.AreEqual(0, NrDequeuers, "There are still dequeuers waiting for items.");
			Assert.AreEqual(0, NrEnqueuers, "There are still enqueuers waiting for space.");
		}

		internal static void DeleteFiles()
		{
			if (File.Exists(QueueFileName))
				File.Delete(QueueFileName);
		}

		[TestMethod]
		public async Task Test_01_EnqueueDequeue_ValueTypes()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

			Assert.IsTrue(await this.queue.Enqueue(1));
			Assert.AreEqual(1, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_02_EnqueueDequeue_Multiple_ValueTypes()
		{
			int i;

			await this.InitQueue(QueueThresholdMode.Exception);

			for (i = 0; i < 10; i++)
				Assert.IsTrue(await this.queue.Enqueue(i));

			for (i = 0; i < 10; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_03_DequeueEnqueue_ValueTypes()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

			TaskCompletionSource<bool> EnqueueResult = new();

			_ = Task.Run(async () =>
			{
				await Task.Delay(1000, CancellationToken.None);
				EnqueueResult.TrySetResult(await this.queue.Enqueue(1));
			}, CancellationToken.None);

			Assert.IsTrue(await EnqueueResult.Task);
			Assert.AreEqual(1, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_04_DequeueEnqueue_Multiple_ValueTypes()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

			TaskCompletionSource<bool> EnqueueResult = new();

			_ = Task.Run(async () =>
			{
				for (int j = 0; j < 10; j++)
				{
					await Task.Delay(100, CancellationToken.None);
					if (!await this.queue.Enqueue(j))
					{
						EnqueueResult.TrySetResult(false);
						return;
					}
				}

				EnqueueResult.TrySetResult(true);
			}, CancellationToken.None);

			for (int i = 0; i < 10; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));

			Assert.IsTrue(await EnqueueResult.Task);
		}

		[TestMethod]
		public async Task Test_05_DequeueEnqueue_RandomMultiple_ValueTypes()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

			Random Rnd = new();
			TaskCompletionSource<bool> EnqueueDone = new();
			TaskCompletionSource<bool> DequeueDone = new();

			_ = Task.Run(async () =>
			{
				try
				{
					for (int i = 0; i < 100; i++)
					{
						await Task.Delay(Rnd.Next(50, 150), CancellationToken.None);
						if (!await this.queue.Enqueue(i))
						{
							EnqueueDone.TrySetResult(false);
							return;
						}
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
						if (await this.queue.Dequeue(10000) is not int j || i != j)
						{
							DequeueDone.TrySetResult(false);
							return;
						}
					}

					DequeueDone.TrySetResult(true);
				}
				catch (Exception ex)
				{
					DequeueDone.TrySetException(ex);
				}
			}, CancellationToken.None);

			Assert.IsTrue(await EnqueueDone.Task);
			Assert.IsTrue(await DequeueDone.Task);
		}

		[TestMethod]
		public async Task Test_06_EnqueueDequeue_ReferenceTypes()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

			Assert.IsTrue(await this.queue.Enqueue(new Simple(1, "Object 1")));
			Simple Item = await this.queue.Dequeue(10000) as Simple;
			Assert.IsNotNull(Item);
			Assert.AreEqual(1, Item.Counter);
			Assert.AreEqual("Object 1", Item.Message);
		}

		[TestMethod]
		public async Task Test_07_EnqueueDequeue_Multiple_ReferenceTypes()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

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
			await this.InitQueue(QueueThresholdMode.Exception);

			TaskCompletionSource<bool> EnqueueResult = new();

			_ = Task.Run(async () =>
			{
				await Task.Delay(1000, CancellationToken.None);
				EnqueueResult.TrySetResult(await this.queue.Enqueue(new Simple(1, "Object 1")));
			}, CancellationToken.None);

			Assert.IsTrue(await EnqueueResult.Task);

			Simple Item = await this.queue.Dequeue(10000) as Simple;
			Assert.IsNotNull(Item);
			Assert.AreEqual(1, Item.Counter);
			Assert.AreEqual("Object 1", Item.Message);
		}

		[TestMethod]
		public async Task Test_09_DequeueEnqueue_Multiple_ReferenceTypes()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

			TaskCompletionSource<bool> EnqueueResult = new();

			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 10; i++)
				{
					await Task.Delay(100, CancellationToken.None);
					if (!await this.queue.Enqueue(new Simple(i, "Object " + i)))
					{
						EnqueueResult.TrySetResult(false);
						return;
					}
				}

				EnqueueResult.TrySetResult(true);
			}, CancellationToken.None);

			for (int i = 0; i < 10; i++)
			{
				Simple Item = await this.queue.Dequeue(10000) as Simple;
				Assert.IsNotNull(Item);
				Assert.AreEqual(i, Item.Counter);
				Assert.AreEqual("Object " + i, Item.Message);
			}

			Assert.IsTrue(await EnqueueResult.Task);
		}

		[TestMethod]
		public async Task Test_10_DequeueEnqueue_RandomMultiple_ReferenceTypes()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

			Random Rnd = new();
			TaskCompletionSource<bool> EnqueueDone = new();
			TaskCompletionSource<bool> DequeueDone = new();

			_ = Task.Run(async () =>
			{
				try
				{
					for (int i = 0; i < 100; i++)
					{
						await Task.Delay(Rnd.Next(50, 150), CancellationToken.None);
						if (!await this.queue.Enqueue(new Simple(i, "Object " + i)))
						{
							EnqueueDone.TrySetResult(false);
							return;
						}
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
					
						if (await this.queue.Dequeue(10000) is not Simple Item || 
							i != Item.Counter || 
							"Object " + i != Item.Message)
						{
							DequeueDone.TrySetResult(false);
							return;
						}
					}

					DequeueDone.TrySetResult(true);
				}
				catch (Exception ex)
				{
					DequeueDone.TrySetException(ex);
				}
			}, CancellationToken.None);

			Assert.IsTrue(await EnqueueDone.Task);
			Assert.IsTrue(await DequeueDone.Task);
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

			int MaxBlocks = this.queue.MaxFileSize / 64;
			int j = 1000 / MaxBlocks * MaxBlocks;

			while (j <= i)
				Assert.AreEqual(j++, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_14_Timeout()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

			Assert.IsNull(await this.queue.Dequeue(2000));
		}

		[TestMethod]
		public async Task Test_15_Threshold_Wait()
		{
			TaskCompletionSource<bool> DequeueDone = new();
			int i = 0;
			int j = 0;

			await this.InitQueue(QueueThresholdMode.Wait);
			this.queue.TrimTimeout = 2000;

			_ = Task.Run(async () =>
			{
				while (j < 1000)
				{
					await Task.Delay(10, CancellationToken.None);
					object Item = await this.queue.Dequeue(10000);

					Console.Out.WriteLine(DateTime.UtcNow.ToLongTimeString() +
						": Dequeued: " + Item?.ToString());

					if (Item is null || !Item.Equals(j++))
					{
						DequeueDone.TrySetResult(false);
						return;
					}
				}

				DequeueDone.TrySetResult(true);

			}, CancellationToken.None);

			while (i < 1000 && await this.queue.Enqueue(i, 10000))
			{
				Console.Out.WriteLine(DateTime.UtcNow.ToLongTimeString() +
						": Enqueued: " + i.ToString());
				i++;
			}

			_ = Task.Delay(10000, CancellationToken.None).ContinueWith((_) =>
			{
				DequeueDone.TrySetResult(false);
			}, CancellationToken.None);

			Assert.AreEqual(1000, i);
			Assert.IsTrue(await DequeueDone.Task);
		}

		[TestMethod]
		public async Task Test_16_CloseAndResume()
		{
			int i;

			await this.InitQueue(QueueThresholdMode.Exception);

			for (i = 0; i < 100; i++)
				Assert.IsTrue(await this.queue.Enqueue(i));

			for (i = 0; i < 50; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));

			await this.queue.DisposeAsync();
			await this.InitQueue(QueueThresholdMode.Exception);

			for (; i < 100; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_17_DequeueEnqueue_MultipleThreads_ValueTypes()
		{
			await this.InitQueue(QueueThresholdMode.Exception);

			SortedDictionary<int, bool> Dequeued = [];
			Random Rnd = new();
			Enqueuer[] Enqueuers = new Enqueuer[10];
			Dequeuer[] Dequeuers = new Dequeuer[10];

			for (int i = 0; i < 10; i++)
			{
				Enqueuers[i] = new(i * 100, 100, 50, 150, Rnd, this.queue);
				_ = Task.Run(Enqueuers[i].Start, CancellationToken.None);
			}

			for (int i = 0; i < 10; i++)
			{
				Dequeuers[i] = new(100, Rnd, this.queue, Dequeued);
				_ = Task.Run(Dequeuers[i].Start, CancellationToken.None);
			}

			foreach (Enqueuer Enqueuer in Enqueuers)
				Assert.IsTrue(await Enqueuer.Wait(), "Enqueuer not finished.");

			foreach (Dequeuer Dequeuer in Dequeuers)
				Assert.IsTrue(await Dequeuer.Wait(), "Dequeuer not finished.");

			Assert.HasCount(1000, Dequeued, "Not all items were dequeued.");

			for (int i = 0; i < 1000; i++)
			{
				if (!Dequeued.ContainsKey(i))
					Assert.Fail("Item " + i.ToString() + " was not dequeued.");
			}
		}
	}
}
