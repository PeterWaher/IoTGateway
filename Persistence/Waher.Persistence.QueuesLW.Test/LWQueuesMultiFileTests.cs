using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
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
	public sealed class DBQueuesMultiFileTests
	{
		internal const string DataFolder = "Data";
		internal const string QueueFolder = "Data\\Queue";
		internal const string UmlFolder = "UmlMulti";
		internal const string CollectionName = "Default";
		internal const int MaxFileSize = 20480;

		private FilesProvider provider;
		private MultiFileQueue queue;
		private FullSerialization fullSerialization;
		private Profiler profiler;

		public TestContext TestContext { get; set; }

		[TestInitialize]
		public async Task TestInitialize()
		{
			DeleteFiles();

#if LW
			this.provider = await FilesProvider.CreateAsync(DataFolder, CollectionName, 8192, 8192, 4096, Encoding.UTF8, 8192);
#else
			this.provider = await FilesProvider.CreateAsync(DataFolder, CollectionName, 8192, 8192, 4096, Encoding.UTF8, 8192, true);
#endif
			this.fullSerialization = new FullSerialization();
			this.profiler = new Profiler(ProfilerThreadType.Sequential);
			this.profiler.Start();

			this.queue = await MultiFileQueue.Create(QueueFolder, true, MaxFileSize,
				this.fullSerialization, this.provider, this.profiler);
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			Tuple<long, long, long, long> P = await this.queue.GetFileState();
			long FirstFilePosition = P.Item1;
			long FirstFileSize = P.Item2;
			long LastFilePosition = P.Item3;
			long LastFileSize = P.Item4;
			int NrFiles = await this.queue.GetFileCount();
			int NrDequeuers = await this.queue.GetNrDequeuers();
			int NrEnqueuers = await this.queue.GetNrEnqueuers();

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

			Assert.AreEqual(0, NrDequeuers, "There are still dequeuers waiting for items.");
			Assert.AreEqual(0, NrEnqueuers, "There are still enqueuers waiting for space.");
			Assert.AreEqual(1, NrFiles, "There should only be one file at the end.");
			Assert.AreEqual(0L, FirstFilePosition, "Queue not cleared.");
			Assert.AreEqual(0L, FirstFileSize, "Queue file not empty.");
			Assert.AreEqual(0L, LastFilePosition, "Queue not cleared.");
			Assert.AreEqual(0L, LastFileSize, "Queue file not empty.");
		}

		internal static void DeleteFiles()
		{
			if (Directory.Exists(QueueFolder))
			{
				string[] Files = Directory.GetFiles(QueueFolder, "*.queue");

				foreach (string FileName in Files)
					File.Delete(FileName);
			}
		}

		[TestMethod]
		public async Task Test_01_EnqueueDequeue_ValueTypes()
		{
			Assert.IsTrue(await this.queue.Enqueue(1));
			Assert.AreEqual(1, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_02_EnqueuePeekTryDequeue_ValueTypes()
		{
			Assert.IsTrue(await this.queue.Enqueue(1));
			Assert.AreEqual(1, await this.queue.Peek());
			Assert.AreEqual(1, await this.queue.TryDequeue());
			Assert.IsNull(await this.queue.TryDequeue());
		}

		[TestMethod]
		public async Task Test_03_EnqueueDequeue_Multiple_ValueTypes()
		{
			int i;

			for (i = 0; i < 10000; i++)
				Assert.IsTrue(await this.queue.Enqueue(i));

			for (i = 0; i < 10000; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_04_DequeueEnqueue_ValueTypes()
		{
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
		public async Task Test_05_DequeueEnqueue_Multiple_ValueTypes()
		{
			TaskCompletionSource<bool> EnqueueResult = new();

			_ = Task.Run(async () =>
			{
				for (int j = 0; j < 10000; j++)
				{
					await Task.Delay(1, CancellationToken.None);
					if (!await this.queue.Enqueue(j))
					{
						EnqueueResult.TrySetResult(false);
						return;
					}
				}

				EnqueueResult.TrySetResult(true);
			}, CancellationToken.None);

			for (int i = 0; i < 10000; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));

			Assert.IsTrue(await EnqueueResult.Task);
		}

		[TestMethod]
		public async Task Test_06_DequeueEnqueue_RandomMultiple_ValueTypes()
		{
			Random Rnd = new();
			TaskCompletionSource<bool> EnqueueDone = new();
			TaskCompletionSource<bool> DequeueDone = new();

			_ = Task.Run(async () =>
			{
				try
				{
					for (int i = 0; i < 10000; i++)
					{
						await Task.Delay(Rnd.Next(1, 3), CancellationToken.None);
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
					for (int i = 0; i < 10000; i++)
					{
						await Task.Delay(Rnd.Next(1, 3), CancellationToken.None);
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
		public async Task Test_07_EnqueueDequeue_ReferenceTypes()
		{
			Assert.IsTrue(await this.queue.Enqueue(new Simple(1, "Object 1")));
			Simple Item = await this.queue.Dequeue(10000) as Simple;
			Assert.IsNotNull(Item);
			Assert.AreEqual(1, Item.Counter);
			Assert.AreEqual("Object 1", Item.Message);
		}

		[TestMethod]
		public async Task Test_08_EnqueuePeekTryDequeue_ReferenceTypes()
		{
			Assert.IsTrue(await this.queue.Enqueue(new Simple(1, "Object 1")));

			Simple Item = await this.queue.Peek() as Simple;
			Assert.IsNotNull(Item);
			Assert.AreEqual(1, Item.Counter);
			Assert.AreEqual("Object 1", Item.Message);

			Item = await this.queue.TryDequeue() as Simple;
			Assert.IsNotNull(Item);
			Assert.AreEqual(1, Item.Counter);
			Assert.AreEqual("Object 1", Item.Message);

			Assert.IsNull(await this.queue.TryDequeue());
		}

		[TestMethod]
		public async Task Test_09_EnqueueDequeue_Multiple_ReferenceTypes()
		{
			int i;

			for (i = 0; i < 10000; i++)
				Assert.IsTrue(await this.queue.Enqueue(new Simple(i, "Object " + i)));

			for (i = 0; i < 10000; i++)
			{
				Simple Item = await this.queue.Dequeue(10000) as Simple;
				Assert.IsNotNull(Item);
				Assert.AreEqual(i, Item.Counter);
				Assert.AreEqual("Object " + i, Item.Message);
			}
		}

		[TestMethod]
		public async Task Test_10_DequeueEnqueue_ReferenceTypes()
		{
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
		public async Task Test_11_DequeueEnqueue_Multiple_ReferenceTypes()
		{
			TaskCompletionSource<bool> EnqueueResult = new();

			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 10000; i++)
				{
					await Task.Delay(1, CancellationToken.None);
					if (!await this.queue.Enqueue(new Simple(i, "Object " + i)))
					{
						EnqueueResult.TrySetResult(false);
						return;
					}
				}

				EnqueueResult.TrySetResult(true);
			}, CancellationToken.None);

			for (int i = 0; i < 10000; i++)
			{
				Simple Item = await this.queue.Dequeue(10000) as Simple;
				Assert.IsNotNull(Item);
				Assert.AreEqual(i, Item.Counter);
				Assert.AreEqual("Object " + i, Item.Message);
			}

			Assert.IsTrue(await EnqueueResult.Task);
		}

		[TestMethod]
		public async Task Test_12_DequeueEnqueue_RandomMultiple_ReferenceTypes()
		{
			Random Rnd = new();
			TaskCompletionSource<bool> EnqueueDone = new();
			TaskCompletionSource<bool> DequeueDone = new();

			_ = Task.Run(async () =>
			{
				try
				{
					for (int i = 0; i < 10000; i++)
					{
						await Task.Delay(Rnd.Next(1, 3), CancellationToken.None);
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
					for (int i = 0; i < 10000; i++)
					{
						await Task.Delay(Rnd.Next(1, 3), CancellationToken.None);

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
		public async Task Test_13_Timeout()
		{
			Assert.IsNull(await this.queue.Dequeue(2000));
		}

		[TestMethod]
		public async Task Test_14_CloseAndResume()
		{
			int i;

			for (i = 0; i < 10000; i++)
				Assert.IsTrue(await this.queue.Enqueue(i));

			for (i = 0; i < 5000; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));

			await this.queue.DisposeAsync();
			this.queue = await MultiFileQueue.Create(QueueFolder, true, MaxFileSize,
				this.fullSerialization, this.provider);

			for (; i < 10000; i++)
				Assert.AreEqual(i, await this.queue.Dequeue(10000));
		}

		[TestMethod]
		public async Task Test_15_DequeueEnqueue_MultipleThreads_ValueTypes()
		{
			SortedDictionary<int, bool> Dequeued = [];
			Random Rnd = new();
			Enqueuer[] Enqueuers = new Enqueuer[10];
			Dequeuer[] Dequeuers = new Dequeuer[10];

			for (int i = 0; i < 10; i++)
			{
				Enqueuers[i] = new(i * 1000, 1000, 0, 1, Rnd, this.queue);
				_ = Task.Run(Enqueuers[i].Start, CancellationToken.None);
			}

			for (int i = 0; i < 10; i++)
			{
				Dequeuers[i] = new(1000, 0, 1, Rnd, this.queue, Dequeued);
				_ = Task.Run(Dequeuers[i].Start, CancellationToken.None);
			}

			foreach (Enqueuer Enqueuer in Enqueuers)
				Assert.IsTrue(await Enqueuer.Wait(), "Enqueuer not finished.");

			foreach (Dequeuer Dequeuer in Dequeuers)
				Assert.IsTrue(await Dequeuer.Wait(), "Dequeuer not finished.");

			Assert.HasCount(10000, Dequeued, "Not all items were dequeued.");

			for (int i = 0; i < 10000; i++)
			{
				if (!Dequeued.ContainsKey(i))
					Assert.Fail("Item " + i.ToString() + " was not dequeued.");
			}
		}
	}
}
