using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.HTTP.HTTP2;

namespace Waher.Networking.HTTP.Test
{
	/// <summary>
	/// Test flow control implementation for HTTP/2, as defined in RFC 9218
	/// </summary>
	[TestClass]
	public class Http20FlowControlRfc9218Tests
	{
		[TestMethod]
		public async Task Test_01_BasicFlowControl()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc9218 Node));
			Assert.AreEqual(0, Node.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream));
			Assert.AreEqual(1, Stream.StreamId);
		}

		[TestMethod]
		public async Task Test_02_SiblingStreams()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);
			FlowControl.AddStreamForTest(3, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			Task<int> T = FlowControl.RequestResources(3, 10000);
			Assert.IsFalse(T.IsCompleted);

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc9218 Node1));
			Assert.AreEqual(0, Node1.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream1));
			Assert.AreEqual(1, Stream1.StreamId);

			Assert.AreEqual(65535, FlowControl.ReleaseConnectionResources(65535));
			Assert.AreEqual(65535, FlowControl.ReleaseStreamResources(1, 65535));
			Assert.IsTrue(FlowControl.RemoveStream(1));

			Assert.AreEqual(10000, await T);

			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(3, 10000));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(3, out PriorityNodeRfc9218 Node2));
			Assert.AreEqual(0, Node2.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(3, out Http2Stream Stream2));
			Assert.AreEqual(3, Stream2.StreamId);
		}

		[TestMethod]
		public async Task Test_03_Cancellation()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			DateTime Start = DateTime.Now;
			CancellationTokenSource Cancel = new();
			Cancel.CancelAfter(1000);
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
			{
				await FlowControl.RequestResources(1, 10000, Cancel.Token);
			});

			Assert.IsTrue(DateTime.Now.Subtract(Start).TotalSeconds >= 1);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream));
			Assert.AreEqual(1, Stream.StreamId);
		}

		[TestMethod]
		public async Task Test_04_ReleaseStreamResources()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			Assert.AreEqual(10000, FlowControl.ReleaseStreamResources(1, 10000));

			DateTime Start = DateTime.Now;
			CancellationTokenSource Cancel = new();
			Cancel.CancelAfter(1000);
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
			{
				await FlowControl.RequestResources(1, 10000, Cancel.Token);
			});

			Assert.IsTrue(DateTime.Now.Subtract(Start).TotalSeconds >= 1);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream));
			Assert.AreEqual(1, Stream.StreamId);
		}

		[TestMethod]
		public async Task Test_05_ReleaseConnectionResources()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			Assert.AreEqual(10000, FlowControl.ReleaseConnectionResources(10000));

			DateTime Start = DateTime.Now;
			CancellationTokenSource Cancel = new();
			Cancel.CancelAfter(1000);
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
			{
				await FlowControl.RequestResources(1, 10000, Cancel.Token);
			});

			Assert.IsTrue(DateTime.Now.Subtract(Start).TotalSeconds >= 1);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream));
			Assert.AreEqual(1, Stream.StreamId);
		}


		[TestMethod]
		public async Task Test_06_ReleaseResources()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			Assert.AreEqual(10000, FlowControl.ReleaseStreamResources(1, 10000));
			Assert.AreEqual(10000, FlowControl.ReleaseConnectionResources(10000));

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc9218 Node));
			Assert.AreEqual(0, Node.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream));
			Assert.AreEqual(1, Stream.StreamId);
		}

		[TestMethod]
		public async Task Test_07_MaxFrameSize()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);

			Assert.AreEqual(16384, await FlowControl.RequestResources(1, 20000));
			Assert.AreEqual(16384, await FlowControl.RequestResources(1, 20000));
			Assert.AreEqual(16384, await FlowControl.RequestResources(1, 20000));
			Assert.AreEqual(16383, await FlowControl.RequestResources(1, 20000));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc9218 Node));
			Assert.AreEqual(0, Node.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream));
			Assert.AreEqual(1, Stream.StreamId);
		}

		[TestMethod]
		public async Task Test_08_AsymmetricSiblingStreams()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);
			FlowControl.AddStreamForTest(3, 0, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc9218 Node1));
			Assert.AreEqual(45535, Node1.AvailableResources);
			Assert.AreEqual(45535, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream1));
			Assert.AreEqual(1, Stream1.StreamId);

			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(3, 10000));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(3, out PriorityNodeRfc9218 Node2));
			Assert.AreEqual(20000, Node2.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(3, out Http2Stream Stream2));
			Assert.AreEqual(3, Stream2.StreamId);

			Task<int> T1 = FlowControl.RequestResources(1, 10000);
			Task<int> T2 = FlowControl.RequestResources(3, 10000);

			Assert.IsFalse(T1.IsCompleted);
			Assert.IsFalse(T2.IsCompleted);

			FlowControl.ReleaseConnectionResources(5000);

			Assert.AreEqual(5000, await T2);
			Assert.IsFalse(T1.IsCompleted);

			Assert.AreEqual(45535, Node1.AvailableResources);
			Assert.AreEqual(15000, Node2.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			T2 = FlowControl.RequestResources(3, 10000);
			Assert.IsFalse(T2.IsCompleted);

			FlowControl.ReleaseConnectionResources(30000);

			Assert.AreEqual(10000, await T2);
			Assert.AreEqual(10000, await T1);

			Assert.AreEqual(35535, Node1.AvailableResources);
			Assert.AreEqual(5000, Node2.AvailableResources);
			Assert.AreEqual(10000, FlowControl.Root.AvailableResources);
		}

		[TestMethod]
		public async Task Test_09_RemoveStream()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			DateTime Start = DateTime.Now;
			_ = Task.Delay(1000).ContinueWith((_) => FlowControl.RemoveStream(1));

			Assert.AreEqual(-1, await FlowControl.RequestResources(1, 10000));

			Assert.IsTrue(DateTime.Now.Subtract(Start).TotalSeconds >= 1);

			Assert.IsFalse(FlowControl.TryGetStream(1, out _));
		}

		[TestMethod]
		public async Task Test_10_ResourcesAndRemovedStreams()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc9218 FlowControl = new(LocalSettings, RemoteSettings);

			FlowControl.AddStreamForTest(1, 3, false);
			FlowControl.AddStreamForTest(3, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));

			FlowControl.RemoveStream(1);

			Assert.AreEqual(65535, FlowControl.ReleaseConnectionResources(20000));
			Assert.AreEqual(-1, FlowControl.ReleaseStreamResources(1, 20000));

			Assert.IsFalse(FlowControl.TryGetPriorityNode(1, out _));
			Assert.AreEqual(65535, FlowControl.Root.AvailableResources);

			Assert.IsFalse(FlowControl.TryGetStream(1, out _));
			Assert.IsTrue(FlowControl.TryGetStream(3, out Http2Stream Stream2));
			Assert.AreEqual(3, Stream2.StreamId);
		}

		/* 
		 * UpdatePriority + trigger
		 * UpdateSettings + trigger
		 * changing window sizes if sibling removed + trigger
		 */
	}
}
