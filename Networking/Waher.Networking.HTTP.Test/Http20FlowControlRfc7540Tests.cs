﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.HTTP.HTTP2;

namespace Waher.Networking.HTTP.Test
{
	/// <summary>
	/// Test flow control implementation for HTTP/2, as defined in RFC 7540
	/// </summary>
	[TestClass]
	public class Http20FlowControlRfc7540Tests
	{
		[TestMethod]
		public async Task Test_01_BasicFlowControl()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000, null));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc7540 Node));
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
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);
			FlowControl.AddStreamForTest(3);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(2767, await FlowControl.RequestResources(1, 10000, null));

			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(2767, await FlowControl.RequestResources(3, 10000, null));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc7540 Node1));
			Assert.AreEqual(0, Node1.AvailableResources);
			Assert.IsTrue(FlowControl.TryGetPriorityNode(3, out PriorityNodeRfc7540 Node2));
			Assert.AreEqual(0, Node2.AvailableResources);
			Assert.AreEqual(1, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream1));
			Assert.AreEqual(1, Stream1.StreamId);
			Assert.IsTrue(FlowControl.TryGetStream(3, out Http2Stream Stream2));
			Assert.AreEqual(3, Stream2.StreamId);
		}

		[TestMethod]
		public async Task Test_03_ChildDependencies()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);
			FlowControl.AddStreamForTest(3);
			FlowControl.AddStreamForTest(5, 16, 3, false);
			FlowControl.AddStreamForTest(7, 16, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(2767, await FlowControl.RequestResources(1, 10000, null));

			Assert.AreEqual(10000, await FlowControl.RequestResources(5, 10000, null));
			Assert.AreEqual(6383, await FlowControl.RequestResources(5, 10000, null));

			Assert.AreEqual(10000, await FlowControl.RequestResources(7, 10000, null));
			Assert.AreEqual(6383, await FlowControl.RequestResources(7, 10000, null));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc7540 Node1));
			Assert.AreEqual(0, Node1.AvailableResources);
			Assert.IsTrue(FlowControl.TryGetPriorityNode(3, out PriorityNodeRfc7540 Node2));
			Assert.AreEqual(32767, Node2.AvailableResources);
			Assert.IsTrue(FlowControl.TryGetPriorityNode(5, out PriorityNodeRfc7540 Node3));
			Assert.AreEqual(0, Node3.AvailableResources);
			Assert.IsTrue(FlowControl.TryGetPriorityNode(7, out PriorityNodeRfc7540 Node4));
			Assert.AreEqual(0, Node4.AvailableResources);
			Assert.AreEqual(2, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream1));
			Assert.AreEqual(1, Stream1.StreamId);
			Assert.IsTrue(FlowControl.TryGetStream(3, out Http2Stream Stream2));
			Assert.AreEqual(3, Stream2.StreamId);
			Assert.IsTrue(FlowControl.TryGetStream(5, out Http2Stream Stream3));
			Assert.AreEqual(5, Stream3.StreamId);
			Assert.IsTrue(FlowControl.TryGetStream(7, out Http2Stream Stream4));
			Assert.AreEqual(7, Stream4.StreamId);
		}

		[TestMethod]
		public async Task Test_04_Cancellation()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000, null));

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
		public async Task Test_05_ReleaseStreamResources()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000, null));

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
		public async Task Test_06_ReleaseConnectionResources()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000, null));

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
		public async Task Test_07_ReleaseResources()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000, null));

			Assert.AreEqual(10000, FlowControl.ReleaseStreamResources(1, 10000));
			Assert.AreEqual(10000, FlowControl.ReleaseConnectionResources(10000));

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc7540 Node));
			Assert.AreEqual(0, Node.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream));
			Assert.AreEqual(1, Stream.StreamId);
		}

		[TestMethod]
		public async Task Test_08_MaxFrameSize()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(16384, await FlowControl.RequestResources(1, 20000, null));
			Assert.AreEqual(16384, await FlowControl.RequestResources(1, 20000, null));
			Assert.AreEqual(16384, await FlowControl.RequestResources(1, 20000, null));
			Assert.AreEqual(16383, await FlowControl.RequestResources(1, 20000, null));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc7540 Node));
			Assert.AreEqual(0, Node.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream));
			Assert.AreEqual(1, Stream.StreamId);
		}

		[TestMethod]
		public async Task Test_09_AsymmetricSiblingStreams()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1, 10, 0, false);
			FlowControl.AddStreamForTest(3, 30, 0, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(6383, await FlowControl.RequestResources(1, 10000, null));

			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(9151, await FlowControl.RequestResources(3, 10000, null));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc7540 Node1));
			Assert.AreEqual(0, Node1.AvailableResources);
			Assert.IsTrue(FlowControl.TryGetPriorityNode(3, out PriorityNodeRfc7540 Node2));
			Assert.AreEqual(0, Node2.AvailableResources);
			Assert.AreEqual(1, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream1));
			Assert.AreEqual(1, Stream1.StreamId);
			Assert.IsTrue(FlowControl.TryGetStream(3, out Http2Stream Stream2));
			Assert.AreEqual(3, Stream2.StreamId);
		}

		[TestMethod]
		public async Task Test_10_AsymmetricChildDependencies()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1, 10, 0, false);
			FlowControl.AddStreamForTest(3, 30, 0, false);
			FlowControl.AddStreamForTest(5, 10, 3, false);
			FlowControl.AddStreamForTest(7, 30, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(6383, await FlowControl.RequestResources(1, 10000, null));

			Assert.AreEqual(10000, await FlowControl.RequestResources(5, 10000, null));
			Assert.AreEqual(2287, await FlowControl.RequestResources(5, 10000, null));

			Assert.AreEqual(10000, await FlowControl.RequestResources(7, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(7, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(7, 10000, null));
			Assert.AreEqual(6863, await FlowControl.RequestResources(7, 10000, null));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc7540 Node1));
			Assert.AreEqual(0, Node1.AvailableResources);
			Assert.IsTrue(FlowControl.TryGetPriorityNode(3, out PriorityNodeRfc7540 Node2));
			Assert.AreEqual(49151, Node2.AvailableResources);
			Assert.IsTrue(FlowControl.TryGetPriorityNode(5, out PriorityNodeRfc7540 Node3));
			Assert.AreEqual(0, Node3.AvailableResources);
			Assert.IsTrue(FlowControl.TryGetPriorityNode(7, out PriorityNodeRfc7540 Node4));
			Assert.AreEqual(0, Node4.AvailableResources);
			Assert.AreEqual(2, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream1));
			Assert.AreEqual(1, Stream1.StreamId);
			Assert.IsTrue(FlowControl.TryGetStream(3, out Http2Stream Stream2));
			Assert.AreEqual(3, Stream2.StreamId);
			Assert.IsTrue(FlowControl.TryGetStream(5, out Http2Stream Stream3));
			Assert.AreEqual(5, Stream3.StreamId);
			Assert.IsTrue(FlowControl.TryGetStream(7, out Http2Stream Stream4));
			Assert.AreEqual(7, Stream4.StreamId);
		}

		[TestMethod]
		public async Task Test_11_RemoveStream()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000, null));

			DateTime Start = DateTime.Now;
			_ = Task.Delay(1000).ContinueWith((_) => FlowControl.RemoveStream(1));

			Assert.AreEqual(-1, await FlowControl.RequestResources(1, 10000, null));

			Assert.IsTrue(DateTime.Now.Subtract(Start).TotalSeconds >= 1);

			Assert.IsFalse(FlowControl.TryGetStream(1, out _));
		}

		[TestMethod]
		public async Task Test_12_ResourcesAndRemovedStreams()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);
			FlowControl.AddStreamForTest(3);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));

			FlowControl.RemoveStream(1);

			Assert.AreEqual(65535, FlowControl.ReleaseConnectionResources(20000));
			Assert.AreEqual(-1, FlowControl.ReleaseStreamResources(1, 20000));

			Assert.IsFalse(FlowControl.TryGetPriorityNode(1, out _));
			Assert.AreEqual(65535, FlowControl.Root.AvailableResources);

			Assert.IsFalse(FlowControl.TryGetStream(1, out _));
			Assert.IsTrue(FlowControl.TryGetStream(3, out Http2Stream Stream2));
			Assert.AreEqual(3, Stream2.StreamId);
		}

		[TestMethod]
		public async Task Test_13_ChangingRelativeWindowSizes()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1);
			FlowControl.AddStreamForTest(3);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(2767, await FlowControl.RequestResources(1, 10000, null));

			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(2767, await FlowControl.RequestResources(3, 10000, null));

			FlowControl.ReleaseConnectionResources(32768);
			FlowControl.ReleaseStreamResources(1, 32768);
			FlowControl.RemoveStream(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.AreEqual(2768, await FlowControl.RequestResources(3, 10000, null));

			Assert.IsFalse(FlowControl.TryGetPriorityNode(1, out _));
			Assert.IsTrue(FlowControl.TryGetPriorityNode(3, out PriorityNodeRfc7540 Node2));
			Assert.AreEqual(0, Node2.AvailableResources);
			Assert.AreEqual(1, FlowControl.Root.AvailableResources);

			Assert.IsFalse(FlowControl.TryGetStream(1, out _));
			Assert.IsTrue(FlowControl.TryGetStream(3, out Http2Stream Stream2));
			Assert.AreEqual(3, Stream2.StreamId);
		}

		[TestMethod]
		public async Task Test_14_BasicExclusiveFlowControl()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1, 16, 0, true);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000, null));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000, null));

			Assert.IsTrue(FlowControl.TryGetPriorityNode(1, out PriorityNodeRfc7540 Node));
			Assert.AreEqual(0, Node.AvailableResources);
			Assert.AreEqual(0, FlowControl.Root.AvailableResources);

			Assert.IsTrue(FlowControl.TryGetStream(1, out Http2Stream Stream));
			Assert.AreEqual(1, Stream.StreamId);
		}

		[TestMethod]
		public async Task Test_15_EdgeSniff()
		{
			ConnectionSettings LocalSettings = new();
			ConnectionSettings RemoteSettings = new();
			using FlowControlRfc7540 FlowControl = new(LocalSettings, RemoteSettings, null);

			FlowControl.AddStreamForTest(1, 255, 0, true);  // .md
			
			// 1: TX, D4200, ES
			Assert.AreEqual(4200, await FlowControl.RequestResources(1, 4200, null));
			Assert.IsTrue(FlowControl.RemoveStream(1));

			FlowControl.AddStreamForTest(3, 255, 0, true);  // .cssx
			FlowControl.AddStreamForTest(5, 219, 3, true);  // .js
			FlowControl.AddStreamForTest(7, 182, 5, true);  // .png

			// 7: TX, EH, D16384, D1732 ES
			Assert.AreEqual(16384, await FlowControl.RequestResources(7, 18116, null));
			Assert.AreEqual(1732, await FlowControl.RequestResources(7, 1732, null));
			Assert.IsTrue(FlowControl.RemoveStream(7));

			// 5: TX, EH, D2165 ES
			Assert.AreEqual(2165, await FlowControl.RequestResources(5, 2165, null));
			Assert.IsTrue(FlowControl.RemoveStream(5));

			// 3: TX, EH
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000, null));
			Assert.IsTrue(FlowControl.RemoveStream(3));
		}

		/* 
		 * delayed release stream resources => trigger
		 * delayed release connection resources => trigger
		 * trigger sibling resources if sibling removed
		 * UpdatePriority + trigger
		 * UpdateSettings + trigger
		 * Exclusive reorder + trigger
		 * Changing priority to exclusive + trigger
		 * changing dependency and solve circular references
		 * changing window sizes if sibling removed + trigger
		 */
	}
}
